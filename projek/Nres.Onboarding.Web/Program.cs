using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.Services.LaporDiri;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Database
// ---------------------------------------------------------------------------
// SQLite keeps the training setup to zero installation. Moving to SQL Server is a
// one-line change here (UseSqlite -> UseSqlServer) plus a new connection string.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException(
                           "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// The SQLite file and the uploads folder both live under App_Data, which sits OUTSIDE
// wwwroot and is therefore never served as a static file. Create it before the data
// layer touches it, otherwise SQLite fails on a missing directory.
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads"));

// ---------------------------------------------------------------------------
// Authentication and authorisation
// ---------------------------------------------------------------------------
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Password rules are stated explicitly rather than left to the defaults so the
        // class can see and discuss them. RequireConfirmedAccount is off and the demo
        // accounts are pre-confirmed because there is no mail server in a lab - both are
        // training conveniences, not recommendations for production.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;

        // Lockout still behaves like production: brute force protection is not optional.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ---------------------------------------------------------------------------
// Application services
// ---------------------------------------------------------------------------
// Scoped: each of these either uses the DbContext (also scoped) or reads the current
// request, so their lifetime must match a single request.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IReferenceNumberService, ReferenceNumberService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// Stateless and thread safe, so a singleton is enough.
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<INotificationService, ConsoleNotificationService>();

// ==========================================================================
// PENDAFTARAN MODUL
// Each team registers its own services inside Services/<Modul>/<Modul>Module.cs.
// The only change any team makes to THIS file is uncommenting its own line,
// once, on Hari 4 - under trainer supervision.
//
// ⚠️ FAIL INI BEKU. Add services in your module file, NOT here.
//    See KOLABORASI.md §3.1.
// ==========================================================================
builder.Services.AddLaporDiriModule();      // Kumpulan 1
// builder.Services.AddAksesModule();       // Kumpulan 2
// builder.Services.AddAkaunModule();       // Kumpulan 3
// builder.Services.AddAsetModule();        // Kumpulan 4

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Seeding
// ---------------------------------------------------------------------------
// A scope is required: the DbContext and UserManager are scoped services and the
// application root provider cannot resolve them directly.
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

// ---------------------------------------------------------------------------
// HTTP pipeline - order matters
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. See https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Authentication answers "who are you?" and must run before authorisation,
// which answers "are you allowed?".
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
