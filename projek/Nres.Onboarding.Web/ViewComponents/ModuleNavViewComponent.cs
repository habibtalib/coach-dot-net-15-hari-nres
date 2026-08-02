using Microsoft.AspNetCore.Mvc;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewComponents;

/// <summary>
/// Renders the module navigation from every registered <see cref="IModuleDescriptorProvider"/>,
/// filtered to the roles the current user holds.
///
/// Adding a module means adding a descriptor file - _Layout.cshtml is never touched.
/// This is also why the Hari 15 master dashboard already knows about all four modules.
/// </summary>
public class ModuleNavViewComponent(IEnumerable<IModuleDescriptorProvider> providers)
    : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var modules = providers
            .Select(p => p.Describe())
            .Where(m => m.Roles.Any(r => UserClaimsPrincipal.IsInRole(r)))
            .OrderBy(m => m.Urutan)
            .ToList();

        return View(modules);
    }
}
