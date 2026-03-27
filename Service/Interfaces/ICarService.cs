
using car.Models;
using car.ViewModels;

namespace Car_reservation_automation_system.Service.Interfaces
{
    public interface ICarService
    {
        void AddNewCar(CarCreateViewModel model);
        List<Car> GetUserCars(string userEmail);

        CarCreateViewModel GetCarForEdit(int id);
        void UpdateCar(CarCreateViewModel model);
        List<Car> GetCarsByEmail(string email);
    }
}