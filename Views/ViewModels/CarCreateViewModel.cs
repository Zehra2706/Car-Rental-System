using carFeature.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // 🚩 Bunu mutlaka ekle!

namespace car.ViewModels
{
    public class CarCreateViewModel
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int ModelYear { get; set; }
        public int UserId { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Plaka kısmı - string? yaparak sistemin "boş kaldı" demesini engelliyoruz
        public string? Plate { get; set; }

        public double DailyPrice { get; set; }
        public double WeeklyPrice { get; set; }
        public double MonthlyPrice { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImagePath { get; set; }
        public bool IsInsured { get; set; }

        // Teknik Özellikler (Bunlar formdan tek tek gelecek)
        public double EngineSize { get; set; }
        public carFeature.Models.CarFeature.TransmissionType Transmission { get; set; }
        public carFeature.Models.CarFeature.FuelType FuelType { get; set; }
        public string MotorInsurance { get; set; } = string.Empty;
        public bool IsChauffeured { get; set; }

        // 🚩 İŞTE KRİTİK DÜZELTME:
        // Bu nesne formdan gelmediği için validasyonu kapatıyoruz
        [ValidateNever]
        public carFeature.Models.CarFeature? CarFeatures { get; set; }
    }
}