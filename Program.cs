using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// MVC + runtime compilation helpful for Razor during dev (optional)
builder.Services.AddControllersWithViews();

// For prototype: increase limit for file uploads in case of test files
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// simple cookie-based role simulation (no real auth)
app.Use(async (context, next) =>
{
    // if no role cookie set, default to Lecturer (for prototype)
    if (!context.Request.Cookies.ContainsKey("CMCS_Role"))
    {
        context.Response.Cookies.Append("CMCS_Role", "Lecturer");
    }
    await next();
});
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
