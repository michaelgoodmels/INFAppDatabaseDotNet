using INFAppDatabaseDotNet.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(480); // 8 hours from Web.config
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "INFAppDatabaseDotNet_Session";
});

// Add authentication with cookie scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(480);
        options.SlidingExpiration = true;
    });

// Add authorization
builder.Services.AddAuthorization();

// Add dependency injection for DataAccess classes
builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<UserDataAccess>();
builder.Services.AddScoped<StudentDataAccess>();
builder.Services.AddScoped<ModuleDataAccess>();
builder.Services.AddScoped<GradeDataAccess>();

// Add IConfiguration for dependency injection
builder.Services.AddSingleton(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Add session and authentication middleware (IMPORTANT: session before authentication)
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
