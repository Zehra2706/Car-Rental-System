using Car_reservation_automation_system.Service.Interfaces;
using Car_reservation_automation_system.Repositories.Interfaces;
using rental.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace car.Service.Concrete
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepo;
        private readonly ICarRepository _carRepo;

        public RentalService(IRentalRepository rentalRepo, ICarRepository carRepo)
        {
            _rentalRepo = rentalRepo;
            _carRepo = carRepo;
        }

        public bool IsAvailable(int carId, DateTime start, DateTime end)
        {
            if (start >= end || start < DateTime.Now.AddMinutes(-10)) return false;
            return _rentalRepo.CheckAvailability(carId, start, end);
        }


        public List<object> GetDisabledDatesJson(int carId)
        {
            var rentals = _rentalRepo.GetActiveRentalsByCarId(carId);

            return rentals.Select(r => (object)new
            {
                from = r.Date.ToString("yyyy-MM-dd HH:mm"),
                to = r.ReturnDate.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
        }

        public (double total, double deposit) CalculateHourlyPrice(int carId, double hours)
        {
            double gunlukFiyat = _carRepo.GetDailyPrice(carId);
            double saatlikFiyat = gunlukFiyat / 24.0;
            double total = saatlikFiyat * hours;
            double deposit = total * 0.10;
            return (total, deposit);
        }

        public (double total, double deposit) CalculatePrice(int carId, int days)
        {
            double gunlukFiyat = _carRepo.GetDailyPrice(carId);
            double total = gunlukFiyat * days;
            double deposit = total * 0.10;
            return (total, deposit);
        }

        public void ConfirmAndSave(Rental rental)
        {
            _rentalRepo.Add(rental);
            _rentalRepo.SaveChanges();
        }

        public List<Rental> GetActiveRentalsByCarId(int carId)
        {
            return _rentalRepo.GetActiveRentalsByCarId(carId);
        }

        public double GetDailyPrice(int carId)
        {
            return _carRepo.GetDailyPrice(carId);
        }
        public void CancelRentalRequest(int rentalId)
        {

            Rental rental = (Rental)_rentalRepo.GetById(rentalId);

            if (rental != null && rental.Status == "OnayBekliyor")
            {
                _rentalRepo.Delete(rentalId);
            }
        }

        public Rental GetRentalById(int id)
        {
            return _rentalRepo.GetActiveRentalsByCarId(0)
                              .Concat(_rentalRepo.GetBusyDates(0))
                              .FirstOrDefault(r => r.Id == id);
        }
    }
}