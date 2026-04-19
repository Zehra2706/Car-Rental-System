using car.Data;
using car.Models;
using Car_reservation_automation_system.Repositories.Interfaces;
using carFeature.Models;
using Microsoft.EntityFrameworkCore;
using price.Models;
using rental.Models;

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
        var car = _context.Cars.Find(id);

        if (car != null)
        {
            var features = _context.CarFeatures.Where(x => x.CarId == id);
            _context.CarFeatures.RemoveRange(features);

            var prices = _context.Prices.Where(x => x.CarId == id);
            _context.Prices.RemoveRange(prices);

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

    public Car GetCarById(int id)
    {
        return _context.Cars
            .Include(c => c.Prices)
            .Include(c => c.CarFeatures)
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
}