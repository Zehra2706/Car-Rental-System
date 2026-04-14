using car.Models;
using System;
using System.Collections.Generic;

namespace Car_reservation_automation_system.Service.Interfaces
{
    public interface IRentalService
    {
        void ConfirmAndSave(rental.Models.Rental rental);

        List<object> GetDisabledDatesJson(int carId);

        bool IsAvailable(int carId, DateTime start, DateTime end);

        (double total, double deposit) CalculatePrice(int carId, int days);
    }
}