using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using car.Models;
using Car_reservation_automation_system.Service.Interfaces;

namespace car.Controllers;

public class HomeController : Controller
{
    private readonly ICarService _carService;

    public HomeController(ICarService carService)
    {
        _carService = carService;
    }

    public IActionResult Index()
    {
        var carList = _carService.GetAllCars();

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