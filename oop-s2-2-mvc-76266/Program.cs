using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;
using OopS22Mvc76266.Web.Data.Seeding;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodSafetyInspectionTracker")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.Use(async (context, next) =>
{
    var userName = context.User?.Identity?.IsAuthenticated == true
        ? context.User.Identity?.Name ?? "UnknownUser"
        : "Anonymous";

    using (LogContext.PushProperty("UserName", userName))
    {
        await next();
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);

    string[] roles = { "Admin", "Inspector", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Log.Information("Role created: {RoleName}", role);
        }
    }

    var adminEmail = "admin@test.com";
    var adminPassword = "Password123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            Log.Information("Seed user created: {Email}", adminEmail);
        }
        else
        {
            Log.Warning("Failed to create seed admin user: {Email}", adminEmail);
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        Log.Information("Role assigned: {Email} -> Admin", adminEmail);
    }

    var inspectorEmail = "inspector@test.com";
    var inspectorPassword = "Password123!";

    var inspectorUser = await userManager.FindByEmailAsync(inspectorEmail);

    if (inspectorUser == null)
    {
        inspectorUser = new IdentityUser
        {
            UserName = inspectorEmail,
            Email = inspectorEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(inspectorUser, inspectorPassword);

        if (result.Succeeded)
        {
            Log.Information("Seed user created: {Email}", inspectorEmail);
        }
        else
        {
            Log.Warning("Failed to create seed inspector user: {Email}", inspectorEmail);
        }
    }

    if (!await userManager.IsInRoleAsync(inspectorUser, "Inspector"))
    {
        await userManager.AddToRoleAsync(inspectorUser, "Inspector");
        Log.Information("Role assigned: {Email} -> Inspector", inspectorEmail);
    }

    var viewerEmail = "viewer@test.com";
    var viewerPassword = "Password123!";

    var viewerUser = await userManager.FindByEmailAsync(viewerEmail);

    if (viewerUser == null)
    {
        viewerUser = new IdentityUser
        {
            UserName = viewerEmail,
            Email = viewerEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(viewerUser, viewerPassword);

        if (result.Succeeded)
        {
            Log.Information("Seed user created: {Email}", viewerEmail);
        }
        else
        {
            Log.Warning("Failed to create seed viewer user: {Email}", viewerEmail);
        }
    }

    if (!await userManager.IsInRoleAsync(viewerUser, "Viewer"))
    {
        await userManager.AddToRoleAsync(viewerUser, "Viewer");
        Log.Information("Role assigned: {Email} -> Viewer", viewerEmail);
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();