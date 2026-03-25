using Car_reservation_automation_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

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

        // Session veya Cookie (şimdilik basit)
        HttpContext.Session.SetString("UserEmail", user.UserInfo.Email);

        return RedirectToAction("Index", "Home");
    }
}