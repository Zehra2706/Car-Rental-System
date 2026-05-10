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
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        var token = Request.Cookies["AuthToken"];
        var remember = Request.Cookies["RememberMe"];



        if (!string.IsNullOrEmpty(token) && remember == "true")
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

                return role == "Admin"
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "User");
            }
            catch
            {
                Response.Cookies.Delete("AuthToken");
            }
        }

        // 🔽 EMAIL REMEMBER (senin mevcut sistemin)
        var email = Request.Cookies["RememberEmail"];

        var model = new LoginViewModel();

        if (!string.IsNullOrEmpty(email))
        {
            model.Email = email;
            model.RememberMe = true;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        Console.WriteLine("GELEN EMAIL: " + model.Email);

        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState Hatalı: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ViewBag.Error = "Formda eksik veya hatalı alan var.";
            return View(model);
        }

        // 1. Önce kullanıcıyı Getir
        var user = _userService.GetByEmail(model.Email);

        if (user == null)
        {
            Console.WriteLine("❌ USER BULUNAMADI: " + model.Email);
            ViewBag.Error = "Email veya şifre yanlış.";
            return View(model);
        }

        Console.WriteLine("✅ USER BULUNDU: " + user.Name);

        // 2. 🔒 Hesap kilitli mi kontrol et?
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.Now)
        {
            Console.WriteLine($"Hesap kilitli: {user.UserInfo.Email}, LockoutEnd: {user.LockoutEnd}");
            ViewBag.Error = "Hesabınız kilitli. Şifrenizi sıfırlamanız gerekiyor.";
            ViewBag.IsLocked = true;
            return View(model);
        }

        // 3. Şifre Doğrulamayı Yap
        var authenticatedUser = _userService.Login(model.Email, model.Password);

        // 4. Şifre Yanlışsa (authenticatedUser null ise)
        if (authenticatedUser == null)
        {
            Console.WriteLine($"Şifre yanlış: {model.Email}");
            user.AccessFailedCount++;

            if (user.AccessFailedCount >= 3)
            {
                user.LockoutEnd = DateTime.MaxValue; // 🔒 kalıcı kilit
                user.AccessFailedCount = 3;
                ViewBag.Error = "3 hatalı giriş. Hesabınız kilitlendi. Şifre sıfırlayın.";
                ViewBag.IsLocked = true;
            }
            else
            {
                ViewBag.Error = $"Yanlış şifre. Kalan hak: {3 - user.AccessFailedCount}";
                Console.WriteLine($"Incorrect entry. Kalan hak: {3 - user.AccessFailedCount}");
            }

            _userService.Update(user); // Hata sayısını DB'ye işle
            return View(model);
        }

        // 5. ✅ Başarılı giriş (Buraya geldiyse şifre doğrudur)
        Console.WriteLine($"Başarılı giriş: {user.UserInfo.Email}");
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        _userService.Update(user); // Kilitleri ve sayaçları sıfırla

        // --- Cookie ve Hatırlama Ayarları ---
        if (model.RememberMe)
        {
            Response.Cookies.Append("RememberMe", "true", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = Request.IsHttps
            });

            Response.Cookies.Append("RememberEmail", model.Email, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = Request.IsHttps
            });
        }
        else
        {
            Response.Cookies.Delete("RememberMe");
            Response.Cookies.Delete("RememberEmail");
        }

        // --- JWT Token Oluşturma ---
        var claims = new List<Claim>
    {
        new Claim("UserId", user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.UserInfo.Email),
        new Claim(ClaimTypes.Role, user.UserRole.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim("TC", user.TC ?? "00000000000")
    };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        if (model.RememberMe)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe, // 🔥 kritik
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });
        }
        else
        {
            await HttpContext.SignInAsync("Cookies", claimsPrincipal);
        }


        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", user.UserRole.ToString());
        HttpContext.Session.SetString("UserEmail", user.UserInfo.Email);
        HttpContext.Session.SetString("TC", user.TC ?? "");

        Console.WriteLine("DB PASSWORD: " + user.UserInfo.Password);
        Console.WriteLine("GELEN PASSWORD: " + model.Password);
        Console.WriteLine("ROLE: " + user.UserRole.ToString());

        // --- Yönlendirme ---
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
        Response.Cookies.Delete("UserEmail");
        Response.Cookies.Delete("UserName");
        Response.Cookies.Delete("RememberMe");
        Response.Cookies.Delete("RememberEmail");

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]

    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        if (!Regex.IsMatch(model.TC, @"^\d{11}$"))
        {
            ViewBag.Error = "TC 11 haneli olmalı";
            return View(model);
        }

        if (!Regex.IsMatch(model.PhoneNumber, @"^0\d{10}$"))
        {
            ViewBag.Error = "Telefon 0 ile başlamalı ve 11 haneli olmalı";
            return View(model);
        }

        if (!Regex.IsMatch(model.LicenseNumber, @"^\d{6}$"))
        {
            ViewBag.Error = "Ehliyet numarası 6 haneli olmalı";
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
            ViewBag.Error = "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.";
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        if (string.IsNullOrEmpty(model.Password))
            return Content("Şifre boş olamaz");
        if (model.Password != model.ConfirmPassword)
        {
            ViewBag.Error = "Şifreler uyuşmuyor";
            return View(model);
        }

        // 🔐 BURAYA KOYUYORSUN
        bool isStrongPassword = Regex.IsMatch(model.Password,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$");

        if (!isStrongPassword)
        {
            ViewBag.Error = "Şifre en az 1 büyük harf, 1 küçük harf, 1 rakam ve 1 özel karakter içermelidir.";
            ViewBag.Token = model.Token;
            return View();
        }

        try
        {
            var user = _userService.GetUserByResetToken(model.Token);

            if (user == null)
                return Content("Geçersiz token");

            _userService.ResetPassword(model.Token, model.Password);

            var updatedUser = _userService.GetByEmail(user.UserInfo.Email);

            updatedUser.AccessFailedCount = 0;
            updatedUser.LockoutEnd = null;

            _userService.Update(updatedUser);

            return RedirectToAction("Login");
        }
        catch
        {
            ModelState.AddModelError("", "Bir hata oluştu");
            ViewBag.Token = model.Token;
            return View(model);
        }
    }
    [HttpGet]
    public IActionResult ResetPassword([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Auth");

        return View(new ResetPasswordViewModel
        {
            Token = token
        });
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