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
using Microsoft.AspNetCore.Authorization;

namespace car.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        private static readonly ConcurrentDictionary<string, string> _paymentCache = new();
private readonly INotificationService _notificationService;
        public RentalController(IRentalService rentalService, ICarService carService, IConfiguration configuration, IUserService userService, INotificationService notificationService)
        {
            _rentalService = rentalService;
            _carService = carService;
            _configuration = configuration;
            _userService = userService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Create(int carId)
        {
            var car = _carService.GetCarForEdit(carId);
            if (car == null) return Content("Araç bulunamadı!");

            var seller = _userService.TGetById(car.UserId);

            if (seller != null)
            {
                ViewBag.SellerName = seller.Name + " " + seller.Surname;

                ViewBag.SellerEmail = seller.UserInfo?.Email ?? "E-posta yok";

                ViewBag.SellerPhone = seller.UserConnections?.Number ?? "Telefon yok";
            }

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

            // _rentalService.UpdateRental(rental);
            var user = _userService.TGetById(userId.Value);
            _rentalService.ConfirmAndSave(user, rental);

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

            _rentalService.UpdateRental(rental);

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
        [AllowAnonymous]
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

                rental.Status = "OnayBekliyor";    // onay bekliyor değiştirdim.
                _rentalService.UpdateRental(rental);
                var user = _userService.TGetById(rental.UserId);
                var car = _carService.GetCarForEdit(rental.CarId);

                // araç sahibini bul
                var owner = _userService.TGetById(car.UserId);

                _notificationService.RentalCreated(user, rental);
                _notificationService.DepositPaid(user, rental);

                // araç sahibine mail
                _notificationService.NewRentalRequest(owner, rental);
                _notificationService.OwnerDepositInfo(owner, rental);
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

            _rentalService.CancelRentalRequest(rentalId);

            TempData["Success"] = "Kiralama talebiniz başarıyla iptal edildi.";
            return RedirectToAction("MyRentals", "User");
        }
        [HttpPost]
        public async Task<IActionResult> StartReturnPayment(int rentalId)
        {
            // 1. Veritabanından kiralama kaydını çek
            var rental = _rentalService.GetRentalById(rentalId);
            if (rental == null) return RedirectToAction("Fail", new { message = "Kiralama kaydı bulunamadı." });

            // 2. Ceza Hesaplama (Güvenlik için Controller'da tekrar hesaplıyoruz)
            double penalty = 0;
            int delayDays = 0;
            if (DateTime.Now > rental.ReturnDate)
            {
                var timeDiff = DateTime.Now - rental.ReturnDate;
                delayDays = timeDiff.Days;
                if (delayDays <= 0 && timeDiff.TotalHours > 0) delayDays = 1; // 1 saat bile geçse 1 gün say

                penalty = delayDays * (rental.Forecast * 0.10); // %10 ceza formülü
            }

            double totalAmount = rental.Forecast + penalty;
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            // 3. iyzico Seçeneklerini Yapılandır (appsettings.json'dan okur)
            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            // 4. Ödeme Formu İsteği Oluştur
            var request = new Iyzipay.Request.CreateCheckoutFormInitializeRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = "RETURN_" + rentalId + "_" + Guid.NewGuid().ToString().Substring(0, 5),
                Price = totalAmount.ToString("0.00", culture),
                PaidPrice = totalAmount.ToString("0.00", culture),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),
                BasketId = "R" + rentalId,
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),
                // 🚩 KRİTİK: Ödeme bitince bu metodumuza geri dönecek
                CallbackUrl = "http://localhost:5054/Rental/ReturnCallback"
            };

            // 5. Alıcı Bilgileri (Burayı veritabanındaki kullanıcı verileriyle besliyoruz)
            request.Buyer = new Iyzipay.Model.Buyer
            {
                Id = rental.UserId.ToString(),
                Name = "Melisa", // Normalde rental.User.Name olmalı
                Surname = "User",
                Email = "test@test.com",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Adres Bilgisi",
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                City = "Istanbul",
                Country = "Turkey"
            };

            // 6. Adres Bilgileri
            var address = new Iyzipay.Model.Address
            {
                ContactName = "Melisa User",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Adres Bilgisi"
            };
            request.BillingAddress = address;
            request.ShippingAddress = address;

            // 7. Sepet Kalemleri (Faturada ne görünecek?)
            request.BasketItems = new List<Iyzipay.Model.BasketItem>
    {
        new Iyzipay.Model.BasketItem
        {
            Id = "FINAL_PAYMENT_" + rentalId,
            Name = "Araç Kiralama ve Gecikme Bedeli",
            Category1 = "Car Rental",
            ItemType = Iyzipay.Model.BasketItemType.PHYSICAL.ToString(),
            Price = totalAmount.ToString("0.00", culture)
        }
    };

            // 8. iyzico'dan Formu İste
            var checkoutForm = await Iyzipay.Model.CheckoutFormInitialize.Create(request, options);

            if (checkoutForm.Status == "success")
            {
                // Token'ı RentalId ile eşleştirip cache'e atıyoruz
                _paymentCache[checkoutForm.Token] = rentalId.ToString();

                // iyzico'nun HTML formunu View'a gönderiyoruz
                ViewBag.PaymentForm = checkoutForm.CheckoutFormContent;
                return View("IyzicoPayment");
            }

            // Hata varsa Fail sayfasına yönlendir
            return RedirectToAction("Fail", new { message = checkoutForm.ErrorMessage });
        }
        [HttpPost]
        [AllowAnonymous] // iyzico'nun dönüş yapabilmesi için dışarıya açık olmalı
        [IgnoreAntiforgeryToken] // iyzico'dan gelen POST isteği CSRF token içermez, bu şart
        public async Task<IActionResult> ReturnCallback(string token)
        {
            // 1. Token kontrolü: iyzico'dan bir anahtar geldi mi?
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Fail", new { message = "Ödeme anahtarı (token) bulunamadı." });

            // 2. Cache kontrolü: Bu token hangi kiralama (rentalId) içindi?
            if (!_paymentCache.TryGetValue(token, out string rentalIdStr))
                return RedirectToAction("Fail", new { message = "Ödeme oturumu zaman aşımına uğradı veya bulunamadı." });

            int rentalId = int.Parse(rentalIdStr);

            // 3. iyzico Ayarları
            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            // 4. iyzico'ya sor: "Bu token'lı işlem gerçekten başarılı mı?"
            var request = new Iyzipay.Request.RetrieveCheckoutFormRequest { Token = token };
            var result = await Iyzipay.Model.CheckoutForm.Retrieve(request, options);

            // 5. Durumu kontrol et
            if (result.Status == "success" && result.PaymentStatus == "SUCCESS")
            {
                // ✅ ÖDEME BAŞARILI! Şimdi veritabanı şov başlasın.
                var rental = _rentalService.GetRentalById(rentalId);
                if (rental != null)
                {
                    rental.IsReturned = true;           // Araba geri geldi
                    rental.RealReturnDate = DateTime.Now; // Tam olarak şu an teslim edildi
                    rental.Status = "Tamamlandı";      // Statüyü kapatıyoruz
                    var user = _userService.TGetById(rental.UserId);

                    var car = _carService.GetCarForEdit(rental.CarId);
                    var owner = _userService.TGetById(car.UserId);

                    _rentalService.UpdateRental(rental); // Veritabanına işle
                    _notificationService.RentalFinished(user, rental);
                    _notificationService.OwnerRentalFinished(owner, rental);
                }

                // Güvenlik için token'ı cache'den temizle
                _paymentCache.TryRemove(token, out _);

                TempData["Success"] = "Ödeme başarıyla alındı ve iade işleminiz tamamlandı. Keyifli sürüşler!";
                return RedirectToAction("Success");
            }
            else
            {
                // ❌ ÖDEME BAŞARISIZ
                _paymentCache.TryRemove(token, out _);
                string errorMessage = result.ErrorMessage ?? "Ödeme işlemi banka tarafından reddedildi.";
                TempData["Error"] = errorMessage;

                return RedirectToAction("Fail", new { message = errorMessage });
            }
        }


    }
}