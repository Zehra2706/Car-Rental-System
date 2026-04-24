using Car_reservation_automation_system.Repositories.Interfaces;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserModel = user.Models.User;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ICarService _carService;
    private readonly IUserService _userService;

    private readonly INotificationService _notificationService;

    private readonly IRentalService _rentalService;

    private readonly IRentalRepository _rentalRepo;
    private readonly IReviewService _reviewService;
    private readonly IReviewRepository _reviewRepository;


    public AdminController(IUserService userService, ICarService carService, INotificationService notificationService, IRentalService rentalService, IRentalRepository rentalRepo, IReviewService reviewService, IReviewRepository reviewRepository)
    {
        _userService = userService;
        _carService = carService;
        _notificationService = notificationService;
        _rentalService = rentalService;
        _rentalRepo = rentalRepo;
        _reviewService = reviewService;
        _reviewRepository = reviewRepository;
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
    [HttpGet]
    public IActionResult ManageReviews()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

        var allReviews = _reviewService.GetAllReviews();
        return View(allReviews);
    }

    public IActionResult DeleteReviewByAdmin(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Unauthorized();

        // Admin olduğu için UserId kontrolü yapmadan direkt siliyoruz
        _reviewRepository.DeleteReview(id);

        TempData["Success"] = "Yorum admin tarafından başarıyla kaldırıldı.";
        return RedirectToAction("ManageReviews");
    }

}