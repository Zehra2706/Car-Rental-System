using car.Service.Concrete;
using Car_reservation_automation_system.Repositories.Concrete;
using Car_reservation_automation_system.Repositories.Interfaces;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------- SERVICES ----------------

builder.Services.AddControllersWithViews();

// DB
// Bu satır sayesinde AWS'deki RDS adresini appsettings.json'dan çekecektir.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); builder.Services.AddDbContext<car.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

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


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddAuthorization();
var app = builder.Build();
// ------------ OTOMATİK VERİTABANI OLUŞTURMA (BYPASS) ------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<car.Data.ApplicationDbContext>();

    // Admin rolü var mı kontrol et
    var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");

    // Default admin kullanıcı var mı kontrol et
    var adminExists = context.UserInfo.Any(u => u.Email == "admin@gmail.com");

    if (!adminExists && adminRole != null)
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<user.Models.User>();

        var admin = new user.Models.User
        {
            Name = "Admin",
            Surname = "Admin",
            TC = "00000000000",
            Date = DateTime.Now,
            RoleId = adminRole.Id,
            IsEmailConfirmed = true,
            UserInfo = new userInfo.Models.UserInfo
            {
                Email = "admin@gmail.com",
                Password = hasher.HashPassword(null, "Admin123!")
            },
            UserConnections = new userConnections.Models.UserConnections
            {
                Adress = "Admin",
                Number = "05000000000"
            },
            Licence = new licence.Models.Licence
            {
                LicenceNumber = "000000"
            }
        };

        context.Users.Add(admin);
        context.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<car.Data.ApplicationDbContext>();
        // Eğer veritabanı yoksa yerel LocalDB üzerinde sıfırdan oluşturur:
        //context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Hata oluşursa konsola yazdırır ama uygulamanın çökmesini engeller
        Console.WriteLine($"Veritabanı oluşturulurken bir hata meydana geldi: {ex.Message}");
    }
}

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