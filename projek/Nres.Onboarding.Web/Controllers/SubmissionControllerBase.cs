using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Web.Controllers;

/// <summary>
/// Workflow actions shared by every module. Module controllers inherit this class and
/// write only what is specific to their module.
///
/// ⚠️ Do NOT copy Approve/Reject into your module controller. Four modules need an
/// approve button; the logic should exist once. If your module needs different
/// behaviour, there are three correct answers depending on the case:
///
///   · Kumpulan 2 - allocation before approval  →  override + call base.Approve()
///   · Kumpulan 3 - a stage the base has no concept of  →  add a NEW action
///   · Kumpulan 4 - status change must sit inside a transaction  →  do not use base
///
/// Anything else is a `shared` issue, not a local decision. See KOLABORASI.md §4.
/// </summary>
[Authorize]
public abstract class SubmissionControllerBase(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications) : Controller
{
    protected readonly ApplicationDbContext Db = db;
    protected readonly IWorkflowService Workflow = workflow;
    protected readonly INotificationService Notifications = notifications;

    /// <summary>Module prefix - each subclass supplies it. See <see cref="ModuleCodes"/>.</summary>
    protected abstract string ModuleCode { get; }

    /// <summary>Role allowed to approve in this module.</summary>
    protected abstract string AdminRole { get; }

    /// <summary>
    /// Approve a submission. Marked <c>virtual</c> on purpose: modules that must also
    /// allocate something (a parking lot, a sticker serial) override this, do their
    /// allocation, then call <c>base.Approve(...)</c> so the status transition, audit
    /// write and notification stay defined in exactly one place.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Approve(int id, string? remarks)
    {
        if (!User.IsInRole(AdminRole)) return Forbid();

        var submission = await Db.Submissions
            .FirstOrDefaultAsync(s => s.Id == id && s.ModuleCode == ModuleCode);

        if (submission is null) return NotFound();

        await Workflow.TransitionAsync(submission, SubmissionStatus.AdminApproved,
            AuditActions.Approved, remarks);

        await Notifications.NotifyAsync(submission.ApplicantUserId,
            $"Permohonan {submission.ReferenceNo} diluluskan",
            remarks ?? string.Empty);

        return RedirectToAction("Details", new { id });
    }

    /// <summary>
    /// Reject a submission. The rejection reason is mandatory and enforced HERE, so
    /// all four modules behave the same way without four teams agreeing on it.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Reject(int id, string remarks)
    {
        if (!User.IsInRole(AdminRole)) return Forbid();

        if (string.IsNullOrWhiteSpace(remarks))
        {
            TempData["Ralat"] = "Sebab penolakan wajib diisi.";
            return RedirectToAction("Details", new { id });
        }

        var submission = await Db.Submissions
            .FirstOrDefaultAsync(s => s.Id == id && s.ModuleCode == ModuleCode);

        if (submission is null) return NotFound();

        await Workflow.TransitionAsync(submission, SubmissionStatus.Rejected,
            AuditActions.Rejected, remarks);

        await Notifications.NotifyAsync(submission.ApplicantUserId,
            $"Permohonan {submission.ReferenceNo} ditolak", remarks);

        return RedirectToAction("Details", new { id });
    }
}
