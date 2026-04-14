using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UserModel = user.Models.User;

public class AdminController : Controller
{
    private readonly ICarService _carService;
    private readonly IUserService _userService;

    public AdminController(IUserService userService, ICarService carService)
    {
        _userService = userService;
        _carService = carService;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public IActionResult Index()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View();
    }

    public IActionResult UserList()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var users = _userService.GetAllUsers()
            .Where(u => u.UserRole == UserModel.Role.Customer)
            .ToList();

        return View(users);
    }

    public IActionResult AdminList()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var admins = _userService.GetAllUsers()
            .Where(u => u.UserRole == UserModel.Role.Admin)
            .ToList();

        return View(admins);
    }


    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

        var user = _userService.GetAllUsers().FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound();

        if (user.Id == currentUserId)
        {
            TempData["Error"] = "Kendinizi silemezsiniz!";
            return RedirectToAction("AdminList");
        }

        if (user.UserInfo.Email == "admin@gmail.com")
        {
            TempData["Error"] = "İlk admin silinemez!";
            return RedirectToAction("AdminList");
        }

        _userService.DeleteUser(id);

        return RedirectToAction("AdminList");
    }

    [HttpPost]
    public IActionResult AddUser(AdminCreateUserViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _userService.AddUser(model);

            TempData["Success"] = "Admin başarıyla eklendi!";
            return RedirectToAction("AdminList");
        }
        catch (Exception ex)
        {
            var error = ex.InnerException?.Message ?? ex.Message;
            ModelState.AddModelError("", error);
            return View(model);
        }
    }
    [HttpGet]
    public IActionResult AddUser()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View();
    }

    public IActionResult CarList()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "Auth");

        var cars = _carService.GetAllCars();
        return View(cars);
    }

    public IActionResult DeleteCar(int id)
    {
        _carService.DeleteCar(id);
        return RedirectToAction("CarList");
    }

    public IActionResult AddCar()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View();
    }

    public IActionResult EditCar(int id)
    {
        return RedirectToAction("Edit", "Car", new { id = id });
    }


    public IActionResult AllRentalRequests()
    {
        var allRentals = _userService.GetAllRentals();
        return View(allRentals);
    }
}