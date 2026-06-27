using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Register your custom service
builder.Services.AddDbContextPool<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EmployeeDBConnection")));
builder.Services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    //app.UseStatusCodePages();
    //app.UseStatusCodePagesWithRedirects("/Error/{0}");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseStaticFiles();
app.MapControllers();
app.Run();
