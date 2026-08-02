using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

/// <summary>
/// Form model for creating a Lapor Diri application.
/// Views bind to this, never to <see cref="Models.OfficerReportingApplication"/>: binding
/// to the entity would let a crafted request set fields the form never showed
/// (over-posting), and it ties the database shape to the screen shape forever.
/// </summary>
public class OfficerReportingCreateViewModel
{
    [Required(ErrorMessage = "Nama penuh wajib diisi.")]
    [StringLength(200)]
    [Display(Name = "Nama Penuh")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "No. Kad Pengenalan wajib diisi.")]
    [StringLength(20, MinimumLength = 6)]
    [Display(Name = "No. Kad Pengenalan")]
    public string IdentityNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Emel wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format emel tidak sah.")]
    [StringLength(200)]
    [Display(Name = "Emel")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "No. telefon wajib diisi.")]
    [StringLength(30, ErrorMessage = "No. telefon tidak boleh melebihi 30 aksara.")]
    [Phone(ErrorMessage = "Format no. telefon tidak sah.")]
    [Display(Name = "No. Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Sila pilih jabatan.")]
    [Display(Name = "Jabatan")]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sila pilih jawatan.")]
    [Display(Name = "Jawatan")]
    public int PositionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sila pilih gred.")]
    [Display(Name = "Gred")]
    public int GradeId { get; set; }

    [Required(ErrorMessage = "Tarikh lapor diri wajib diisi.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarikh Lapor Diri")]
    public DateTime ReportingDate { get; set; } = DateTime.Today;

    [StringLength(200)]
    [Display(Name = "Agensi Terdahulu")]
    public string? PreviousAgency { get; set; }

    [StringLength(200)]
    [Display(Name = "Hubungan Kecemasan")]
    public string? EmergencyContact { get; set; }

    /// <summary>
    /// Optional supporting document. Validated by the controller against
    /// <see cref="Services.FileUploadRules"/> - an <c>IFormFile</c> cannot be checked
    /// properly with data annotations alone.
    /// </summary>
    [Display(Name = "Dokumen Sokongan")]
    public IFormFile? Attachment { get; set; }

    // Dropdown sources. Populated by the controller before the view renders and after
    // every failed post - a re-displayed form with empty dropdowns is a classic bug.
    public IEnumerable<SelectListItem> Departments { get; set; } = [];

    public IEnumerable<SelectListItem> Positions { get; set; } = [];

    public IEnumerable<SelectListItem> Grades { get; set; } = [];
}
