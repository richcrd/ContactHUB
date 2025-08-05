using Microsoft.EntityFrameworkCore;
using ContactHUB.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ContactHUB.Services.AuthService>();

builder.Services.AddDbContext<ContactDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error500");
    app.UseStatusCodePages(async context =>
    {
        if (context.HttpContext.Response.StatusCode == 404)
        {
            context.HttpContext.Response.Redirect("/NotFound");
        }
    });
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();



// Area route mapping for Admin controllers
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/NotFound", context =>
{
    context.Response.StatusCode = 404;
    return context.Response.SendFileAsync("Views/Shared/NotFound.cshtml");
});

app.MapGet("/Error500", context =>
{
    context.Response.StatusCode = 500;
    return context.Response.SendFileAsync("Views/Shared/Error500.cshtml");
});


app.Run();