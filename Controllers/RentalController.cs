using Microsoft.AspNetCore.Mvc;
using car.Models;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using rental.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization; // 🚩 Sadece bu eklendi

namespace car.Controllers
{
    [Authorize] // 🚩 Sınıfın tepesine eklendi (Giriş şartı)
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;
        private readonly IConfiguration _configuration;

        private static readonly ConcurrentDictionary<string, string> _paymentCache = new();

        public RentalController(IRentalService rentalService, ICarService carService, IConfiguration configuration)
        {
            _rentalService = rentalService;
            _carService = carService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Create(int carId)
        {
            var car = _carService.GetCarForEdit(carId);
            if (car == null) return Content($"Hata: {carId} ID'li araç bulunamadı!");

            ViewBag.Car = car;
            var disabledRanges = _rentalService.GetDisabledDatesJson(carId);
            ViewBag.DisabledDates = JsonConvert.SerializeObject(disabledRanges);

            return View();
        }

        [HttpGet]
        public IActionResult Payment()
        {
            var data = TempData["PendingRental"] as string;
            if (string.IsNullOrEmpty(data)) return RedirectToAction("Index", "Home");

            var rental = JsonConvert.DeserializeObject<rental.Models.Rental>(data);
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

            _rentalService.ConfirmAndSave(rental);

            TempData["Success"] = "Kiralama talebiniz başarıyla iletildi!";
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public JsonResult GetPriceCalculation(int carId, double hours)
        {
            var calculation = _rentalService.CalculateHourlyPrice(carId, hours);
            return Json(new { total = calculation.total, deposit = calculation.deposit });
        }

        [HttpPost]
        public IActionResult ConfirmPayment(rental.Models.Rental rental)
        {
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

            rental.RealReturnDate = null;
            rental.Status = "OnayBekliyor";
            rental.IsReturned = false;

            _rentalService.ConfirmAndSave(rental);

            TempData["Success"] = "Ödeme başarılı! Kiralama talebiniz yöneticiye iletildi.";
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        public async Task<IActionResult> StartPayment(int CarId, DateTime Date, DateTime ReturnDate, string Forecast, string Deposit)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var rental = new Rental
            {
                CarId = CarId,
                Date = Date,
                ReturnDate = ReturnDate,
                UserId = userId.Value
            };

            var culture = System.Globalization.CultureInfo.InvariantCulture;

            if (double.TryParse(Forecast?.Replace(",", "."), System.Globalization.NumberStyles.Any, culture, out double parsedForecast))
            {
                rental.Forecast = parsedForecast;
            }

            if (double.TryParse(Deposit?.Replace(",", "."), System.Globalization.NumberStyles.Any, culture, out double parsedDeposit))
            {
                rental.Deposit = (int)Math.Round(parsedDeposit);
            }

            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.SecretKey))
            {
                TempData["Error"] = "iyzico API anahtarları eksik!";
                return RedirectToAction("Fail", new { message = "API anahtarı eksik" });
            }

            if (rental.Deposit <= 0)
            {
                TempData["Error"] = $"Depozito hesaplanamadı veya 0. Lütfen tekrar deneyin.";
                return RedirectToAction("Create", new { carId = rental.CarId });
            }

            var request = new Iyzipay.Request.CreateCheckoutFormInitializeRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = rental.Deposit.ToString("0.00", culture),
                PaidPrice = rental.Deposit.ToString("0.00", culture),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                BasketId = "B" + rental.CarId + "_" + userId.Value,
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = "http://localhost:5054/Rental/PaymentCallback"
            };

            request.Buyer = new Iyzipay.Model.Buyer
            {
                Id = userId.Value.ToString(),
                Name = "Test",
                Surname = "User",
                Email = "test@test.com",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Adres",
                Ip = "85.34.78.112",
                City = "Istanbul",
                Country = "Turkey"
            };

            request.ShippingAddress = new Iyzipay.Model.Address
            {
                ContactName = "Test User",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Adres"
            };

            request.BillingAddress = request.ShippingAddress;

            request.BasketItems = new List<Iyzipay.Model.BasketItem>
            {
                new Iyzipay.Model.BasketItem
                {
                    Id = "BI101",
                    Name = "Araç Depozito",
                    Category1 = "Rental",
                    ItemType = Iyzipay.Model.BasketItemType.PHYSICAL.ToString(),
                    Price = rental.Deposit.ToString("0.00", culture)
                }
            };

            var checkoutForm = await Iyzipay.Model.CheckoutFormInitialize.Create(request, options);

            if (checkoutForm.Status != "success")
            {
                TempData["Error"] = $"iyzico Hatası: {checkoutForm.ErrorMessage}";
                return RedirectToAction("Fail", new { message = checkoutForm.ErrorMessage });
            }

            _paymentCache[checkoutForm.Token] = JsonConvert.SerializeObject(rental);
            ViewBag.PaymentForm = checkoutForm.CheckoutFormContent;

            return View("IyzicoPayment");
        }

        [HttpPost]
        [AllowAnonymous] // 🚩 GEREKLİ: iyzico'nun ödeme sonucunu bildirebilmesi için bu metot herkese açık olmalı.
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentCallback(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Fail", new { message = "Token gelmedi" });

            if (!_paymentCache.TryGetValue(token, out string rentalDataString))
            {
                return RedirectToAction("Fail", new { message = "Kiralama verisi bulunamadı." });
            }

            var rental = JsonConvert.DeserializeObject<Rental>(rentalDataString);

            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            var request = new Iyzipay.Request.RetrieveCheckoutFormRequest { Token = token };
            var result = await Iyzipay.Model.CheckoutForm.Retrieve(request, options);

            if (result.Status == "success" && result.PaymentStatus == "SUCCESS")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null) rental.UserId = userId.Value;

                rental.Status = "OnayBekliyor";
                _rentalService.ConfirmAndSave(rental);

                _paymentCache.TryRemove(token, out _);

                TempData["Success"] = "Ödemeniz başarıyla alındı!";
                return RedirectToAction("Success");
            }
            else
            {
                _paymentCache.TryRemove(token, out _);
                string errorMessage = result.ErrorMessage ?? "Ödeme hatası.";
                TempData["Error"] = errorMessage;
                return RedirectToAction("Fail", new { message = errorMessage });
            }
        }

        [HttpGet]
        public IActionResult Success() => View();

        [HttpGet]
        public IActionResult Fail(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        [HttpPost]
        public IActionResult CancelRequest(int rentalId)
        {
            // Talebi silmek veya durumunu 'İptal Edildi' yapmak için servisi çağırıyoruz
            // Ben burada direkt silme (Cancel) mantığını ekliyorum
            _rentalService.CancelRentalRequest(rentalId);

            TempData["Success"] = "Kiralama talebiniz başarıyla iptal edildi.";
            return RedirectToAction("MyRentals", "User");
        }

    }
}