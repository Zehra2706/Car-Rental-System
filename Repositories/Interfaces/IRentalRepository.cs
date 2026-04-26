using rental.Models;
using System.Collections.Generic;
using System;
using user.Models;

namespace Car_reservation_automation_system.Repositories.Interfaces
{
    public interface IRentalRepository
    {
        List<Rental> GetActiveRentalsByCarId(int carId);
        void Delete(int id);
        void Add(Rental rental);
        void SaveChanges();
        List<Rental> GetBusyDates(int carId);
        bool CheckAvailability(int carId, DateTime start, DateTime end);
        Rental GetById(int rentalId);
        void Update(Rental rental);
        void GetRentalById(int rentalId);
        User GetUserByRental(int rentalId);
        List<Rental> GetAllActiveRentals();
        bool HasActiveRentalForCar(int carId);
        bool HasActiveRentalForUser(int userId);

        List<Rental> GetRentalsByUserId(int userId);
    }
}