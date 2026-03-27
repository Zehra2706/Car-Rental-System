using Microsoft.AspNetCore.Mvc;
using car.ViewModels; // ViewModel klasörünü görmesi için

namespace car.Controllers
{
    public class UserController : Controller
    {
        // 1. Servisi buraya tanıtıyoruz (Aşçıyı mutfağa çağırdık)
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // --- Sayfalar ---

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Sayfa açıldığında mevcut bilgileri servisten çekip kutucuklara doldurmalıyız
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
                // Bilgileri servise gönderip güncelletiyoruz
                _userService.UpdateProfile(model, email);
                TempData["Success"] = "Profiliniz başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}