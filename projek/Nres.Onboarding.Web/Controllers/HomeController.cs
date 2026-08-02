using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<HomeController> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard. Anonymous visitors see the module map; signed-in users also get the
    /// counts that matter to them.
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (_currentUser.IsAuthenticated)
        {
            var userId = _currentUser.UserId!;

            ViewData["MyDraftCount"] = await _db.Submissions.CountAsync(
                s => s.ApplicantUserId == userId && s.Status == SubmissionStatus.Draft,
                cancellationToken);

            ViewData["MySubmittedCount"] = await _db.Submissions.CountAsync(
                s => s.ApplicantUserId == userId && s.Status == SubmissionStatus.Submitted,
                cancellationToken);

            if (_currentUser.IsInRole(ApplicationRoles.HrAdmin))
            {
                ViewData["ReviewQueueCount"] = await _db.Submissions.CountAsync(
                    s => s.ModuleCode == ModuleCodes.LaporDiri && s.Status == SubmissionStatus.Submitted,
                    cancellationToken);
            }
        }

        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
