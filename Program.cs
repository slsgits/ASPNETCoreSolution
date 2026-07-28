using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.Services;
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
                     options.SignIn.RequireConfirmedEmail = true;
                     options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

                     // Configure lockout settings
                     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                     options.Lockout.MaxFailedAccessAttempts = 3;
                     options.Lockout.AllowedForNewUsers = true;
                 })
                .AddEntityFrameworkStores<AppDBContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider
                                  <ApplicationUser>>("CustomEmailConfirmation");

builder.Services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

// Register the custom authorization handler
builder.Services.AddScoped<
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

// Add Facebook authentication
builder.Services.AddAuthentication()
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"] 
        ?? throw new InvalidOperationException("Facebook AppId is missing.");
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] 
        ?? throw new InvalidOperationException("Facebook AppSecret is missing.");
    });

// token lifespan configuration globally for all token providers (default is 1 day)
builder.Services
    .Configure<DataProtectionTokenProviderOptions>
    (options =>
    {
        options.TokenLifespan = TimeSpan.FromHours(6);
    });

// token lifespan configuration for email confirmation
builder.Services
    .Configure<EmailConfirmationTokenProviderOptions>
    (options =>
    {
      options.TokenLifespan = TimeSpan.FromHours(1);
    });

// Register the data protection services
builder.Services.AddDataProtection();
builder.Services.AddScoped<IDataProtectionService,
                           DataProtectionService>();
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

