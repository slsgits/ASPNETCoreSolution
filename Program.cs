using EmployeeManagement.Models;

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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallback(async context =>
{
    await context.Response.WriteAsync("Page not found");
});

app.Run();
