using licence.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
using car.ViewModels;
using static user.Models.User;
using car.Data;
using car.Models;
using rental.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace car.Service.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public UserService(IUserRepository userRepository, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        // --- GİRİŞ VE KAYIT İŞLEMLERİ ---
        public User? Login(string email, string password)
        {
            User? user = _userRepository.GetUserWithDetails(email);
            if (user == null) return null;
            if (user.UserInfo.Password != password) return null;
            return user;
        }

        public void Register(RegisterViewModel model)
        {
            if (_userRepository.EmailExists(model.Email)) throw new Exception("Bu email zaten kayıtlı");
            if (_userRepository.PhoneExists(model.PhoneNumber)) throw new Exception("Bu telefon numarası zaten kayıtlı");
            if (_userRepository.LicenseExists(model.LicenseNumber)) throw new Exception("Bu ehliyet numarası zaten kayıtlı");

            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                UserRole = Role.Customer,
                Date = DateTime.Now,
                UserInfo = new UserInfo { Email = model.Email, Password = model.Password },
                UserConnections = new UserConnections { Adress = model.Address, Number = model.PhoneNumber },
                Licence = new Licence { LicenceNumber = model.LicenseNumber }
            };
            _userRepository.AddUser(user);
        }

        // --- PROFİL İŞLEMLERİ ---
        public EditProfileViewModel GetProfileForEdit(string email)
        {
            var user = _userRepository.GetUserWithDetails(email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı");

            return new EditProfileViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.UserInfo.Email,
                PhoneNumber = user.UserConnections?.Number ?? "",
                Address = user.UserConnections?.Adress ?? "",
                LicenseNumber = user.Licence?.LicenceNumber ?? ""
            };
        }

        public void UpdateProfile(EditProfileViewModel model, string email)
        {
            var existingUser = _userRepository.GetByEmail(email);
            if (existingUser != null)
            {
                existingUser.Name = model.Name;
                existingUser.Surname = model.Surname;
                if (existingUser.UserConnections != null)
                {
                    existingUser.UserConnections.Adress = model.Address;
                    existingUser.UserConnections.Number = model.PhoneNumber;
                }
                _userRepository.Update(existingUser);
            }
        }

        // 1. Kendi taleplerim listesi için
        public List<rental.Models.Rental> GetMyRentalRequests(int userId)
        {
            return _context.Rentals
                .Include(r => r.Car) // 🚩 Plaka ve Araç ismi için şart!
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        // 2. Gelen istekler (Araç sahipleri) listesi için
        public List<rental.Models.Rental> GetIncomingRequests(int ownerId)
        {
            return _context.Rentals
                .Include(r => r.Car) // 🚩 Plaka ve Araç ismi için şart!
                .Where(r => r.Car.UserId == ownerId)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        // 3. Admin panelindeki tüm kiralamalar için
        public List<rental.Models.Rental> GetAllRentals()
        {
            return _context.Rentals
                .Include(r => r.Car)  // 🚩 Plaka bilgisi için
                .Include(r => r.User) // Kim kiralamış görmek için
                .OrderByDescending(r => r.Date)
                .ToList();
        }
        public List<car.Models.Car> GetAllCarsForUser()
        {
            return _context.Cars.ToList();
        }




        public void UpdateRentalStatus(int rentalId, string status)
        {
            var rental = _context.Rentals.Find(rentalId);
            if (rental != null)
            {
                rental.Status = status;
                _context.SaveChanges();
            }
        }
        public rental.Models.Rental GetReturnCalculation(int rentalId)
        {
            // 1. ADIM: Prices (çoğul) olarak Include ediyoruz
            var rental = _context.Rentals
                .Include(r => r.Car)
                    .ThenInclude(c => c.Prices) // Koleksiyon olduğu için 'Prices' yazdık
                .FirstOrDefault(r => r.Id == rentalId);

            if (rental == null || rental.Car == null || !rental.Car.Prices.Any())
                return rental;

            // 2. ADIM: Listedeki ilk (veya en güncel) fiyat kaydını alıyoruz
            var carPrice = rental.Car.Prices.FirstOrDefault();

            // 3. ADIM: Gün Sayısını Hesapla
            int totalDays = (rental.ReturnDate.Date - rental.Date.Date).Days;
            if (totalDays <= 0) totalDays = 1;

            double baseAmount = 0;

            // 4. ADIM: Fiyatlandırma Skalası (Küçük harflere dikkat: daily, weekly, monthly)
            if (totalDays >= 30)
            {
                baseAmount = (totalDays / 30.0) * carPrice.monthly;
            }
            else if (totalDays >= 7)
            {
                baseAmount = (totalDays / 7.0) * carPrice.weekly;
            }
            else
            {
                baseAmount = totalDays * carPrice.daily;
            }

            rental.Forecast = baseAmount;

            // 5. ADIM: Gecikme Faizi (%10)
            DateTime now = DateTime.Now;
            rental.RealReturnDate = now;

            if (now.Date > rental.ReturnDate.Date)
            {
                int delayDays = (now.Date - rental.ReturnDate.Date).Days;
                if (delayDays > 0)
                {
                    double penalty = delayDays * (baseAmount * 0.10);
                    rental.Forecast += penalty;
                }
            }

            return rental;
        }



        public void ConfirmReturnAndPayment(int rentalId, double totalPaid)
        {
            var rental = _context.Rentals.Find(rentalId);
            if (rental != null)
            {
                rental.IsReturned = true;
                rental.RealReturnDate = DateTime.Now;
                rental.Status = "Tamamlandı";
                rental.Forecast = totalPaid; // Faizli son tutarı kaydet
                _context.SaveChanges();
            }
        }

        public void ReturnCar(int rentalId)
        {
            var rental = _context.Rentals.Find(rentalId);
            if (rental != null)
            {
                rental.IsReturned = true;
                rental.RealReturnDate = DateTime.Now;
                rental.Status = "Tamamlandı";
                _context.SaveChanges();
            }
        }

        public List<User> GetAllUsers() => _userRepository.GetAllUsers();
        public void DeleteUser(int id) => _userRepository.DeleteUser(id);

        public void AddUser(AdminCreateUserViewModel model)
        {
            if (_userRepository.EmailExists(model.Email)) throw new Exception("Bu email zaten kayıtlı");
            if (_userRepository.PhoneExists(model.PhoneNumber)) throw new Exception("Bu telefon numarası zaten kayıtlı");
            if (_userRepository.LicenseExists(model.LicenseNumber)) throw new Exception("Bu ehliyet numarası zaten kayıtlı");

            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                UserRole = Role.Admin,
                Date = DateTime.Now,
                UserInfo = new UserInfo { Email = model.Email, Password = model.Password },
                UserConnections = new UserConnections { Adress = model.Address, Number = model.PhoneNumber },
                Licence = new Licence { LicenceNumber = model.LicenseNumber }
            };
            _userRepository.AddUser(user);
        }
    }
}