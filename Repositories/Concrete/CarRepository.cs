using car.Data;
using car.Models;
using Car_reservation_automation_system.Repositories.Interfaces;
using carFeature.Models;
using Microsoft.EntityFrameworkCore;
using price.Models;

public class CarRepository : ICarRepository
{
    private readonly ApplicationDbContext _context;

    public CarRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public void AddCar(Car car)
    {
        _context.Cars.Add(car);
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


}