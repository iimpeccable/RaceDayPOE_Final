using Microsoft.AspNetCore.Authentication.Cookies;
using RacingDays.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

// 2. Configure Session State
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// 4. Register Named HttpClient pointing to API port 7001
builder.Services.AddHttpClient("RacingDaysApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
});

// 5. Register Typed HttpClient for RacingDaysApiClient
builder.Services.AddHttpClient<RacingDaysApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Participant}/{action=Index}/{id?}");

app.Run();