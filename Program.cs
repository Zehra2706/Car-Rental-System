using Car_reservation_automation_system.Repositories.Interfaces;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using car.Service.Concrete;
using Car_reservation_automation_system.Repositories.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ---------------- SERVICES ----------------

builder.Services.AddControllersWithViews();

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<car.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();

builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
// SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// JWT AUTH
// builder.Services.AddAuthentication("Cookies")
//     .AddCookie("Cookies", options =>
//     {
//         options.LoginPath = "/Auth/Login";
//         options.AccessDeniedPath = "/Auth/AccessDenied";
//     })    
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
//             NameClaimType = System.Security.Claims.ClaimTypes.Name,
//             RoleClaimType = System.Security.Claims.ClaimTypes.Role
//         };

//         options.Events = new JwtBearerEvents
//         {
//             OnMessageReceived = context =>
//             {
//                 context.Token = context.Request.Cookies["AuthToken"];
//                 return Task.CompletedTask;
//             },
//             OnChallenge = context =>
//             {
//                 context.HandleResponse();
//                 context.Response.Redirect("/Auth/Login");
//                 return Task.CompletedTask;
//             },
//             OnForbidden = context =>
//             {
//                 context.Response.Redirect("/Auth/AccessDenied");
//                 return Task.CompletedTask;
//             }
//         };
//     });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddAuthorization();
var app = builder.Build();

// ---------------- MIDDLEWARE PIPELINE ----------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();

app.UseAuthentication();   // önce auth
app.UseAuthorization();    // sonra yetki

app.UseMiddleware<RequestLoggingMiddleware>(); // sonra log middleware

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();