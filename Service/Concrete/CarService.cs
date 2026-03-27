using car.ViewModels;
using car.Models;
using carFeature.Models;
using Car_reservation_automation_system.Service.Interfaces;
using Car_reservation_automation_system.Repositories.Interfaces;
using price.Models;

namespace car.Service.Concrete
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;

        public CarService(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        public List<Car> GetUserCars(string userEmail)
        {
            return _carRepository.GetCarsByEmail(userEmail);
        }
        public List<Car> GetCarsByEmail(string email)
        {
            return _carRepository.GetCarsByEmail(email);
        }

        public void AddNewCar(CarCreateViewModel model)
        {

            var car = new Car
            {
                Brand = model.Brand,
                ModelName = model.ModelName,
                ModelYear = model.ModelYear,
                Color = model.Color,
                Location = model.Location,
                Description = model.Description,
                ImagePath = model.ImagePath,
                UserId = model.UserId,
                IsInsured = true
            };

            _carRepository.AddCar(car);
            _carRepository.SaveChanges();


            var feature = new CarFeature
            {
                CarId = car.Id,
                engineSize = model.EngineSize,
                Transmission = model.Transmission,
                fuelType = model.FuelType,
                motorInsurance = model.MotorInsurance,
                IsChauffeured = model.IsChauffeured
            };

            _carRepository.AddCarFeature(feature);

            var priceInfo = new Price
            {
                daily = model.DailyPrice,
                weekly = model.WeeklyPrice,
                monthly = model.MonthlyPrice
            };


            _carRepository.AddPrice(priceInfo);


            _carRepository.SaveChanges();
        }
        public CarCreateViewModel GetCarForEdit(int id)
        {

            var car = _carRepository.GetCarById(id);
            if (car == null) return null;


            var feature = car.CarFeatures?.FirstOrDefault();
            var price = car.Prices?.FirstOrDefault();

            return new CarCreateViewModel
            {
                Id = car.Id,
                Brand = car.Brand,
                ModelName = car.ModelName,
                ModelYear = car.ModelYear,
                Color = car.Color,
                Location = car.Location,
                Description = car.Description,
                ImagePath = car.ImagePath,

                EngineSize = feature?.engineSize ?? 0,
                Transmission = feature?.Transmission ?? 0,
                FuelType = feature?.fuelType ?? 0,
                MotorInsurance = feature?.motorInsurance,
                IsChauffeured = feature?.IsChauffeured ?? false,

                // Ücretler
                DailyPrice = price?.daily ?? 0,
                WeeklyPrice = price?.weekly ?? 0,
                MonthlyPrice = price?.monthly ?? 0
            };
        }
        public void UpdateCar(CarCreateViewModel model)
        {
            var car = _carRepository.GetCarById(model.Id);
            if (car == null) return;

            car.Brand = model.Brand;
            car.ModelName = model.ModelName;
            car.ModelYear = model.ModelYear;
            car.Color = model.Color;
            car.Location = model.Location;
            car.Description = model.Description;

            if (!string.IsNullOrEmpty(model.ImagePath))
            {
                car.ImagePath = model.ImagePath;
            }

            var feature = car.CarFeatures?.FirstOrDefault();
            if (feature != null)
            {
                feature.engineSize = model.EngineSize;
                feature.Transmission = model.Transmission;
                feature.fuelType = model.FuelType;
                feature.motorInsurance = model.MotorInsurance;
                feature.IsChauffeured = model.IsChauffeured;
            }

            var price = car.Prices?.FirstOrDefault();
            if (price != null)
            {
                price.daily = model.DailyPrice;
                price.weekly = model.WeeklyPrice;
                price.monthly = model.MonthlyPrice;
            }

            _carRepository.SaveChanges();

        }
    }
}