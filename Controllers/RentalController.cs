using Microsoft.AspNetCore.Mvc;
using car.Models;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace car.Controllers
{
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;

        // Constructor tek olmalı ve sadece servisleri almalı
        public RentalController(IRentalService rentalService, ICarService carService)
        {
            _rentalService = rentalService;
            _carService = carService;
        }

        [HttpGet]
        public IActionResult Create(int carId)
        {
            var car = _carService.GetCarForEdit(carId);

            if (car == null)
            {
                return Content($"Hata: {carId} ID'li araç veritabanında bulunamadı!");
            }

            ViewBag.Car = car;

            var disabledDates = _rentalService.GetDisabledDatesJson(carId);
            ViewBag.DisabledDates = Newtonsoft.Json.JsonConvert.SerializeObject(disabledDates);

            return View();
        }


        [HttpPost]
        public IActionResult ProcessToPayment(rental.Models.Rental rental)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");
            rental.UserId = userId.Value;

            if (!_rentalService.IsAvailable(rental.CarId, rental.Date, rental.ReturnDate))
            {
                TempData["Error"] = "Seçtiğiniz tarihler arasında araç artık müsait değil!";
                return RedirectToAction("Create", new { carId = rental.CarId });
            }

            TempData["PendingRental"] = JsonConvert.SerializeObject(rental);

            return RedirectToAction("Payment");
        }

        [HttpGet]
        public IActionResult Payment()
        {
            // TempData'dan veriyi geri alıyoruz
            var data = TempData["PendingRental"] as string;
            if (string.IsNullOrEmpty(data)) return RedirectToAction("Index", "Home");

            var rental = JsonConvert.DeserializeObject<rental.Models.Rental>(data);

            // Tekrar okuma ihtimaline karşı veriyi TempData'da tutalım
            TempData.Keep("PendingRental");

            return View(rental);
        }


        [HttpPost]
        public IActionResult SendRequest(rental.Models.Rental rental)
        {
            if (!_rentalService.IsAvailable(rental.CarId, rental.Date, rental.ReturnDate))
            {
                TempData["Error"] = "Seçtiğiniz tarihler arasında araç artık müsait değil!";
                return RedirectToAction("Create", new { carId = rental.CarId });
            }


            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            rental.UserId = userId.Value;
            rental.Status = "OnayBekliyor";
            rental.IsReturned = false;

            // Servis üzerinden kaydetme işlemini yapıyoruz
            _rentalService.ConfirmAndSave(rental);

            TempData["Success"] = "Kiralama talebiniz başarıyla iletildi!";
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public JsonResult GetPriceCalculation(int carId, int days)
        {
            // Servis üzerinden gerçek rakamları alıyoruz
            var calculation = _rentalService.CalculatePrice(carId, days);

            // Dönen değerleri ekrana (JavaScript'e) gönderiyoruz
            return Json(new { total = calculation.total, deposit = calculation.deposit });
        }

        [HttpPost]
        public IActionResult ConfirmPayment(rental.Models.Rental rental)
        {
            // Formdan UserId gelmediyse veya 0 geldiyse Session'dan tekrar çekiyoruz
            if (rental.UserId <= 0)
            {
                var sessionUserId = HttpContext.Session.GetInt32("UserId");
                if (sessionUserId == null)
                {
                    TempData["Error"] = "Oturum süreniz dolmuş, lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Auth");
                }
                rental.UserId = sessionUserId.Value;
            }

            // RealReturnDate'i NULL yapıyoruz (Daha önce SQL'de nullable yapmıştık)
            rental.RealReturnDate = null;
            rental.Status = "OnayBekliyor";
            rental.IsReturned = false;

            // Veritabanına kaydet (Service Katmanı)
            _rentalService.ConfirmAndSave(rental);

            TempData["Success"] = "Ödeme başarılı! Kiralama talebiniz yöneticiye iletildi.";
            return RedirectToAction("Index", "User");
        }
    }
}