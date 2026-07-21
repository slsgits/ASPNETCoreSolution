using EmployeeManagement.Models;
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
// Add authorization policies
builder.Services.AddAuthorizationBuilder()
                .AddPolicy("DeleteRolePolicy", policy => policy
                           .RequireClaim("Delete Role")
                           .RequireClaim("Create Role"));

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
