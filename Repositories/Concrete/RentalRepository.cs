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
        // 2. Yapılan işlemleri veritabanına (SQL) kaydeder
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        // 3. 🕒 SAAT BAZLI ÇAKIŞMA KONTROLÜ
        // Bu metot, veritabanında belirtilen saatler arasında başka bir kayıt olup olmadığına bakar.
        public bool CheckAvailability(int carId, DateTime start, DateTime end)
        {
            // Eğer Any() içindeki şart sağlanıyorsa (çakışma varsa) True döner. 
            // Biz önüne "!" koyarak: "Çakışma YOKSA True (müsait)" demiş oluyoruz.
            return !_context.Rentals.Any(r =>
                r.CarId == carId &&
                r.Status != "Reddedildi" && // İptal edilenler çakışma sayılmaz
                start < r.ReturnDate &&     // Yeni isteğin başlangıcı, eskinin bitişinden önceyse
                end > r.Date                // Yeni isteğin bitişi, eskinin başlangıcından sonraysa
            );
        }

        // 4. Aktif kiralamaları getirir (Takvimde engelli saatleri göstermek için kullanılır)
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

        // 5. Alternatif isimli metot (Eğer projenin başka yerlerinde bu isimle çağrılıyorsa)
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