namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Metadata describing one module: how it appears in navigation and which roles
/// may see it.
/// </summary>
/// <param name="Code">Module prefix, see <see cref="ModuleCodes"/>.</param>
/// <param name="Nama">Display name (Bahasa Melayu).</param>
/// <param name="Controller">Controller that owns the module landing page.</param>
/// <param name="Ikon">Bootstrap icon class.</param>
/// <param name="Roles">Roles allowed to see this module in navigation.</param>
/// <param name="Urutan">Sort order in the navigation bar.</param>
public record ModuleDescriptor(
    string Code,
    string Nama,
    string Controller,
    string Ikon,
    string[] Roles,
    int Urutan);

/// <summary>
/// Implemented once per module, inside that module's own folder.
///
/// This is the anti-conflict pattern for navigation: <c>ModuleNavViewComponent</c>
/// collects every registered provider, so adding a module means ADDING A FILE -
/// nobody edits <c>_Layout.cshtml</c>. See KOLABORASI.md §3.3.
/// </summary>
public interface IModuleDescriptorProvider
{
    ModuleDescriptor Describe();
}
