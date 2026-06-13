using EmployeeManagement.Models;

var builder = WebApplication.CreateBuilder(args);
// Register MVC services
builder.Services.AddControllersWithViews().AddXmlSerializerFormatters();
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
    pattern: "{controller=Home}/{action=Details}/{id?}");

app.MapFallback(async context =>
{
    await context.Response.WriteAsync("Page not found");
});

app.Run();
