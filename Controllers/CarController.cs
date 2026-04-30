using Microsoft.AspNetCore.Mvc;
using car.ViewModels;
using car.Models;
using System.IO;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using car.Data;

namespace car.Controllers
{
    [Authorize]
    public class CarController : Controller
    {
        private readonly ICarService _carService;
        private readonly IRentalService _rentalService;
        private readonly IReviewService _reviewService;
        private readonly ApplicationDbContext _context;

        public CarController(ICarService carService, IRentalService rentalService, IReviewService reviewService, ApplicationDbContext context)
        {
            _carService = carService;
            _rentalService = rentalService;
            _reviewService = reviewService;
            _context = context;
        }

        // --- 1. İLANLARIM LİSTESİ ---
        public IActionResult MyCars()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var myCars = _carService.GetCarsByUserId(userId.Value);
            return View(myCars);
        }

        // --- 2. YENİ ARAÇ OLUŞTURMA (GET) ---
        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarCreateViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            model.UserId = userId.Value;

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    model.ImagePath = await SaveImage(model.ImageFile);
                }

                _carService.AddNewCar(model);
                return RedirectToAction("MyCars");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.Errors = errors;

            return View(model);
        }

        // --- 4. ARAÇ DETAYLARI ---
        public IActionResult Details(int id)
        {
            var car = _carService.GetCarById(id);

            if (car == null) return NotFound();

            // Diğer bilgiler
            var disabledDates = _rentalService.GetDisabledDatesJson(id);
            ViewBag.DisabledDates = JsonConvert.SerializeObject(disabledDates);
            ViewBag.Reviews = _reviewService.GetCarReviews(id);

            return View(car);
        }

        // --- 5. ARAÇ DÜZENLEME (GET) ---
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.Role = role;

            var model = _carService.GetCarForEdit(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // --- 5. ARAÇ DÜZENLEME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> Edit(CarCreateViewModel model)
        {
            if (model.Id == 0)
            {
                return Content("Hata: ID 0 geliyor, bu yüzden güncellenecek araç bulunamıyor!");
            }
            if (!ModelState.IsValid) return View(model);

            try
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    model.ImagePath = await SaveImage(model.ImageFile);
                }

                _carService.UpdateCar(model);
                TempData["Success"] = "İlan başarıyla güncellendi!";
                return RedirectByRole();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Bir hata oluştu. Lütfen tekrar deneyin." ;
                return View(model);
            }
        }

        // --- 6. ARAÇ SİLME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
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

            return RedirectToAction("MyCars");
        }

        // --- YARDIMCI METOTLAR ---
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return "/images/cars/" + fileName;
        }
        [HttpGet]
        public IActionResult EditReview(int id)
        {
            var review = _reviewService.GetReview(id);
            if (review == null) return NotFound();

            // Güvenlik kontrolü
            if (review.UserId != HttpContext.Session.GetInt32("UserId")) return Unauthorized();

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(int id, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var review = _reviewService.GetReview(id);
            if (review == null) return NotFound();

            _reviewService.UpdateReview(id, userId.Value, rating, comment);

            return RedirectToAction("MyReviews");
        }
        public IActionResult ViewCarReviews(int carId)
        {
            // 1. Aracı buluyoruz (Başlıkta marka/model göstermek ve geri dönüş linki için)
            var car = _carService.GetCarById(carId);

            if (car == null)
            {
                return NotFound();
            }

            var reviews = _reviewService.GetCarReviews(carId);

            ViewBag.CarId = carId;
            ViewBag.CarInfo = $"{car.Brand} {car.ModelName}";
            ViewBag.CarImage = car.ImagePath;


            return View(reviews);
        }


        public IActionResult DeleteReview(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");


            _reviewService.DeleteReview(id, userId.Value);

            TempData["Success"] = "Yorumunuz kalıcı olarak silindi.";

            return RedirectToAction("MyReviews");
        }
        public IActionResult MyReviews()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var reviews = _reviewService.GetUserReviews(userId.Value);
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(int carId, int rating, string comment)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            _reviewService.PostReview(carId, userId.Value, rating, comment);

            TempData["Success"] = "Değerlendirmeniz başarıyla kaydedildi, teşekkür ederiz!";

            return RedirectToAction("Index", "User");
        }
        private IActionResult RedirectByRole()
        {
            var role = HttpContext.Session.GetString("UserRole");

            return role == "Admin"
                ? RedirectToAction("CarList", "Admin")
                : RedirectToAction("MyCars", "Car");
        }
        [HttpGet]
        public IActionResult AdminList(string search, CarFilter filter)
        {
            var cars = _carService.FilterCars(filter);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                cars = cars.Where(c =>
                    c.Brand.ToLower().Contains(search) ||
                    c.ModelName.ToLower().Contains(search) ||
                    c.Plate.ToLower().Contains(search)
                ).ToList();
            }
            return View("~/Views/Admin/CarList.cshtml", cars);
        }

        public IActionResult UserList(string search, CarFilter filter)
        {
            var cars = _carService.FilterCars(filter);
            cars = cars.Where(x => x.IsActive == true).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                cars = cars.Where(c =>
                    c.Brand.ToLower().Contains(search) ||
                    c.ModelName.ToLower().Contains(search) ||
                    c.Plate.ToLower().Contains(search)
                ).ToList();
            }

            return View("~/Views/User/AvailableCars.cshtml", cars);
        }

        public IActionResult ToggleActive(int id)
        {
            var car = _context.Cars.Find(id);

            if (car == null)
                return NotFound();

            // 🔥 ASIL ÖNEMLİ SATIR
            car.IsActive = !car.IsActive;

            _context.SaveChanges();

            TempData["Success"] = car.IsActive 
                ? "İlan açıldı" 
                : "İlan kapatıldı";

            return RedirectToAction("MyCars");
}

    }
}