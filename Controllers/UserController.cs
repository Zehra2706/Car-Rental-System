using Microsoft.AspNetCore.Mvc;
using car.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq; // Filtreleme için gerekli

namespace car.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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

        // --- 🟢 GÜNCELLENEN KISIM: KENDİ TALEPLERİM & AKTİF KİRALAMALARIM ---
        public IActionResult MyRentals(string? status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // Servisten tüm listeyi çekiyoruz
            var myRequests = _userService.GetMyRentalRequests(userId.Value);

            // 💡 Melisa, eğer linkten 'Onaylandı' parametresi gelirse listeyi filtreliyoruz
            if (!string.IsNullOrEmpty(status))
            {
                myRequests = myRequests.Where(r => r.Status == status).ToList();
                ViewBag.PageTitle = status == "Onaylandı" ? "Aktif Kiralamalarım" : "Taleplerim";
            }
            else
            {
                ViewBag.PageTitle = "Tüm Taleplerim";
            }

            return View(myRequests);
        }

        // --- GELEN İSTEKLER (Araç Sahipleri İçin) ---
        public IActionResult IncomingRequests()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var incoming = _userService.GetIncomingRequests(userId.Value);
            return View(incoming);
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
        // 1. İade Özet Sayfası (Ödeme öncesi)
        [HttpPost]
        public IActionResult ProcessReturnPayment(int rentalId)
        {
            // Servis katmanında gecikme ve %10 faiz hesaplayan metodu çağırıyoruz
            var rentalCalc = _userService.GetReturnCalculation(rentalId);

            // Hesaplanan verilerle ödeme sayfasına gönderiyoruz
            return View("ReturnPayment", rentalCalc);
        }

        // 2. Ödeme Onay ve İade Tamamlama
        [HttpPost]
        public IActionResult FinalizeReturn(int rentalId, double finalAmount)
        {
            _userService.ConfirmReturnAndPayment(rentalId, finalAmount);
            TempData["Success"] = "Ödeme alındı ve iade işlemi başarıyla tamamlandı.";
            return RedirectToAction("MyRentals");
        }
    }
}