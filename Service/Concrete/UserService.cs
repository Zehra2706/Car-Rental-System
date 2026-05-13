using licence.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
using car.ViewModels;
// using static user.Models.User; // removed to avoid ambiguity with nested User.Role
using car.Data;
using car.Models;
using Microsoft.AspNetCore.Identity;
using rental.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Car_reservation_automation_system.Repositories.Interfaces;
using Car_reservation_automation_system.Repositories.Concrete;

namespace car.Service.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly ICarRepository _carRepository;
        private readonly INotificationService _notificationService;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        public UserService(IUserRepository userRepository, ApplicationDbContext context, EmailService emailService, ICarRepository carRepository, INotificationService notificationService)
        {
            _userRepository = userRepository;
            _context = context;
            _emailService = emailService;
            _carRepository = carRepository;
            _notificationService = notificationService;
        }

        public User? Login(string email, string password)
        {
            User? user = _userRepository.GetUserWithDetails(email);
            if (user == null) return null;

            if (!user.UserInfo.Password.StartsWith("AQAAAA"))
            {
                if (user.UserInfo.Password != password) return null;
                return user;
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.UserInfo.Password, password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        public void Register(RegisterViewModel model)
        {
            var userRole = _context.Roles.FirstOrDefault(r => r.RoleName == "User");
            if (userRole == null) throw new Exception("User rolü bulunamadı");
            // 1. Önce Kullanıcıyı (User) tüm alt bilgileriyle oluşturuyoruz (Ama henüz Rol yok)
            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                TC = model.TC,
                Date = DateTime.Now,
                RoleId = userRole.Id, // direkt ata
                UserInfo = new UserInfo
                {
                    Email = model.Email,
                    Password = _passwordHasher.HashPassword(null, model.Password)
                },
                UserConnections = new UserConnections
                {
                    Adress = model.Address,
                    Number = model.PhoneNumber
                },
                Licence = new Licence
                {
                    LicenceNumber = model.LicenseNumber
                }
            };

            // 2. Kullanıcıyı veritabanına ekliyoruz. Bu işlem bize "user.Id"yi verecek.
            _context.Users.Add(user);
            _context.SaveChanges();
        }
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

        public List<rental.Models.Rental> GetMyRentalRequests(int userId)
        {
            return _context.Rentals
                .Include(r => r.Car)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Date)
                .ToList();
        }


        public List<Rental> GetIncomingRequests(int userId)
        {
            return _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .ThenInclude(u => u.UserInfo)
                .Where(r => r.Car.UserId == userId)
                .ToList();
        }


        public List<rental.Models.Rental> GetAllRentals()
        {
            return _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .OrderBy(r => r.ReturnDate) 
                .ToList();
        }
        public List<car.Models.Car> GetAllCarsForUser()
        {
            return _context.Cars
                .Where(c => c.IsActive) 
                .ToList();
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
            var rental = _context.Rentals
                .Include(r => r.Car)
                    .ThenInclude(c => c.Prices)
                .FirstOrDefault(r => r.Id == rentalId);

            if (rental == null || rental.Car == null || !rental.Car.Prices.Any())
                return rental;

            var carPrice = rental.Car.Prices.FirstOrDefault();

            int totalDays = (rental.ReturnDate.Date - rental.Date.Date).Days;
            if (totalDays <= 0) totalDays = 1;

            decimal baseAmount;

            if (totalDays >= 30)
                baseAmount = ((decimal)totalDays / 30m) * (decimal)carPrice.monthly;
            else if (totalDays >= 7)
                baseAmount = ((decimal)totalDays / 7m) * (decimal)carPrice.weekly;
            else
                baseAmount = (decimal)totalDays * (decimal)carPrice.daily;

          
            decimal penalty = 0m;

            if (DateTime.Now.Date > rental.ReturnDate.Date)
            {
                int delayDays = (DateTime.Now.Date - rental.ReturnDate.Date).Days;
                if (delayDays <= 0) delayDays = 1;

                penalty = delayDays * (baseAmount * 0.03m); // %3 günlük
            }

            // ❗ SADECE BASE TUT
            rental.Forecast = baseAmount;

            // ekstra alan gibi düşün (UI için)
            rental.RealReturnDate = DateTime.Now;

            return rental;
        }
        public void ConfirmReturnAndPayment(int rentalId, decimal totalPaid)
        {
            var rental = _context.Rentals.Find(rentalId);
            if (rental != null)
            {
                rental.IsReturned = true;
                rental.RealReturnDate = DateTime.Now;
                rental.Status = "Tamamlandı";
                rental.Forecast = totalPaid; // decimal = decimal, sorun yok!
                _context.SaveChanges();

                var user = _context.Users.Find(rental.UserId);
                _notificationService.RentalFinished(user, rental);

                var owner = _carRepository.GetOwnerByCarId(rental.CarId);
                _notificationService.OwnerRentalFinished(owner, rental);
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
        public void DeleteUser(int userId)
        {

            if (!CanDeleteUser(userId))
                throw new Exception();

            _userRepository.DeleteUser(userId);
        }
        public void AddUser(AdminCreateUserViewModel model)
        {
            if (_userRepository.EmailExists(model.Email)) throw new Exception("Bu email zaten kayıtlı");
            if (_userRepository.PhoneExists(model.PhoneNumber)) throw new Exception("Bu telefon numarası zaten kayıtlı");
            if (_userRepository.LicenseExists(model.LicenseNumber)) throw new Exception("Bu ehliyet numarası zaten kayıtlı");

            var adminRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
            if (adminRole == null) throw new Exception("Admin rolü bulunamadı");

            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                RoleId = adminRole.Id, // direkt ata
                Date = DateTime.Now,
                UserConnections = new UserConnections { Adress = model.Address, Number = model.PhoneNumber },
                Licence = new Licence { LicenceNumber = model.LicenseNumber }
            };

            var hashedPassword = _passwordHasher.HashPassword(user, model.Password);

            user.UserInfo = new UserInfo
            {
                Email = model.Email,
                Password = hashedPassword // Hashlenmiş şifre
            };
            //_userRepository.AddUser(user);
            //var role = new car.Models.Role
            //{
            //    UserId = user.Id,
            //    RoleName = user.UserRole.ToString()
            //};

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public user.Models.User TGetById(int id)
        {
            return _context.Users
                .Include(u => u.UserInfo)
                .Include(u => u.UserConnections)
                .FirstOrDefault(u => u.Id == id);
        }
       


        public string GeneratePasswordResetToken(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null) return null;

            var token = Guid.NewGuid().ToString();

            user.UserInfo.ResetToken = token;
            user.UserInfo.ResetTokenExpire = DateTime.Now.AddMinutes(30);

            _userRepository.Update(user);

            return token;
        }
        public User GetUserByResetToken(string token)
        {
            return _context.Users
                .Include(u => u.UserInfo)
                .FirstOrDefault(u =>
                    u.UserInfo != null &&
                    u.UserInfo.ResetToken == token &&
                    u.UserInfo.ResetTokenExpire > DateTime.Now);
        }
        public void ResetPassword(string token, string newPassword)
        {
            var user = GetUserByResetToken(token);

            if (user == null)
                throw new Exception("Token geçersiz veya süresi dolmuş");

            var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
            user.UserInfo.Password = hashedPassword;

            user.UserInfo.ResetToken = null;
            user.UserInfo.ResetTokenExpire = null;

            _userRepository.Update(user);
        }

        public User GetByEmail(string email)
        {
            return _context.Users
                .Include(u => u.UserInfo)
                .FirstOrDefault(u => u.UserInfo.Email == email);
        }

        public User GetByResetToken(string token)
        {
            return _context.Users
                .Include(u => u.UserInfo)
                .FirstOrDefault(u =>
                    u.UserInfo.ResetToken == token &&
                    u.UserInfo.ResetTokenExpire > DateTime.Now);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void SendPasswordResetEmail(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null) return;

            var token = Guid.NewGuid().ToString();

            user.UserInfo.ResetToken = token;
            user.UserInfo.ResetTokenExpire = DateTime.Now.AddMinutes(30);

            _userRepository.Update(user);

            var link = $"http://arackiralama-env.eba-qpyhhmb2.eu-north-1.elasticbeanstalk.com/Auth/ResetPassword?token={token}";

            var body = $@"
    <div style='font-family: Arial, sans-serif; line-height:1.6; color:#333'>

        <h2 style='color:#2c3e50;'>Şifre Sıfırlama Talebi</h2>

        <p>Sayın kullanıcı,</p>

        <p>Hesabınız için bir şifre sıfırlama talebi oluşturulmuştur.</p>

        <p>
            Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayabilirsiniz:
        </p>

        <p>
            <a href='{link}' style='color:#1a73e8; font-weight:bold;'>
                Şifremi Sıfırla
            </a>
        </p>

        <p style='color:#d35400; font-weight:bold;'>
            ⚠ Bu bağlantı yalnızca 30 dakika geçerlidir.
        </p>

        <p>
            Eğer bu işlemi siz yapmadıysanız, bu e-postayı dikkate almayınız.
        </p>

        <br/>

        <p>Saygılarımızla,<br/>
        <b>Araç Kiralama Sistemi Ekibi</b></p>

    </div>
    ";

            _emailService.SendEmail(email, "Şifre Sıfırlama Talebi", body);
        }

        public User GetById(int id)
        {
            return _context.Users
                .Include(u => u.UserInfo)
                .Include(u => u.UserConnections)
                .FirstOrDefault(u => u.Id == id);
        }


        public bool CanDeleteUser(int userId)
        {
            // kullanıcının kendi kiralamaları
            var activeRentals = _context.Rentals
                .Any(r => r.UserId == userId &&
                (r.Status == "Onaylandı" || r.Status == "OnayBekliyor"));

            if (activeRentals)
                return false;

            // kullanıcının arabaları
            var userCarIds = _context.Cars
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .ToList();

            // bu arabaların aktif kiralamaları
            var carActiveRentals = _context.Rentals
                .Any(r => userCarIds.Contains(r.CarId) &&
                (r.Status == "Onaylandı" || r.Status == "OnayBekliyor"));

            if (carActiveRentals)
                return false;

            return true;
        }

        public void CancelRentalRequest(int rentalId)
        {
            throw new NotImplementedException();
        }

        public void ConfirmReturnAndPayment(int rentalId, double totalPaid)
        {
            throw new NotImplementedException();
        }
    }
}