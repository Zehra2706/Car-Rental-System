using Microsoft.AspNetCore.Mvc;
using car.ViewModels;
using car.Models;
using System.IO;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace car.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarService _carService;
        private readonly IRentalService _rentalService; // 🟢 RentalService'i ekledik

        // Constructor'ı her iki servisi alacak şekilde güncelledik
        public CarController(ICarService carService, IRentalService rentalService)
        {
            _carService = carService;
            _rentalService = rentalService;
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

            // 🚩 BURAYI EKLE: Eğer model geçersizse, nedenini anlamak için hataları toplayalım
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.Errors = errors; // View tarafında bu listeyi yazdırabiliriz.

            return View(model);
        }
        public IActionResult Details(int id)
        {
            var car = _carService.GetCarForEdit(id);
            if (car == null) return NotFound();

            // 🟢 HATALI KISIM BURAYDI: Servis üzerinden temizce çekiyoruz
            var disabledDates = _rentalService.GetDisabledDatesJson(id);

            // JSON'a çevirip takvime gönderiyoruz
            ViewBag.DisabledDates = JsonConvert.SerializeObject(disabledDates);

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
        public async Task<IActionResult> Edit(CarCreateViewModel model)
        {
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
                ViewBag.Error = "Hata: " + ex.Message;
                return View(model);
            }
        }

        // --- 6. ARAÇ SİLME ---
        [HttpGet]
        public IActionResult Delete(int id)
        {
            _carService.DeleteCar(id);
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

        private IActionResult RedirectByRole()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin" ? RedirectToAction("CarList", "Admin") : RedirectToAction("MyCars");
        }
    }
}