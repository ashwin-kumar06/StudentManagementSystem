using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StudentManagementSystem.Services;
using StudentManagementSystem.Models.Auth;
using StudentManagementSystem.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["OAuth:AuthorizationServerBaseUrl"];
    options.Audience = builder.Configuration["OAuth:ClientId"];
    options.RequireHttpsMetadata = false; // Set to true in production
});
builder.Services.AddHttpClient<IOAuthClientService, OAuthClientService>().AddHttpMessageHandler<AuthenticatedHttpClientHandler>();
builder.Services.Configure<OAuthConfig>(builder.Configuration.GetSection("OAuth"));
builder.Services.AddScoped<AuthenticatedHttpClientHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.WithOrigins("https://localhost:5105")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5105, options => // Use 5105 for Student Management System
    {
        options.UseHttps();
    });
});



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");
app.Run();