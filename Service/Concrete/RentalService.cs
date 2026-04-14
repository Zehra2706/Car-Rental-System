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

        // --- 1. FONKSİYON: TAKVİM İÇİN DOLU TARİHLERİ GETİR ---
        // Flatpickr takviminin anlayacağı { from: '...', to: '...' } formatında döner
        public List<object> GetDisabledDatesJson(int carId)
        {
            var busyDates = _rentalRepo.GetBusyDates(carId);
            return busyDates.Select(r => new
            {
                from = r.Date.ToString("yyyy-MM-dd"),
                to = r.ReturnDate.ToString("yyyy-MM-dd")
            }).Cast<object>().ToList();
        }

        public bool IsAvailable(int carId, DateTime start, DateTime end)
        {
            return _rentalRepo.CheckAvailability(carId, start, end);
        }

        public (double total, double deposit) CalculatePrice(int carId, int days)
        {
            var car = _carRepo.GetCarById(carId);

            if (car == null || car.Prices == null)
            {
                return (0, 0);
            }

            var price = car.Prices.FirstOrDefault();

            if (price == null)
            {
                return (0, 0);
            }

            double selectedRate;
            if (days >= 30) selectedRate = (double)price.monthly;
            else if (days >= 7) selectedRate = (double)price.weekly;
            else selectedRate = (double)price.daily;

            double total = days * selectedRate;
            double deposit = total * 0.10;

            return (total, deposit);
        }
        public void ConfirmAndSave(Rental rental)
        {
            // Kaydetmeden hemen önce bir kez daha kontrol (Double-check)
            if (!IsAvailable(rental.CarId, rental.Date, rental.ReturnDate))
            {
                throw new Exception("Üzgünüz, bu tarihler arasında araç artık müsait değil.");
            }

            _rentalRepo.Add(rental);
            _rentalRepo.SaveChanges();
        }
    }
}