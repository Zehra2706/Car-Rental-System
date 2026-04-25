using Microsoft.EntityFrameworkCore;
using car.Data;
using car.Models;
using Car_reservation_automation_system.Repositories.Interfaces;
using carFeature.Models;
using price.Models;
using rental.Models;
using user.Models;


public class CarRepository : ICarRepository
{
    private readonly ApplicationDbContext _context;

    public CarRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public double GetDailyPrice(int carId)
    {
        var priceObj = _context.Prices.FirstOrDefault(p => p.CarId == carId);


        return priceObj != null ? (double)priceObj.daily : 0;
    }

    public void AddCar(Car car)
    {
        _context.Cars.Add(car);
    }

    public List<Car> GetAllCars()
    {
        return _context.Cars
            .Include(x => x.Prices)
            .ToList();
    }

    public void DeleteCar(int id)
    {
        var car = _context.Cars
            .Include(c => c.CarFeatures)
            .Include(c => c.Prices)
            .Include(c => c.Rentals)
            .Include(c => c.Reviews)
            .FirstOrDefault(c => c.Id == id);

        if (car != null)
        {
            _context.CarFeatures.RemoveRange(car.CarFeatures);
            _context.Prices.RemoveRange(car.Prices);
            _context.Rentals.RemoveRange(car.Rentals);
            _context.Reviews.RemoveRange(car.Reviews);

            _context.Cars.Remove(car);

            _context.SaveChanges();
        }
    }

    public void AddCarFeature(CarFeature feature)
    {
        _context.CarFeatures.Add(feature);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public void AddPrice(Price price)
    {
        _context.Prices.Add(price);
    }
    public List<Car> GetUserCars(string email)
    {
        var userInfo = _context.UserInfo.FirstOrDefault(ui => ui.Email == email);
        if (userInfo == null) return new List<Car>();

        return _context.Cars
            .Include(c => c.Prices)
            .Where(c => c.UserId == userInfo.UserId)
            .ToList();
    }

    public Car? GetCarById(int id)
    {
        return _context.Cars
            .Include(c => c.CarFeatures)
            .Include(c => c.Prices)
            .FirstOrDefault(c => c.Id == id);
    }
    public List<Car> GetCarsByEmail(string email)
    {
        var userInfo = _context.UserInfo.FirstOrDefault(ui => ui.Email == email);
        if (userInfo == null) return new List<Car>();

        return _context.Cars
            .Include(c => c.Prices)
            .Where(c => c.UserId == userInfo.UserId)
            .ToList();
    }
    public string? GetCarsByUserId(int value)
    {
        var userInfo = _context.UserInfo.FirstOrDefault(ui => ui.UserId == value);
        if (userInfo == null) return null;

        var car = _context.Cars
            .Include(c => c.Prices)
            .FirstOrDefault(c => c.UserId == userInfo.UserId);

        return car?.Brand;
    }
    public bool CheckAvailability(int carId, DateTime start, DateTime end)
    {
        return !_context.Rentals.Any(r =>
            r.CarId == carId &&
            r.Status != "Reddedildi" &&
            start < r.ReturnDate &&
            end > r.Date
        );
    }

    public List<Rental> GetActiveRentalsByCarId(int carId)
    {
        return _context.Rentals
            .Where(r => r.CarId == carId && r.Status != "Reddedildi" && r.ReturnDate > DateTime.Now)
            .ToList();
    }

    public List<Car> FilterCars(CarFilter filter)
    {
        var query = _context.Cars
            .Include(c => c.CarFeatures)
            .Include(c => c.Prices)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Brand))
            query = query.Where(c => c.Brand.ToLower().Contains(filter.Brand.ToLower()));

        if (!string.IsNullOrEmpty(filter.ModelName))
            query = query.Where(c => c.ModelName.ToLower().Contains(filter.ModelName.ToLower()));

        if (!string.IsNullOrEmpty(filter.Location))
            query = query.Where(c => c.Location == filter.Location);

        if (filter.MinYear.HasValue)
            query = query.Where(c => c.ModelYear >= filter.MinYear.Value);

        if (filter.MaxYear.HasValue)
            query = query.Where(c => c.ModelYear <= filter.MaxYear.Value);

        if (filter.Transmission.HasValue)
        {
            query = query.Where(c =>
                c.CarFeatures != null &&
                c.CarFeatures.Any(f => f.Transmission == filter.Transmission.Value));
        }
        if (filter.FuelType.HasValue)
        {
            query = query.Where(c =>
                c.CarFeatures != null &&
                c.CarFeatures.Any(f => f.fuelType == filter.FuelType.Value));
        }

        if (filter.IsChauffeured.HasValue)
        {
            query = query.Where(c =>
                c.CarFeatures != null &&
                c.CarFeatures.Any(f => f.IsChauffeured == filter.IsChauffeured.Value));
        }
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(c =>
                c.Prices.Any(p => p.daily >= filter.MinPrice.Value));
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(c =>
                c.Prices.Any(p => p.daily <= filter.MaxPrice.Value));
        }
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(c =>
                c.Prices.Any(p => p.daily >= filter.MinPrice.Value));
        }
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(c => c.Prices.Any(p =>
                (filter.PriceType == "weekly" && p.weekly >= filter.MinPrice) ||
                (filter.PriceType == "monthly" && p.monthly >= filter.MinPrice) ||
                (filter.PriceType == "daily" && p.daily >= filter.MinPrice)
            ));
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(c => c.Prices.Any(p =>
                (filter.PriceType == "weekly" && p.weekly <= filter.MaxPrice) ||
                (filter.PriceType == "monthly" && p.monthly <= filter.MaxPrice) ||
                (filter.PriceType == "daily" && p.daily <= filter.MaxPrice)
            ));
        }
        return query.ToList();
    }

    public List<Car> SearchCars(string search)
    {
        return _context.Cars
            .Include(c => c.Prices)
            .Where(c =>
                c.Brand.Contains(search) ||
                c.ModelName.Contains(search) ||
                c.Plate.Contains(search))
            .ToList();
    }

    public User GetOwnerByCarId(int carId)
    {
        var car = _context.Cars.FirstOrDefault(x => x.Id == carId);

        if (car == null)
            return null;

        return _context.Users
            .Include(u => u.UserInfo)
            .FirstOrDefault(u => u.Id == car.UserId);
    }

}