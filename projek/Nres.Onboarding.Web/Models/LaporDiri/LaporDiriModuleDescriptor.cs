using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.LaporDiri;

/// <summary>
/// Kumpulan 1's navigation entry. Collected automatically by
/// <c>ModuleNavViewComponent</c> - nobody edits _Layout.cshtml.
/// </summary>
public class LaporDiriModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.LaporDiri,
        Nama: "Lapor Diri",
        Controller: "OfficerReporting",
        Ikon: "bi-person-plus",
        Roles: [ApplicationRoles.Applicant, ApplicationRoles.HrAdmin,
                ApplicationRoles.SystemAdmin],
        Urutan: 1);
}
