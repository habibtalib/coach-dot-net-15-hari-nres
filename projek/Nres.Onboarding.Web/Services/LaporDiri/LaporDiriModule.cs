using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.LaporDiri;

/// <summary>
/// Kumpulan 1 registers its own services here.
///
/// Program.cs calls AddLaporDiriModule() and never has to change again - which is
/// what stops four teams from editing the same file for eleven days.
/// See KOLABORASI.md §3.1.
/// </summary>
public static class LaporDiriModule
{
    public static IServiceCollection AddLaporDiriModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, LaporDiriModuleDescriptor>();

        // Module-specific services go here as the team writes them.
        return services;
    }
}
