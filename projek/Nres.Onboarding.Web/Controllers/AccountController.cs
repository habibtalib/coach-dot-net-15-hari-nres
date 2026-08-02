using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nres.Onboarding.Web.ViewModels;
using Nres.Onboarding.Web.ViewModels.LaporDiri;

namespace Nres.Onboarding.Web.Controllers;

/// <summary>
/// Sign-in and sign-out. Deliberately hand written instead of scaffolding the Identity
/// Razor Pages UI: the course is an MVC course, and a 60 line controller is far easier
/// to read and explain than thirty generated pages.
/// There is no self-registration action - in a government internal system accounts are
/// provisioned by a SystemAdmin, not created by whoever finds the URL.
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserName} signed in.", model.UserName);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Akaun dikunci sementara. Sila cuba sebentar lagi.");
            return View(model);
        }

        // One generic message for "no such user" and "wrong password": telling them apart
        // would let an attacker enumerate valid accounts.
        ModelState.AddModelError(string.Empty, "Nama pengguna atau kata laluan tidak sah.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    /// <summary>
    /// Only ever redirect to a URL inside this application. Redirecting to an arbitrary
    /// returnUrl is an open redirect and a standard phishing vector.
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
