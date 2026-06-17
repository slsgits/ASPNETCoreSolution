using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Register your custom service
builder.Services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

// Configure routing for MVC
//app.MapControllerRoute(
//    name: "default",
//    pattern: "shantilal/{controller=Home}/{action=Index}/{id?}"
//    );

//app.MapDefaultControllerRoute(); // This is a shorthand for the above MapControllerRoute method
app.MapControllers();

app.MapFallback(async context =>
{
    await context.Response.WriteAsync("Page not found");
});

app.Run();
