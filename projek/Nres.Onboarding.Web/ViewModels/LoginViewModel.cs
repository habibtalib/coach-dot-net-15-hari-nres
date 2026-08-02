using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.ViewModels;

/// <summary>Sign-in form. The password is never stored on the model beyond the request.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Nama pengguna atau emel wajib diisi.")]
    [Display(Name = "Emel / Nama Pengguna")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kata laluan wajib diisi.")]
    [DataType(DataType.Password)]
    [Display(Name = "Kata Laluan")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ingat saya")]
    public bool RememberMe { get; set; }

    /// <summary>Where to go after a successful sign-in. Validated as a local URL before use.</summary>
    public string? ReturnUrl { get; set; }
}
