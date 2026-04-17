using Microsoft.AspNetCore.Mvc;
using car.Models;
using Car_reservation_automation_system.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using rental.Models;
using Microsoft.Extensions.Configuration;

namespace car.Controllers
{
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;
        private readonly IConfiguration _configuration;

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

            if (car == null)
                return Content($"Hata: {carId} ID'li araç veritabanında bulunamadı!");

            ViewBag.Car = car;

            // ✅ Direkt serialize ediyoruz, {from, to} formatında gelir
            var disabledRanges = _rentalService.GetDisabledDatesJson(carId);
            ViewBag.DisabledDates = JsonConvert.SerializeObject(disabledRanges);

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

        // ✅ int days → double hours olarak düzeltildi
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
        public async Task<IActionResult> StartPayment(Rental rental)
        {
            HttpContext.Session.SetString("RentalData", JsonConvert.SerializeObject(rental));

            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.SecretKey))
                throw new Exception("iyzico API anahtarları eksik!");

            var request = new Iyzipay.Request.CreateCheckoutFormInitializeRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = "123456",
                Price = rental.Deposit.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                PaidPrice = rental.Deposit.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                BasketId = "B67832",
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = "http://localhost:5054/User/Index"
            };

            request.Buyer = new Iyzipay.Model.Buyer
            {
                Id = rental.UserId.ToString(),
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
                    Price = rental.Deposit.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                }
            };

            var checkoutForm = await Iyzipay.Model.CheckoutFormInitialize.Create(request, options);
            ViewBag.PaymentForm = checkoutForm.CheckoutFormContent;
            TempData["RentalData"] = JsonConvert.SerializeObject(rental);

            return View("IyzicoPayment");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentCallback(string token)
        {
            var rentalDataString = HttpContext.Session.GetString("RentalData");

            if (string.IsNullOrEmpty(rentalDataString))
                return RedirectToAction("Fail", new { message = "Kiralama verisi bulunamadı." });

            var rental = JsonConvert.DeserializeObject<Rental>(rentalDataString);

            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Fail", new { message = "Token gelmedi (iyzico callback hatası)" });

            var request = new Iyzipay.Request.RetrieveCheckoutFormRequest { Token = token };
            var result = await Iyzipay.Model.CheckoutForm.Retrieve(request, options);

            if (result.Status == "success" && result.PaymentStatus == "SUCCESS")
            {
                var rentalData = TempData["RentalData"] as string;
                if (string.IsNullOrEmpty(rentalData))
                    return RedirectToAction("Fail", new { message = "Kiralama verisi bulunamadı." });

                _rentalService.ConfirmAndSave(rental);

                TempData["Success"] = "Ödemeniz başarıyla alındı ve rezervasyonunuz oluşturuldu.";
                return RedirectToAction("Success");
            }
            else
            {
                string errorMessage = result.ErrorMessage ?? "Ödeme işlemi sırasında bir hata oluştu.";
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
    }
}