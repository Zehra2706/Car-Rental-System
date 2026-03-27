using Microsoft.AspNetCore.Mvc;
using car.ViewModels;
using car.Models;
using System.IO;
using Car_reservation_automation_system.Service.Interfaces;

namespace car.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarService _carService;

        public CarController(ICarService carService) => _carService = carService;

        // --- LİSTELEME ---
        public IActionResult MyCars()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var myCars = _carService.GetCarsByEmail(userEmail);
            return View(myCars);
        }

        // --- YENİ İLAN OLUŞTURMA ---
        [HttpGet]
        public IActionResult Create()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
                model.UserId = currentUserId;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars");

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    model.ImagePath = "/images/cars/" + fileName;
                }

                _carService.AddNewCar(model);
                TempData["Success"] = "İlan başarıyla oluşturuldu!";
                return RedirectToAction("MyCars");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _carService.GetCarForEdit(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CarCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cars");

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    model.ImagePath = "/images/cars/" + fileName;
                }

                _carService.UpdateCar(model);
                TempData["Success"] = "İlan başarıyla güncellendi!";
                return RedirectToAction("MyCars");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Güncelleme sırasında hata oluştu: " + ex.Message;
                return View(model);
            }
        }
    }
}