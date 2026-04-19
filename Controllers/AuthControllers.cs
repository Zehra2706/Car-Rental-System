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

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        var user = _userService.Login(model.Email, model.Password);

        if (user == null)
        {
            ViewBag.Error = "Email veya şifre yanlış";
            return View();
        }

        // ==========================================
        // 1. JWT (KİMLİK DOĞRULAMA / GÜVENLİK) İŞLEMLERİ
        // ==========================================
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.UserInfo.Email),
            new Claim(ClaimTypes.Role, user.UserRole.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2), // Token 2 saat geçerli
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // JWT'yi tarayıcıya güvenli bir Çerez (Cookie) olarak ekliyoruz
        Response.Cookies.Append("AuthToken", tokenString, new CookieOptions
        {
            HttpOnly = true, // JavaScript sızmalarına karşı koruma
            Secure = true,   // HTTPS zorunluluğu
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.Now.AddHours(2)
        });

        // ==========================================
        // 2. SESSION (GEÇİCİ HAFIZA) İŞLEMLERİ
        // ==========================================
        // Mevcut projenin diğer yerlerinde (örn. iyzico sepeti) sorun çıkmasın diye Session'a da yazıyoruz
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserEmail", user.UserInfo.Email);
        HttpContext.Session.SetString("UserRole", user.UserRole.ToString());

        // Yönlendirme (Rol bazlı)
        if (user.UserRole == UserEntity.Role.Admin)
        {
            return RedirectToAction("Index", "Admin");
        }

        return RedirectToAction("Index", "User");
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
    public IActionResult Register(RegisterViewModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            ViewBag.Error = "Şifreler uyuşmuyor!";
            return View(model);
        }

        try
        {
            _userService.Register(model);
            TempData["Success"] = "Kayıt başarılı! Lütfen giriş yapınız.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View(model);
        }
    }
}