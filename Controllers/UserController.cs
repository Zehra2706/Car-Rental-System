using Microsoft.AspNetCore.Mvc;
using car.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Car_reservation_automation_system.Service.Interfaces;

namespace car.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRentalService _rentalService;

        public UserController(IUserService userService, IRentalService rentalService)
        {
            _userService = userService;
            _rentalService = rentalService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- PROFİL DÜZENLEME ---
        [HttpGet]
        public IActionResult EditProfile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");
            var model = _userService.GetProfileForEdit(email);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");
            if (ModelState.IsValid)
            {
                _userService.UpdateProfile(model, email);
                TempData["Success"] = "Profiliniz başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // --- ARAÇ LİSTELEME ---
        [HttpGet]
        public IActionResult AvailableCars()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");
            var cars = _userService.GetAllCarsForUser();
            return View(cars);
        }

        public IActionResult MyRentals(string? filter)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Servisten kullanıcının tüm taleplerini çekiyoruz
            var myRequests = _userService.GetMyRentalRequests(userId.Value);

            if (filter == "aktif")
            {
                myRequests = myRequests.Where(r => r.Status == "Onaylandı" || r.Status == "OnayBekliyor" || r.Status == "Onaylandi" || r.Status == "Gecikmis").ToList();
                ViewBag.PageTitle = "Aktif Kiralamalarım ve Taleplerim";
            }
            else
            {
                ViewBag.PageTitle = "Tüm Kiralama Geçmişim";
            }

            return View(myRequests);
        }


        public IActionResult IncomingRequests(string? filter)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var incomingRequests = _userService.GetIncomingRequests(userId.Value);

            bool isAktifMod = filter == "aktif";
            if (isAktifMod)
            {
                incomingRequests = incomingRequests.Where(r => r.Status == "OnayBekliyor").ToList();
                ViewBag.PageTitle = "Aktif Gelen Talepler";
            }
            else
            {
                ViewBag.PageTitle = "Gelen Talepler Geçmişi";
            }

            return View(incomingRequests);
        }

        [HttpPost]
        public IActionResult UpdateRequestStatus(int rentalId, string status)
        {
            _userService.UpdateRentalStatus(rentalId, status);
            return RedirectToAction("IncomingRequests");
        }

        [HttpPost]
        public IActionResult ReturnCar(int rentalId)
        {
            _userService.ReturnCar(rentalId);
            TempData["Success"] = "Araç başarıyla iade edildi. Bizi tercih ettiğiniz için teşekkürler!";
            return RedirectToAction("MyRentals", new { status = "Onaylandı" });
        }

        [HttpPost]
        public IActionResult ProcessReturnPayment(int rentalId)
        {
            var rentalCalc = _userService.GetReturnCalculation(rentalId);

            return View("ReturnPayment", rentalCalc);
        }

        [HttpPost]
        public IActionResult FinalizeReturn(int rentalId, double finalAmount)
        {
            _userService.ConfirmReturnAndPayment(rentalId, finalAmount);
            TempData["Success"] = "Ödeme alındı ve iade işlemi başarıyla tamamlandı.";
            return RedirectToAction("MyRentals");
        }
        [HttpPost]
        public IActionResult ApproveRental(int rentalId)
        {
            _rentalService.ApproveRental(rentalId);
            return RedirectToAction("IncomingRequests", new { filter = "aktif" });
        }

        [HttpPost]
        public IActionResult RejectedRental(int rentalId)
        {
            _rentalService.RejectedRental(rentalId);
            return RedirectToAction("IncomingRequests", new { filter = "aktif" });
        }

    }
}