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

        public RentalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Yeni kiralama ekler
        public void Add(Rental rental)
        {
            _context.Rentals.Add(rental);
        }
        public Rental GetById(int id)
        {
            return _context.Rentals.Find(id);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
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
                .Where(r => r.CarId == carId && r.Status != "Reddedildi")
                .ToList();
        }
        public void Delete(int id)
        {
            var rental = _context.Rentals.Find(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                _context.SaveChanges();
            }
        }


        public List<Rental> GetBusyDates(int carId)
        {
            return GetActiveRentalsByCarId(carId);
        }

        object IRentalRepository.GetById(int rentalId)
        {
            return GetById(rentalId);
        }
    }
}