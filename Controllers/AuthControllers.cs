using Car_reservation_automation_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using user.Models;
using UserEntity = user.Models.User;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;


    public AuthController(IUserService userService, IConfiguration configuration, INotificationService notificationService)
    {
        _userService = userService;
        _configuration = configuration;
        _notificationService = notificationService;
    }

    [HttpGet]

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        // Model doğruluğunu kontrol et
        if (!ModelState.IsValid) return View(model);

        // 1. Kullanıcıyı getir
        var user = _userService.GetByEmail(model.Email);

        if (user == null)
        {
            ViewBag.Error = "Email veya şifre yanlış.";
            return View(model);
        }

        // 2. Kilitleme Kontrolü
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.Now)
        {
            var kalanDakika = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.Now).TotalMinutes);
            ViewBag.Error = $"Çok fazla hatalı deneme! Hesabınız {kalanDakika} dakika kilitlendi.";
            ViewBag.IsLocked = true;
            return View(model);
        }

        // 3. Şifre Doğrulama
        var authenticatedUser = _userService.Login(model.Email, model.Password);

        if (authenticatedUser == null)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.Now.AddMinutes(20);
                ViewBag.Error = "5 kez hatalı giriş yapıldı. Hesabınız 20 dakika kilitlendi.";
                ViewBag.IsLocked = true;
            }
            else
            {
                ViewBag.Error = $"Email veya şifre yanlış. Kalan deneme hakkınız: {5 - user.AccessFailedCount}";
            }

            _userService.Update(user);
            return View(model);
        }

        // 4. Başarılı Giriş: Sayaçları sıfırla
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        _userService.Update(user);

        // 5. JWT Token Oluşturma (Claims)
        var claims = new List<Claim>
    {
        new Claim("UserId", user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.UserInfo.Email),
        new Claim(ClaimTypes.Role, user.UserRole.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim("TC", user.TC ?? "00000000000")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // 6. Cookie ve Session İşlemleri
        Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.Now.AddHours(2)
        });

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", user.UserRole.ToString());

        // 7. Yönlendirme
        return user.UserRole == UserEntity.Role.Admin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        // ==========================================
        // ÇIKIŞ İŞLEMİ: HEM JWT'Yİ HEM DE SESSION'I TEMİZLE
        // ==========================================

        // 1. JWT Çerezini sil
        Response.Cookies.Delete("AuthToken");

        // 2. Tüm Session'ı boşalt
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ViewBag.Error = "Şifreler uyuşmuyor!";
            return View(model);
        }

        try
        {
            _userService.Register(model);
            TempData["Success"] = "Kayıt başarılı! Lütfen giriş yapınız.";
            var user = _userService.GetByEmail(model.Email);
            var token = Guid.NewGuid().ToString();
            var code = new Random().Next(100000, 999999).ToString();
            user.EmailVerificationCode = code;
            user.EmailConfirmationToken = token;
            user.IsEmailConfirmed = false;
            _userService.Update(user);
            _notificationService.WelcomeMail(user);
            _notificationService.EmailVerification(user, code);
            return RedirectToAction("VerifyEmail", "Auth", new { email = model.Email });
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View(model);
        }
    }
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        var user = _userService.GetByEmail(email);

        if (user == null)
        {
            ViewBag.Error = "Bu email kayıtlı değil";
            return View();
        }

        _userService.SendPasswordResetEmail(email);

        ViewBag.Message = "Şifre sıfırlama maili gönderildi";
        return View();
    }
    [HttpGet]
    public IActionResult ResetPassword(string token, string password, string confirmPassword)
    {
        var user = _userService.GetUserByResetToken(token);

        if (password != confirmPassword)
        {
            ViewBag.Error = "Şifreler uyuşmuyor!";
            ViewBag.Token = token;
            return View();
        }

        if (user == null)
            return Content("Link geçersiz veya süresi dolmuş");

        ViewBag.Token = token;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(string token, string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return Content("Şifre boş olamaz");
        }
        try
        {
            _userService.ResetPassword(token, password);
            return RedirectToAction("Login");
        }
        catch
        {
            return Content("Bir hata oluştu");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyEmail(string email, string code)
    {
        var user = _userService.GetByEmail(email);

        if (user == null)
            return Content("User not found");

        if (user.EmailVerificationCode != code)
        {
            TempData["Error"] = "Kod yanlış!";
            return RedirectToAction("VerifyEmail", new { email });
        }

        user.IsEmailConfirmed = true;
        user.EmailVerificationCode = null;

        _userService.Update(user);
        TempData["Success"] = "Email doğrulandı!";
        return RedirectToAction("Login", "Auth");
    }
    [HttpGet]
    public IActionResult VerifyEmail(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [HttpGet]
    public IActionResult ResendCode(string email)
    {
        var user = _userService.GetByEmail(email);

        if (user == null)
            return Content("User not found");

        var code = new Random().Next(100000, 999999).ToString();

        user.EmailVerificationCode = code;
        _userService.Update(user);


        _notificationService.EmailVerification(user, code);

        TempData["Success"] = "Yeni doğrulama kodu gönderildi.";

        return RedirectToAction("VerifyEmail", new { email = email });
    }




}