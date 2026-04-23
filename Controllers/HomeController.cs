using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using car.Models;
// Servisini kullanabilmek için Interface yolunu ekliyoruz:
using Car_reservation_automation_system.Service.Interfaces;

namespace car.Controllers;

public class HomeController : Controller
{
    private readonly ICarService _carService;

    // 1. Servisi kullanabilmek için Constructor (Yapıcı Metot) içine alıyoruz
    public HomeController(ICarService carService)
    {
        _carService = carService;
    }

    public IActionResult Index()
    {
        // 2. Veritabanından araçların listesini çekiyoruz
        // (Eğer senin servisinde tüm araçları çeken metodun adı farklıysa burayı değiştir, örn: GetCars() vb.)
        var carList = _carService.GetAllCars();

        // 3. Çektiğimiz listeyi (Modeli) sayfanın içine koyup öyle açıyoruz!
        return View(carList);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}