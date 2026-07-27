using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Remove default logging providers
builder.Logging.ClearProviders();
// Add NLog
//builder.Logging.AddDebug();
builder.Host.UseNLog();

// Register MVC services
builder.Services.AddControllersWithViews(
    // Add a global authorization filter to require authentication for all controllers
    config =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        config.Filters.Add(new AuthorizeFilter(policy));
    });

// Register your custom service
builder.Services.AddDbContextPool<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmployeeDBConnection")));

// Register identity services
// Add claim based authorization policies
builder.Services.AddAuthorizationBuilder()
                .AddPolicy("CreateRolePolicy", policy => policy
                           .RequireClaim("Create Role", "true"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("EditRolePolicy", policy =>
    {
        policy.AddRequirements(
            new ManageAdminRolesAndClaimsRequirement());
    });

builder.Services.AddAuthorizationBuilder()
                .AddPolicy("DeleteRolePolicy", policy => policy
                           .RequireClaim("Delete Role", "true"));

// Add role based authorization policies
builder.Services.AddAuthorizationBuilder()
                .AddPolicy("AdminRolePolicy", policy => policy
                           .RequireRole("Admin"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                 options =>
                 {
                     options.Password.RequiredLength = 6;
                     options.Password.RequiredUniqueChars = 1;
                     //options.Password.RequireNonAlphanumeric = false;
                     //options.Password.RequireUppercase = true;
                     //options.Password.RequireLowercase = true;
                     //options.Password.RequireDigit = true;
                     //options.User.RequireUniqueEmail = true;
                     //options.SignIn.RequireConfirmedEmail = false;
                 })
                .AddEntityFrameworkStores<AppDBContext>();

builder.Services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

// Register the custom authorization handler
builder.Services.AddSingleton<
    IAuthorizationHandler, 
    CanEditOnlyOtherAdminRolesAndClaimsHandler>();

// Add authentication cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Administration/AccessDenied";
});

// Add Google authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] 
        ?? throw new InvalidOperationException("Google ClientId is missing.");

        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] 
        ?? throw new InvalidOperationException("Google ClientSecret is missing.");
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    //app.UseStatusCodePages();
    //app.UseStatusCodePagesWithRedirects("/Error/{0}");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Custom authorization logic for EditRolePolicy
static bool AuthorizeAccess(AuthorizationHandlerContext context)
{
    return (context.User.IsInRole("Admin") &&
           context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "true")) ||
           context.User.IsInRole("Super Admin");
}

