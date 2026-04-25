using car.Models;
using rental.Models;
using System;
using System.Collections.Generic;
using user.Models;

namespace Car_reservation_automation_system.Service.Interfaces
{
    public interface IRentalService
    {
        void CancelRentalRequest(int rentalId);
        void ConfirmAndSave(User user,Rental rental);
        Rental GetRentalById(int id);
        List<object> GetDisabledDatesJson(int carId);
        List<rental.Models.Rental> GetActiveRentalsByCarId(int carId);
        bool IsAvailable(int carId, DateTime start, DateTime end);

        (double total, double deposit) CalculatePrice(int carId, int days);
        double GetDailyPrice(int carId);

        User GetUserByRental(int rentalId);
        void UpdateRental(Rental rental);

        void ApproveRental(int rentalId);
        void RejectedRental(int rentalId);

        (double total, double deposit) CalculateHourlyPrice(int carId, double hours);

        void CheckLateRentals();
        void CheckEndingSoonRentals();
        bool CanUserRentCar(int userId);
    }
}