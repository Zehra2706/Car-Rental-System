using rental.Models;
using System.Collections.Generic;
using System;

namespace Car_reservation_automation_system.Repositories.Interfaces
{
    public interface IRentalRepository
    {
        // 🟢 Hataları düzelten eksik tanımlamalar:
        void Add(Rental rental);
        void SaveChanges();
        List<Rental> GetBusyDates(int carId);
        bool CheckAvailability(int carId, DateTime start, DateTime end);
    }
}