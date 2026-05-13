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
using car.ViewModels;
using car.Data;

namespace car.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly ICarService _carService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        private static readonly ConcurrentDictionary<string, string> _paymentCache = new();
        private readonly INotificationService _notificationService;
        public RentalController(IRentalService rentalService, ICarService carService, IConfiguration configuration, IUserService userService, INotificationService notificationService, ApplicationDbContext context)
        {
            _rentalService = rentalService;
            _carService = carService;
            _configuration = configuration;
            _userService = userService;
            _notificationService = notificationService;
            _context = context;
        }

        public IActionResult Success(int carId, bool showReview = true)
        {
            var model = new RentalSuccessViewModel
            {
                CarId = carId,
            };
            ViewBag.ShowReview = showReview;
            return View(model);
        }

        [HttpGet]
        public IActionResult Create(int carId)
        {
            if (carId == 0) return Content("ID gelmedi!");

            var car = _carService.GetCarById(carId);

            if (car == null) Console.WriteLine("VERİTABANINDA BU ID İLE ARABA YOK!");

            ViewBag.Car = car;
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
        [ValidateAntiForgeryToken]
        public IActionResult SendRequest(Rental rental)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // YENİ KONTROL
            if (!_rentalService.CanUserRentCar(userId.Value))
            {
                TempData["Error"] = "Aktif kiralamanız var. Araç iade edilmeden yeni araç kiralayamazsınız.";
                return RedirectToAction("Index", "User");
            }

            if (!_rentalService.IsAvailable(rental.CarId, rental.Date, rental.ReturnDate))
            {
                TempData["Error"] = "Seçtiğiniz tarihler arasında araç artık müsait değil!";
                return RedirectToAction("Create", new { carId = rental.CarId });
            }

            var car = _carService.GetCarById(rental.CarId);

            if (car.UserId == userId.Value)
            {
                TempData["Error"] = "Kendi aracınızı kiralayamazsınız.";
                return RedirectToAction("Create", new { carId = rental.CarId });
            }

            rental.UserId = userId.Value;
            rental.Status = "OnayBekliyor";
            rental.IsReturned = false;

            var user = _userService.TGetById(userId.Value);
            _rentalService.ConfirmAndSave(user, rental);

            TempData["Success"] = "Kiralama talebiniz başarıyla iletildi!";
            return RedirectToAction("Index", "User");

        }


        [HttpGet]
        public JsonResult GetPriceCalculation(int carId, int days)
        {
            var result = _rentalService.CalculatePrice(carId, days);
            return Json(new { total = result.total, deposit = result.deposit });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartPayment(int CarId, DateTime Date, DateTime ReturnDate, string Forecast, string Deposit, bool IsContractApproved = false)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (!IsContractApproved)
            {
                TempData["Error"] = "Sözleşmeyi onaylamadan ödeme adımına geçemezsiniz!";
                return RedirectToAction("Create", new { carId = CarId });
            }

            var rental = new Rental
            {
                CarId = CarId,
                Date = Date,
                ReturnDate = ReturnDate,
                UserId = userId.Value,
                IsContractApproved = IsContractApproved // 🚩 Pakete ekledik, callback'te DB'ye gidecek
            };


            var culture = System.Globalization.CultureInfo.InvariantCulture;

            if (double.TryParse(Forecast?.Replace(",", "."), System.Globalization.NumberStyles.Any, culture, out double parsedForecast))
            {
                rental.Forecast = (decimal)parsedForecast;
            }

            if (double.TryParse(Deposit?.Replace(",", "."), System.Globalization.NumberStyles.Any, culture, out double parsedDeposit))
            {
                rental.Deposit = (decimal)parsedDeposit;
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
                CallbackUrl = "http://arackiralama-env.eba-qpyhhmb2.eu-north-1.elasticbeanstalk.com/Rental/PaymentCallback"
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

            //_paymentCache[checkoutForm.Token] = JsonConvert.SerializeObject(rental);
            rental.Status = "OdemeBekliyor";
            rental.PaymentToken = checkoutForm.Token;
            _context.Rentals.Add(rental);
            _context.SaveChanges();
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

            //if (!_paymentCache.TryGetValue(token, out string rentalDataString))
            //{
            //    return RedirectToAction("Fail", new { message = "Kiralama verisi bulunamadı." });
            //}
            //var rentalDataString = HttpContext.Session.GetString("PendingRental_" + token);
            //if (string.IsNullOrEmpty(rentalDataString))
            //    return RedirectToAction("Fail", new { message = "Kiralama verisi bulunamadı." });

            //var rental = JsonConvert.DeserializeObject<Rental>(rentalDataString);

            var rental = _context.Rentals.FirstOrDefault(r => r.PaymentToken == token);
            if (rental == null)
                return RedirectToAction("Fail", new { message = "Kiralama verisi bulunamadı." });


            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            var request = new Iyzipay.Request.RetrieveCheckoutFormRequest
            {
                Token = token
            };

            var result = await Iyzipay.Model.CheckoutForm.Retrieve(request, options);

            if (result.Status == "success" && result.PaymentStatus == "SUCCESS")
            {
                var car = _carService.GetCarForEdit(rental.CarId);

                if (car.UserId == rental.UserId)
                {
                    rental.PaymentToken = null;
                    _context.SaveChanges();
                    return RedirectToAction("Fail", new { message = "Kendi aracınızı kiralayamazsınız." });
                }

                if (!_rentalService.CanUserRentCar(rental.UserId))
                {
                    rental.PaymentToken = null;
                    _context.SaveChanges();
                    return RedirectToAction("Fail", new { message = "Zaten aktif kiralamanız var" });
                }

                rental.Status = "OnayBekliyor";

                _rentalService.UpdateRental(rental);

                var user = _userService.TGetById(rental.UserId);

                // araç sahibini bul
                var owner = _userService.TGetById(car.UserId);

                _notificationService.RentalCreated(user, rental);
                _notificationService.DepositPaid(user, rental);

                // araç sahibine bildirim
                _notificationService.NewRentalRequest(owner, rental);
                _notificationService.OwnerDepositInfo(owner, rental);

                HttpContext.Session.Remove("PendingRental_" + token);

                TempData["Success"] = "Ödemeniz başarıyla alındı!";
                return RedirectToAction("Success", new { carId = rental.CarId, showReview = false });
            }
            else
            {
                HttpContext.Session.Remove("PendingRental_" + token);

                string errorMessage = result.ErrorMessage ?? "Ödeme hatası.";
                TempData["Error"] = errorMessage;

                return RedirectToAction("Fail", new { message = errorMessage });
            }
        }

        [HttpGet]
        public IActionResult Fail(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelRequest(int rentalId)
        {

            _rentalService.CancelRentalRequest(rentalId);

            TempData["Success"] = "Kiralama talebiniz başarıyla iptal edildi.";
            return RedirectToAction("MyRentals", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartReturnPayment(int rentalId)
        {
        

            var rental = _rentalService.GetRentalById(rentalId);
            if (rental == null)
                return RedirectToAction("Fail", new { message = "Kiralama kaydı bulunamadı." });

            var calc = _rentalService.GetPaymentBreakdown(rental);

            decimal baseAmount = calc.baseAmount;
            decimal penalty = calc.penalty;
            decimal totalAmount = calc.total;

            var culture = System.Globalization.CultureInfo.InvariantCulture;

            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            var request = new Iyzipay.Request.CreateCheckoutFormInitializeRequest
            {
                Locale = Iyzipay.Model.Locale.TR.ToString(),
                ConversationId = "RETURN_" + rentalId + "_" + Guid.NewGuid().ToString("N").Substring(0, 6),

                Price = totalAmount.ToString("0.00", culture),
                PaidPrice = totalAmount.ToString("0.00", culture),
                Currency = Iyzipay.Model.Currency.TRY.ToString(),

                BasketId = "R" + rentalId,
                PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = "http://arackiralama-env.eba-qpyhhmb2.eu-north-1.elasticbeanstalk.com/Rental/ReturnCallback"
            };

            request.Buyer = new Iyzipay.Model.Buyer
            {
                Id = rental.UserId.ToString(),
                Name = "User",
                Surname = "Test",
                Email = "test@test.com",
                IdentityNumber = "11111111111",
                RegistrationAddress = "Adres",
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                City = "Istanbul",
                Country = "Turkey"
            };

            var address = new Iyzipay.Model.Address
            {
                ContactName = "User Test",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Adres"
            };

            request.BillingAddress = address;
            request.ShippingAddress = address;

            request.BasketItems = new List<Iyzipay.Model.BasketItem>
    {
        new Iyzipay.Model.BasketItem
        {
            Id = "FINAL_" + rentalId,
            Name = "Kiralama + Gecikme Bedeli",
            Category1 = "Car Rental",
            ItemType = Iyzipay.Model.BasketItemType.PHYSICAL.ToString(),
            Price = totalAmount.ToString("0.00", culture)
        }
    };

            var checkoutForm = await Iyzipay.Model.CheckoutFormInitialize.Create(request, options);

            if (checkoutForm.Status == "success")
            {
                var rentalToUpdate = _context.Rentals.Find(rentalId);
                rentalToUpdate.PaymentToken = checkoutForm.Token;
                _context.SaveChanges();

                ViewBag.PaymentForm = checkoutForm.CheckoutFormContent;
                ViewBag.Rental = rental;
                return View("IyzicoPayment");
            }


            return RedirectToAction("Fail", new { message = checkoutForm.ErrorMessage });
        }
        [HttpGet]
        public IActionResult ReturnPaymentConfirm(int rentalId)
        {
            var rental = _rentalService.GetRentalById(rentalId);
            rental.IsPaid = true;
            var calc = _rentalService.GetPaymentBreakdown(rental);

            ViewBag.BaseAmount = calc.baseAmount;
            ViewBag.Penalty = calc.penalty;
            ViewBag.Total = calc.total;
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");
            var rentals = _rentalService.GetMyRentals(userId.Value);

            return View(rental);
        }
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReturnCallback(string token)
        {

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Fail", new { message = "Ödeme anahtarı (token) bulunamadı." });

                var rentalIdStr = HttpContext.Session.GetString("ReturnRental_" + token);
                if (string.IsNullOrEmpty(rentalIdStr))
                    return RedirectToAction("Fail", new { message = "Ödeme oturumu bulunamadı." });
                //HttpContext.Session.Remove("ReturnRental_" + token);

            int rentalId = int.Parse(rentalIdStr);

            // 3. iyzico Ayarları
            var options = new Iyzipay.Options
            {
                ApiKey = _configuration["Iyzipay:ApiKey"],
                SecretKey = _configuration["Iyzipay:SecretKey"],
                BaseUrl = _configuration["Iyzipay:BaseUrl"]
            };

            var request = new Iyzipay.Request.RetrieveCheckoutFormRequest { Token = token };
            var result = await Iyzipay.Model.CheckoutForm.Retrieve(request, options);
            var rental = _rentalService.GetRentalById(rentalId);
            if (result.Status == "success" && result.PaymentStatus == "SUCCESS")
            {
                
                if (rental != null)
                {
                    rental.Status = "Ödendi";
                    rental.IsPaid = true;
                    rental.PaymentToken = null;
                    var user = _userService.TGetById(rental.UserId);

                    var car = _carService.GetCarForEdit(rental.CarId);
                    var owner = _userService.TGetById(car.UserId);

                    _rentalService.UpdateRental(rental); // Veritabanına işle
                    _notificationService.RentalFinished(user, rental);
                    _notificationService.OwnerRentalFinished(owner, rental);

                    TempData["Success"] = "Ödeme başarıyla alındı!";
                    return RedirectToAction("Success", new { carId = rental.CarId });
                }

                // Güvenlik için token'ı cache'den temizle
                //rental.PaymentToken = null;
                //_context.SaveChanges();


                //TempData["Success"] = "Ödeme başarıyla alındı ve iade işleminiz tamamlandı. Keyifli sürüşler!";
                return RedirectToAction("Fail", new { message = "Kiralama bulunamadı." });
            }
            else
            {
                rental.PaymentToken = null;
                _context.SaveChanges();
                string errorMessage = result.ErrorMessage ?? "Ödeme işlemi reddedildi.";
                TempData["Error"] = errorMessage;
                return RedirectToAction("Fail", new { message = errorMessage });
            }
        }

        public IActionResult PaymentForm(int rentalId)
        {
            var rental = _context.Rentals.FirstOrDefault(x => x.Id == rentalId);

            return View("IyzicoPayment", rental);
        }



    }
}