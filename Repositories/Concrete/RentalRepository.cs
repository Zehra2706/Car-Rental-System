using car.Models;
using car.Data;
using Car_reservation_automation_system.Repositories.Interfaces;
using rental.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Car_reservation_automation_system.Repositories.Concrete
{
    public class RentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext _context;
        public RentalRepository(ApplicationDbContext context) => _context = context;

        public void Add(Rental rental) => _context.Rentals.Add(rental);

        public void SaveChanges() => _context.SaveChanges();

        public List<Rental> GetBusyDates(int carId)
        {
            return _context.Rentals
                .Where(r => r.CarId == carId && r.Status != "Reddedildi")
                .ToList();
        }

        public bool CheckAvailability(int carId, DateTime start, DateTime end)
        {
            // Bu mantık tarih çakışmalarını kontrol eder
            return !_context.Rentals.Any(r =>
                r.CarId == carId &&
                r.Status != "Reddedildi" &&
                start < r.ReturnDate &&
                end > r.Date);
        }

    }
}