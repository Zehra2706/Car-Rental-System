using Car_reservation_automation_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using user.Models;
using UserEntity = user.Models.User;

public class AuthController : Controller
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
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
        // Login başarılı olduktan sonra:
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserEmail", user.UserInfo.Email);
        HttpContext.Session.SetString("UserRole", user.UserRole.ToString());

    if (user.UserRole == UserEntity.Role.Admin)
    {
        return RedirectToAction("Index", "Admin");
    }

        return RedirectToAction("Index", "User");
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
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View(model);
        }
    }
}