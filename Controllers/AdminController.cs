using car.Data;
using Car_reservation_automation_system.Repositories.Interfaces;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static user.Models.User;
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

    private readonly ApplicationDbContext _context;


    public AdminController(IUserService userService, ICarService carService, INotificationService notificationService, IRentalService rentalService, IRentalRepository rentalRepo, IReviewService reviewService, IReviewRepository reviewRepository, ApplicationDbContext context)
    {
        _userService = userService;
        _carService = carService;
        _notificationService = notificationService;
        _rentalService = rentalService;
        _rentalRepo = rentalRepo;
        _reviewService = reviewService;
        _reviewRepository = reviewRepository;
        _context = context;
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
    [ValidateAntiForgeryToken]
    public IActionResult DeleteUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var user = _userService.GetAllUsers().FirstOrDefault(u => u.Id == id);

        if (user == null) return NotFound();

        if (user.Id == currentUserId)
        {
            TempData["Error"] = "Kendinizi silemezsiniz!";
            return RedirectToAction("AdminList");
        }

        if (user.UserInfo?.Email == "admin@gmail.com")
        {
            TempData["Error"] = "Sistem yöneticisi silinemez!";
            return RedirectToAction("AdminList");
        }

        if (!_userService.CanDeleteUser(id))
        {
            TempData["Error"] = "Kullanıcının aktif kiralaması veya kirada aracı var.";
            return user.UserRole == UserModel.Role.Admin ? RedirectToAction("AdminList") : RedirectToAction("UserList");
        }

        var role = user.UserRole;
        _userService.DeleteUser(id);

        TempData["Success"] = "Kullanıcı başarıyla silindi.";
        return (role == UserModel.Role.Admin) ? RedirectToAction("AdminList") : RedirectToAction("UserList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddUser(AdminCreateUserViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid) return View(model);

        try
        {
            // UserService içindeki AddUser metodunda rol ekleme kodun varsa burası tamdır.
            _userService.AddUser(model);

            TempData["Success"] = "İşlem başarıyla tamamlandı!";
            return RedirectToAction("AdminList");
        }
        catch (Exception ex)
        {
            // Hata mesajını (Email zaten var vb.) kullanıcıya gösterir
            ModelState.AddModelError("", ex.Message);
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
        try
        {
            _carService.DeleteCar(id);
            TempData["Success"] = "Araç başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.";
        }

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
        var aktifler = allRentals.Where(x => !x.IsReturned).ToList();
        var gecmisler = allRentals.Where(x => x.IsReturned).ToList();

        ViewBag.Gecmisler = gecmisler;

        return View(aktifler);
        
    }
    [HttpGet]
    public IActionResult ManageReviews()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Auth");

        var allReviews = _reviewService.GetAllReviews();
        return View(allReviews);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReturnCar(int rentalId)
    {
        var rental = _rentalRepo.GetById(rentalId);

        rental.IsReturned = true;
        rental.IsCompleted = false;

        _rentalService.UpdateRental(rental);

        return RedirectToAction("AllRentalRequests");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompleteRental(int rentalId)
    {
        var rental = _rentalRepo.GetById(rentalId);

        rental.IsCompleted = true;
        rental.Status = "Tamamlandı";

        _rentalService.UpdateRental(rental);

        return RedirectToAction("AllRentalRequests");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteReviewByAdmin(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Unauthorized();

        // Admin olduğu için UserId kontrolü yapmadan direkt siliyoruz
        _reviewRepository.DeleteReview(id);

        TempData["Success"] = "Yorum admin tarafından başarıyla kaldırıldı.";
        return RedirectToAction("ManageReviews");
    }

    public IActionResult PastRentals()
    {
        var rentals = _userService.GetAllRentals()
                        .Where(x => x.IsReturned)
                        .ToList();

        return View(rentals);
    }
    [HttpGet]
    public IActionResult RentalDetail(int id)
    
    {
        var rental = _context.Rentals
            .Include(x => x.User)
                .ThenInclude(u => u.UserInfo)
            .Include(x => x.User)
                .ThenInclude(u => u.UserConnections)
            .Include(x => x.User)
                .ThenInclude(u => u.Licence)
            .Include(x => x.Car)
            .FirstOrDefault(x => x.Id == id);
        if (rental == null)
            return NotFound();

        return View(rental);
    }


}