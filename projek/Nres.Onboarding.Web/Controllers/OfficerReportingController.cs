using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.LaporDiri;

namespace Nres.Onboarding.Web.Controllers;

/// <summary>
/// Module 1 - Lapor Diri (officer reporting).
/// Implements the universal flow every module in this system repeats:
/// Form -&gt; Validation -&gt; Draft -&gt; Submit -&gt; Review -&gt; Approve/Reject -&gt; Audit.
/// For this module the approval route is a single step performed by an <c>HrAdmin</c>,
/// which maps onto <see cref="SubmissionStatus.AdminApproved"/>.
/// </summary>
[Authorize]
public class OfficerReportingController : Controller
{
    /// <summary>Buttons post this value to say "keep it as a draft".</summary>
    private const string CommandSaveDraft = "draft";

    /// <summary>Buttons post this value to say "save and send for review".</summary>
    private const string CommandSubmit = "submit";

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IReferenceNumberService _referenceNumbers;
    private readonly IFileStorageService _fileStorage;
    private readonly IAuditLogService _audit;
    private readonly INotificationService _notifications;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<OfficerReportingController> _logger;

    public OfficerReportingController(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IReferenceNumberService referenceNumbers,
        IFileStorageService fileStorage,
        IAuditLogService audit,
        INotificationService notifications,
        UserManager<IdentityUser> userManager,
        ILogger<OfficerReportingController> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _referenceNumbers = referenceNumbers;
        _fileStorage = fileStorage;
        _audit = audit;
        _notifications = notifications;
        _userManager = userManager;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // Applicant screens
    // -----------------------------------------------------------------------

    /// <summary>The signed-in user's own Lapor Diri applications.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();

        var items = await QueryListItems()
            .Where(x => x.Submission!.ApplicantUserId == userId)
            .OrderByDescending(x => x.Submission!.CreatedAt)
            .Select(ToListItem())
            .ToListAsync(cancellationToken);

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new OfficerReportingCreateViewModel();
        var userId = RequireUserId();

        // Pre-fill from the staff profile so the applicant does not retype what the
        // system already knows. Everything stays editable.
        var profile = await _db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is not null)
        {
            model.FullName = profile.FullName;
            model.IdentityNo = profile.IdentityNo;
            model.Phone = profile.Phone ?? string.Empty;
            model.DepartmentId = profile.DepartmentId ?? 0;
            model.PositionId = profile.PositionId ?? 0;
            model.GradeId = profile.GradeId ?? 0;
            model.Email = (await _userManager.FindByIdAsync(profile.UserId))?.Email ?? string.Empty;
        }

        await LoadLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        OfficerReportingCreateViewModel model,
        string command,
        CancellationToken cancellationToken)
    {
        ValidateAttachmentIfPresent(model.Attachment);

        if (!ModelState.IsValid)
        {
            // Repopulating the dropdowns is mandatory - the posted form only sent ids.
            await LoadLookupsAsync(model, cancellationToken);
            return View(model);
        }

        var userId = RequireUserId();

        var submission = new Submission
        {
            ModuleCode = ModuleCodes.LaporDiri,
            ApplicantUserId = userId,
            Status = SubmissionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var application = new OfficerReportingApplication
        {
            Submission = submission,
            FullName = model.FullName,
            IdentityNo = model.IdentityNo,
            Email = model.Email,
            Phone = model.Phone,
            DepartmentId = model.DepartmentId,
            PositionId = model.PositionId,
            GradeId = model.GradeId,
            ReportingDate = model.ReportingDate.Date,
            PreviousAgency = model.PreviousAgency,
            EmergencyContact = model.EmergencyContact
        };

        _db.Set<OfficerReportingApplication>().Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(submission.Id, AuditActions.Created, ct: cancellationToken);

        if (model.Attachment is not null)
        {
            await StoreAttachmentAsync(submission.Id, model.Attachment, cancellationToken);
        }

        if (!string.Equals(command, CommandSubmit, StringComparison.OrdinalIgnoreCase))
        {
            TempData["StatusMessage"] = "Draf berjaya disimpan.";
            return RedirectToAction(nameof(Details), new { id = submission.Id });
        }

        return await SubmitInternalAsync(submission.Id, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        if (!CanView(application.Submission!))
        {
            return Forbid();
        }

        if (application.Submission!.Status != SubmissionStatus.Draft)
        {
            // Business rule: once submitted the record is frozen. Reopening it is an
            // administrator action, which this module does not expose yet.
            TempData["ErrorMessage"] = "Permohonan yang telah dihantar tidak boleh disunting.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new OfficerReportingEditViewModel
        {
            SubmissionId = application.SubmissionId,
            ReferenceNo = application.Submission.ReferenceNo,
            Status = application.Submission.Status,
            FullName = application.FullName,
            IdentityNo = application.IdentityNo,
            Email = application.Email,
            Phone = application.Phone,
            DepartmentId = application.DepartmentId,
            PositionId = application.PositionId,
            GradeId = application.GradeId,
            ReportingDate = application.ReportingDate,
            PreviousAgency = application.PreviousAgency,
            EmergencyContact = application.EmergencyContact,
            Attachments = MapAttachments(application.Submission.Attachments)
        };

        await LoadLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Edit(
        int id,
        OfficerReportingEditViewModel model,
        string command,
        CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        if (!CanView(application.Submission!))
        {
            return Forbid();
        }

        if (application.Submission!.Status != SubmissionStatus.Draft)
        {
            TempData["ErrorMessage"] = "Permohonan yang telah dihantar tidak boleh disunting.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ValidateAttachmentIfPresent(model.Attachment);

        if (!ModelState.IsValid)
        {
            model.SubmissionId = application.SubmissionId;
            model.ReferenceNo = application.Submission.ReferenceNo;
            model.Status = application.Submission.Status;
            model.Attachments = MapAttachments(application.Submission.Attachments);
            await LoadLookupsAsync(model, cancellationToken);
            return View(model);
        }

        application.FullName = model.FullName;
        application.IdentityNo = model.IdentityNo;
        application.Email = model.Email;
        application.Phone = model.Phone;
        application.DepartmentId = model.DepartmentId;
        application.PositionId = model.PositionId;
        application.GradeId = model.GradeId;
        application.ReportingDate = model.ReportingDate.Date;
        application.PreviousAgency = model.PreviousAgency;
        application.EmergencyContact = model.EmergencyContact;

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(id, AuditActions.Updated, ct: cancellationToken);

        if (model.Attachment is not null)
        {
            await StoreAttachmentAsync(id, model.Attachment, cancellationToken);
        }

        if (!string.Equals(command, CommandSubmit, StringComparison.OrdinalIgnoreCase))
        {
            TempData["StatusMessage"] = "Draf berjaya dikemas kini.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return await SubmitInternalAsync(id, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken, includeLookups: true);

        if (application is null)
        {
            return NotFound();
        }

        var submission = application.Submission!;

        if (!CanView(submission))
        {
            return Forbid();
        }

        var isOwner = submission.ApplicantUserId == RequireUserId();

        var model = new OfficerReportingDetailsViewModel
        {
            SubmissionId = submission.Id,
            ReferenceNo = submission.ReferenceNo,
            Status = submission.Status,
            CreatedAt = submission.CreatedAt,
            SubmittedAt = submission.SubmittedAt,
            CompletedAt = submission.CompletedAt,
            ApplicantDisplayName = await ResolveUserDisplayNameAsync(submission.ApplicantUserId, cancellationToken),
            FullName = application.FullName,
            IdentityNo = application.IdentityNo,
            Email = application.Email,
            Phone = application.Phone,
            DepartmentName = application.Department?.Name ?? string.Empty,
            PositionName = application.Position?.Name ?? string.Empty,
            GradeName = application.Grade?.Name ?? string.Empty,
            ReportingDate = application.ReportingDate,
            PreviousAgency = application.PreviousAgency,
            EmergencyContact = application.EmergencyContact,
            Attachments = MapAttachments(submission.Attachments),
            AuditTrail = await MapAuditTrailAsync(submission.AuditLogs, cancellationToken),
            CanEdit = isOwner && submission.Status == SubmissionStatus.Draft,
            CanSubmit = isOwner && submission.Status == SubmissionStatus.Draft,
            CanReview = _currentUser.IsInRole(ApplicationRoles.HrAdmin)
                        && submission.Status == SubmissionStatus.Submitted
        };

        return View(model);
    }

    /// <summary>Adds one more supporting document to a draft.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachment(
        int id,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == id && s.ModuleCode == ModuleCodes.LaporDiri, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        if (!CanView(submission) || submission.ApplicantUserId != RequireUserId())
        {
            return Forbid();
        }

        if (submission.Status != SubmissionStatus.Draft)
        {
            TempData["ErrorMessage"] = "Lampiran hanya boleh ditambah semasa status draf.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!FileUploadRules.TryValidate(file, out var error))
        {
            TempData["ErrorMessage"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        await StoreAttachmentAsync(id, file!, cancellationToken);

        TempData["StatusMessage"] = "Lampiran berjaya dimuat naik.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Streams an attachment back to the browser.
    /// Files are not statically servable, so this action is the only way to read them -
    /// and it authorises the caller before opening anything.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(int id, CancellationToken cancellationToken)
    {
        var attachment = await _db.Attachments
            .AsNoTracking()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attachment?.Submission is null)
        {
            return NotFound();
        }

        if (!CanView(attachment.Submission))
        {
            return Forbid();
        }

        Stream stream;

        try
        {
            // Only the stored (server generated) name is passed in. The original name is
            // used purely as the download name shown to the user.
            stream = _fileStorage.OpenRead(attachment.SubmissionId, attachment.StoredFileName);
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning(
                "Attachment {AttachmentId} is missing on disk ({StoredFileName}).",
                attachment.Id,
                attachment.StoredFileName);
            return NotFound();
        }

        return File(stream, attachment.ContentType, attachment.OriginalFileName);
    }

    /// <summary>Sends a draft for HR review.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, CancellationToken cancellationToken)
    {
        return await SubmitInternalAsync(id, cancellationToken);
    }

    // -----------------------------------------------------------------------
    // HR review screens
    // -----------------------------------------------------------------------

    /// <summary>Queue of applications waiting for an HR decision.</summary>
    [HttpGet]
    [Authorize(Roles = ApplicationRoles.HrAdmin)]
    public async Task<IActionResult> Review(SubmissionStatus? status, CancellationToken cancellationToken)
    {
        // Default view is the work that actually needs a decision.
        var filter = status ?? SubmissionStatus.Submitted;

        var items = await QueryListItems()
            .Where(x => x.Submission!.Status == filter)
            .OrderBy(x => x.Submission!.SubmittedAt)
            .Select(ToListItem())
            .ToListAsync(cancellationToken);

        ViewData["StatusFilter"] = filter;
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ApplicationRoles.HrAdmin)]
    public async Task<IActionResult> Approve(int id, string? remarks, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var submission = application.Submission!;

        if (submission.Status != SubmissionStatus.Submitted)
        {
            TempData["ErrorMessage"] = "Hanya permohonan berstatus 'Submitted' boleh diluluskan.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // HrAdmin approval is the final decision for this module, so the submission goes
        // straight to AdminApproved (there is no supervisor stage in Lapor Diri).
        submission.Status = SubmissionStatus.AdminApproved;
        submission.CompletedAt = DateTime.UtcNow;

        CloseApprovalStep(submission, ApprovalDecision.Approved, remarks);

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(id, AuditActions.Approved, remarks: remarks, ct: cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _notifications.NotifyAsync(
            application.Email,
            $"Lapor Diri {submission.ReferenceNo} diluluskan",
            $"Permohonan lapor diri anda ({submission.ReferenceNo}) telah diluluskan oleh Sumber Manusia.");

        TempData["StatusMessage"] = $"Permohonan {submission.ReferenceNo} telah diluluskan.";
        return RedirectToAction(nameof(Review));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ApplicationRoles.HrAdmin)]
    public async Task<IActionResult> Reject(int id, string? reason, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var submission = application.Submission!;

        if (submission.Status != SubmissionStatus.Submitted)
        {
            TempData["ErrorMessage"] = "Hanya permohonan berstatus 'Submitted' boleh ditolak.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // A rejection without a reason is unusable for the applicant and indefensible in
        // an audit, so the reason is enforced on the server - not only in the browser.
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Sebab penolakan wajib diisi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var trimmedReason = reason.Trim();

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        submission.Status = SubmissionStatus.Rejected;
        submission.CompletedAt = DateTime.UtcNow;

        CloseApprovalStep(submission, ApprovalDecision.Rejected, trimmedReason);

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(id, AuditActions.Rejected, remarks: trimmedReason, ct: cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _notifications.NotifyAsync(
            application.Email,
            $"Lapor Diri {submission.ReferenceNo} ditolak",
            $"Permohonan lapor diri anda ({submission.ReferenceNo}) ditolak. Sebab: {trimmedReason}");

        TempData["StatusMessage"] = $"Permohonan {submission.ReferenceNo} telah ditolak.";
        return RedirectToAction(nameof(Review));
    }

    // -----------------------------------------------------------------------
    // Internals
    // -----------------------------------------------------------------------

    /// <summary>
    /// The one place that moves a draft to <see cref="SubmissionStatus.Submitted"/>.
    /// Reached from the Submit button on the list and detail screens, and from the
    /// "Hantar" button on the create/edit forms.
    /// </summary>
    private async Task<IActionResult> SubmitInternalAsync(int id, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var submission = application.Submission!;

        if (submission.ApplicantUserId != RequireUserId())
        {
            return Forbid();
        }

        if (submission.Status != SubmissionStatus.Draft)
        {
            TempData["ErrorMessage"] = "Hanya draf boleh dihantar.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var attachmentCount = await _db.Attachments.CountAsync(a => a.SubmissionId == id, cancellationToken);

        // Submission validation is stricter than draft validation: a draft may be saved
        // half finished, but the moment it reaches HR it must be complete.
        var problems = ValidateForSubmission(application, attachmentCount);

        if (problems.Count > 0)
        {
            TempData["ErrorMessage"] = "Permohonan belum lengkap: " + string.Join(" ", problems);
            return RedirectToAction(nameof(Details), new { id });
        }

        // The reference number is generated and persisted inside one transaction, so a
        // failure part-way through cannot burn a number.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var referenceNo = await _referenceNumbers.GenerateAsync(ModuleCodes.LaporDiri, cancellationToken);

        submission.ReferenceNo = referenceNo;
        submission.Status = SubmissionStatus.Submitted;
        submission.SubmittedAt = DateTime.UtcNow;

        // Single step route for this module. Later modules add more rows here.
        if (submission.ApprovalSteps.Count == 0)
        {
            submission.ApprovalSteps.Add(new ApprovalStep
            {
                StepOrder = 1,
                RoleRequired = ApplicationRoles.HrAdmin,
                Decision = ApprovalDecision.Pending
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(id, AuditActions.Submitted, remarks: $"Nombor rujukan {referenceNo}.", ct: cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await NotifySubmittedAsync(application, referenceNo);

        TempData["StatusMessage"] = $"Permohonan berjaya dihantar. Nombor rujukan: {referenceNo}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Rules that only apply at submission time. Returns a list of human readable problems.
    /// At least one supporting document is required: a Lapor Diri without the appointment
    /// or transfer letter cannot be processed by HR.
    /// </summary>
    private static List<string> ValidateForSubmission(
        OfficerReportingApplication application,
        int attachmentCount)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(application.FullName))
        {
            problems.Add("Nama penuh belum diisi.");
        }

        if (string.IsNullOrWhiteSpace(application.IdentityNo))
        {
            problems.Add("No. Kad Pengenalan belum diisi.");
        }

        if (string.IsNullOrWhiteSpace(application.Email))
        {
            problems.Add("Emel belum diisi.");
        }

        if (string.IsNullOrWhiteSpace(application.Phone))
        {
            problems.Add("No. telefon belum diisi.");
        }

        if (application.DepartmentId <= 0)
        {
            problems.Add("Jabatan belum dipilih.");
        }

        if (application.PositionId <= 0)
        {
            problems.Add("Jawatan belum dipilih.");
        }

        if (application.GradeId <= 0)
        {
            problems.Add("Gred belum dipilih.");
        }

        if (application.ReportingDate == default)
        {
            problems.Add("Tarikh lapor diri belum diisi.");
        }

        if (attachmentCount == 0)
        {
            problems.Add("Sekurang-kurangnya satu dokumen sokongan diperlukan.");
        }

        return problems;
    }

    private void CloseApprovalStep(Submission submission, ApprovalDecision decision, string? remarks)
    {
        var step = submission.ApprovalSteps
            .Where(s => s.Decision == ApprovalDecision.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        if (step is null)
        {
            return;
        }

        step.Decision = decision;
        step.DecidedByUserId = _currentUser.UserId;
        step.DecidedAt = DateTime.UtcNow;
        step.Remarks = remarks;
    }

    private async Task NotifySubmittedAsync(OfficerReportingApplication application, string referenceNo)
    {
        await _notifications.NotifyAsync(
            application.Email,
            $"Lapor Diri {referenceNo} diterima",
            $"Permohonan lapor diri anda telah dihantar dan menunggu semakan Sumber Manusia.");

        // Everyone who can act on the queue is told there is new work waiting.
        var reviewers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.HrAdmin);

        foreach (var reviewer in reviewers.Where(r => !string.IsNullOrWhiteSpace(r.Email)))
        {
            await _notifications.NotifyAsync(
                reviewer.Email!,
                $"Semakan diperlukan: {referenceNo}",
                $"Permohonan lapor diri {referenceNo} daripada {application.FullName} menunggu semakan.");
        }
    }

    private async Task StoreAttachmentAsync(int submissionId, IFormFile file, CancellationToken cancellationToken)
    {
        var stored = await _fileStorage.SaveAsync(submissionId, file, cancellationToken);

        _db.Attachments.Add(new Attachment
        {
            SubmissionId = submissionId,
            OriginalFileName = stored.OriginalFileName,
            StoredFileName = stored.StoredFileName,
            ContentType = stored.ContentType,
            SizeBytes = stored.FileSizeBytes,
            UploadedByUserId = RequireUserId()
        });

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            submissionId,
            AuditActions.AttachmentUploaded,
            remarks: stored.OriginalFileName,
            ct: cancellationToken);
    }

    private void ValidateAttachmentIfPresent(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return;
        }

        if (!FileUploadRules.TryValidate(file, out var error))
        {
            ModelState.AddModelError(nameof(OfficerReportingCreateViewModel.Attachment), error);
        }
    }

    // No Include() here on purpose: the query ends in a projection, so EF builds exactly
    // the joins the projection needs and an Include would simply be ignored.
    private IQueryable<OfficerReportingApplication> QueryListItems() =>
        _db.Set<OfficerReportingApplication>()
            .AsNoTracking()
            .Where(x => x.Submission!.ModuleCode == ModuleCodes.LaporDiri);

    private static System.Linq.Expressions.Expression<
        Func<OfficerReportingApplication, OfficerReportingListItemViewModel>> ToListItem() =>
        x => new OfficerReportingListItemViewModel
        {
            SubmissionId = x.SubmissionId,
            ReferenceNo = x.Submission!.ReferenceNo,
            FullName = x.FullName,
            DepartmentName = x.Department!.Name,
            ReportingDate = x.ReportingDate,
            Status = x.Submission.Status,
            CreatedAt = x.Submission.CreatedAt,
            SubmittedAt = x.Submission.SubmittedAt,
            AttachmentCount = x.Submission.Attachments.Count
        };

    private Task<OfficerReportingApplication?> LoadApplicationAsync(
        int submissionId,
        CancellationToken cancellationToken,
        bool includeLookups = false)
    {
        var query = _db.Set<OfficerReportingApplication>()
            .Include(x => x.Submission!).ThenInclude(s => s.Attachments)
            .Include(x => x.Submission!).ThenInclude(s => s.AuditLogs)
            .Include(x => x.Submission!).ThenInclude(s => s.ApprovalSteps)
            .AsQueryable();

        if (includeLookups)
        {
            query = query
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Include(x => x.Grade);
        }

        return query.FirstOrDefaultAsync(
            x => x.SubmissionId == submissionId && x.Submission!.ModuleCode == ModuleCodes.LaporDiri,
            cancellationToken);
    }

    /// <summary>
    /// An applicant only ever sees their own records. HR and system administrators see
    /// everything because reviewing is their job.
    /// </summary>
    private bool CanView(Submission submission) =>
        submission.ApplicantUserId == RequireUserId()
        || _currentUser.IsInRole(ApplicationRoles.HrAdmin)
        || _currentUser.IsInRole(ApplicationRoles.SystemAdmin);

    private string RequireUserId() =>
        _currentUser.UserId
        ?? throw new InvalidOperationException("An authenticated user is required for this action.");

    private static IReadOnlyList<AttachmentViewModel> MapAttachments(IEnumerable<Attachment> attachments) =>
        attachments
            .OrderBy(a => a.UploadedAt)
            .Select(a => new AttachmentViewModel
            {
                Id = a.Id,
                OriginalFileName = a.OriginalFileName,
                FileSizeBytes = a.SizeBytes,
                UploadedAt = a.UploadedAt
            })
            .ToList();

    private async Task<IReadOnlyList<AuditLogViewModel>> MapAuditTrailAsync(
        IEnumerable<AuditLog> logs,
        CancellationToken cancellationToken)
    {
        var ordered = logs.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id).ToList();
        var actorIds = ordered.Select(l => l.ActorUserId).Distinct().ToList();

        // One query for all actors instead of one per row.
        var actors = await _db.Users
            .AsNoTracking()
            .Where(u => actorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id, cancellationToken);

        return ordered
            .Select(l => new AuditLogViewModel
            {
                Action = l.Action,
                ActorDisplayName = actors.TryGetValue(l.ActorUserId, out var name) ? name : l.ActorUserId,
                Remarks = l.Remarks,
                CreatedAt = l.CreatedAt
            })
            .ToList();
    }

    private async Task<string> ResolveUserDisplayNameAsync(string userId, CancellationToken cancellationToken)
    {
        var profileName = await _db.UserProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(profileName))
        {
            return profileName;
        }

        var userName = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return userName ?? userId;
    }

    private async Task LoadLookupsAsync(
        OfficerReportingCreateViewModel model,
        CancellationToken cancellationToken)
    {
        var departments = await _db.LookupDepartments
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync(cancellationToken);

        var positions = await _db.LookupPositions
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync(cancellationToken);

        var grades = await _db.LookupGrades
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync(cancellationToken);

        // Mapped in memory: SelectListItem is a UI type and has no business inside a
        // LINQ-to-SQL expression tree.
        model.Departments = departments
            .Select(l => new SelectListItem(l.Name, l.Id.ToString(), l.Id == model.DepartmentId))
            .ToList();

        model.Positions = positions
            .Select(l => new SelectListItem(l.Name, l.Id.ToString(), l.Id == model.PositionId))
            .ToList();

        model.Grades = grades
            .Select(l => new SelectListItem(l.Name, l.Id.ToString(), l.Id == model.GradeId))
            .ToList();
    }
}
