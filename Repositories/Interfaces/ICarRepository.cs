using car.Models;
using carFeature.Models;
using price.Models;

namespace Car_reservation_automation_system.Repositories.Interfaces
{
    public interface ICarRepository
    {
        void AddCar(Car car);
        void AddCarFeature(CarFeature feature);
        void AddPrice(Price price);
        void SaveChanges();
        List<Car> GetUserCars(string userEmail);
        List<Car> GetCarsByEmail(string email);
        Car GetCarById(int id);
        List<Car> GetAllCars();
        void DeleteCar(int id);
        string? GetCarsByUserId(int value);

    }
}