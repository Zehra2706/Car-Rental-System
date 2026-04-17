using Car_reservation_automation_system.Repositories.Interfaces;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using car.Service.Concrete;
using Car_reservation_automation_system.Repositories.Concrete;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS KAYITLARI (BUILD'DEN ÖNCE) ---

builder.Services.AddControllersWithViews();

// Repository ve Service kayıtları
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();

// Veritabanı bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<car.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🚩 SESSION AYARLARI (Buraya taşıdık ve tek bir yerDde dotnet topladık)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Tarayıcı çerezleri kısıtlasa da çalışmasını sağlar
});

// ------------------------------------------------------------------
var app = builder.Build(); // 🚧 KRİTİK SINIR: Mutfak kapandı!
// ------------------------------------------------------------------

// --- 2. MIDDLEWARE / BORU HATTI (BUILD'DEN SONRA) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // MapStaticAssets yerine standart UseStaticFiles daha güvenlidir

app.UseRouting();

// 🚩 UseSession MUTLAKA UseRouting'den sonra, UseAuthorization'dan önce olmalı
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();