using car.ViewModels;
using car.Models;
using carFeature.Models;
using Car_reservation_automation_system.Service.Interfaces;
using Car_reservation_automation_system.Repositories.Interfaces;
using price.Models;
using Microsoft.EntityFrameworkCore;

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

        public List<Car> GetAllCars()
        {
            return _carRepository.GetAllCars();
        }

        public void DeleteCar(int id)
        {
            _carRepository.DeleteCar(id);
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

                IsInsured = model.IsInsured,
                Plate = model.Plate
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
                CarId = car.Id,
                daily = (float)model.DailyPrice,
                weekly = (float)model.WeeklyPrice,
                monthly = (float)model.MonthlyPrice
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
                Plate = car.Plate,
                UserId = car.UserId,
                IsInsured = car.IsInsured,

                EngineSize = feature?.engineSize ?? 0,
                Transmission = feature?.Transmission ?? 0,
                FuelType = feature?.fuelType ?? 0,
                MotorInsurance = feature?.motorInsurance ?? string.Empty,
                IsChauffeured = feature?.IsChauffeured ?? false,

                DailyPrice = (double)(price?.daily ?? 0),
                WeeklyPrice = (double)(price?.weekly ?? 0),
                MonthlyPrice = (double)(price?.monthly ?? 0)
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
            car.Plate = model.Plate;
            car.IsInsured = model.IsInsured;

            if (!string.IsNullOrEmpty(model.ImagePath))
                car.ImagePath = model.ImagePath;

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
                price.daily = (float)model.DailyPrice;
                price.weekly = (float)model.WeeklyPrice;
                price.monthly = (float)model.MonthlyPrice;
            }

            _carRepository.SaveChanges();
        }


        public List<Car> GetAllCarsForUser()
        {
            return _carRepository.GetAllCars();
        }

        public (decimal TotalPrice, decimal DepositAmount) CalculateRentalFee(int carId, DateTime start, DateTime end)
        {
            var car = _carRepository.GetCarById(carId);
            var price = car?.Prices?.FirstOrDefault();

            int totalDays = (end - start).Days;
            if (totalDays <= 0) totalDays = 1;

            decimal selectedRate = 0;
            if (price != null)
            {
                if (totalDays >= 30) selectedRate = (decimal)price.monthly;
                else if (totalDays >= 7) selectedRate = (decimal)price.weekly;
                else selectedRate = (decimal)price.daily;
            }

            decimal totalPrice = totalDays * selectedRate;
            decimal deposit = totalPrice * 0.10m;

            return (totalPrice, deposit);
        }

        public List<Car> GetCarsByUserId(int userId)
        {
            var allCars = _carRepository.GetAllCars();
            if (allCars == null) return new List<Car>();

            return allCars.Where(c => c.UserId == userId).ToList();
        }
        public List<Car> FilterCars(CarFilter filter)
        {
             return _carRepository.FilterCars(filter);
        }

    }
}