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
        void ConfirmAndSave(User user, Rental rental);
        Rental GetRentalById(int id);

        List<object> GetDisabledDatesJson(int carId);
        List<Rental> GetActiveRentalsByCarId(int carId);

        bool IsAvailable(int carId, DateTime start, DateTime end);

        public (decimal baseAmount, decimal penalty, decimal total) GetPaymentBreakdown(Rental rental);
        double GetDailyPrice(int carId);

        User GetUserByRental(int rentalId);
        void UpdateRental(Rental rental);

        void ApproveRental(int rentalId);
        void RejectedRental(int rentalId);

        (decimal total, decimal deposit) CalculatePrice(int carId, int days);
        (decimal total, decimal deposit) CalculateHourlyPrice(int carId, double hours);

        void CheckLateRentals();
        void CheckEndingSoonRentals();
        bool CanUserRentCar(int userId);
    }
}