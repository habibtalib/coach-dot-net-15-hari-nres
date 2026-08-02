using Microsoft.AspNetCore.Mvc.Rendering;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels;

/// <summary>
/// Backing model for the shared <c>_FilterBar</c> partial.
///
/// Module list view models inherit this so every admin screen filters the same way.
/// Modules add their own extra fields on the subclass.
/// </summary>
public class FilterBarViewModel
{
    public SubmissionStatus? Status { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? DariTarikh { get; set; }
    public DateTime? HinggaTarikh { get; set; }
    public string? Carian { get; set; }

    // --- Paging ---
    public int Halaman { get; set; } = 1;
    public int SaizHalaman { get; set; } = 20;
    public int JumlahRekod { get; set; }
    public int JumlahHalaman => (int)Math.Ceiling(JumlahRekod / (double)SaizHalaman);

    /// <summary>Populated by the controller; empty means the department filter is hidden.</summary>
    public IEnumerable<SelectListItem> Departments { get; set; } = [];
}
