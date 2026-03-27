using Car_reservation_automation_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class AuthController : Controller
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    // 🔹 LOGIN GET
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // 🔹 LOGIN POST
    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        var user = _userService.Login(model.Email, model.Password);

        if (user == null)
        {
            ViewBag.Error = "Email veya şifre yanlış";
            return View();
        }

        HttpContext.Session.SetString("UserEmail", user.UserInfo.Email);

        return RedirectToAction("Index", "Home");
    }

    // 🔹 REGISTER GET
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // 🔹 REGISTER POST
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