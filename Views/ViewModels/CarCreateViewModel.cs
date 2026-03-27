using carFeature.Models;
using Microsoft.AspNetCore.Http; // IFormFile için 

namespace car.ViewModels
{
    public class CarCreateViewModel
    {

        public string Brand { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int ModelYear { get; set; }
        public int UserId { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Id { get; set; }
        public double DailyPrice { get; set; }
        public double WeeklyPrice { get; set; }
        public double MonthlyPrice { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImagePath { get; set; }

        public double EngineSize { get; set; }
        public CarFeature.TransmissionType Transmission { get; set; }
        public CarFeature.FuelType FuelType { get; set; }
        public string MotorInsurance { get; set; } = string.Empty;
        public bool IsChauffeured { get; set; }
    }
}