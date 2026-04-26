using Car_reservation_automation_system.Service.Interfaces;
using Car_reservation_automation_system.Repositories.Interfaces;
using rental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using user.Models;

namespace car.Service.Concrete
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepo;
        private readonly ICarRepository _carRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        public RentalService(
            IRentalRepository rentalRepo,
            ICarRepository carRepo,
            INotificationService notificationService,
            IUserRepository userRepository,
            IUserService userService)
        {
            _rentalRepo = rentalRepo;
            _carRepo = carRepo;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _userService = userService;
        }

        public bool IsAvailable(int carId, DateTime start, DateTime end)
        {
            if (start >= end || start < DateTime.Now.AddMinutes(-10)) return false;
            return _rentalRepo.CheckAvailability(carId, start, end);
        }


        public (decimal baseAmount, decimal penalty, decimal total) GetPaymentBreakdown(Rental rental)
        {
            decimal baseAmount = rental.Forecast;

            decimal penalty = 0m;

            if (DateTime.Now.Date > rental.ReturnDate.Date)
            {
                int delayDays = (DateTime.Now.Date - rental.ReturnDate.Date).Days;
                if (delayDays <= 0) delayDays = 1;

                penalty = delayDays * (rental.Forecast * 0.03m);
            }

            decimal total = baseAmount + penalty;

            return (baseAmount, penalty, total);
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
        public (decimal total, decimal deposit) CalculatePrice(int carId, int days)
        {
            var car = _carRepo.GetCarById(carId);

            decimal daily = (decimal)car.Prices.FirstOrDefault()?.daily;
            decimal weekly = (decimal)car.Prices.FirstOrDefault()?.weekly;
            decimal monthly = (decimal)car.Prices.FirstOrDefault()?.monthly;

            decimal total = 0;

            if (days < 7)
            {
                total = daily * days;
            }
            else if (days < 30)
            {
                int weeks = days / 7;
                int remainingDays = days % 7;

                total = (weeks * weekly) + (remainingDays * daily);
            }
            else
            {
                int months = days / 30;
                int remainingDays = days % 30;

                total = (months * monthly) + (remainingDays >= 7 ? weekly : daily * remainingDays);
            }

            decimal deposit = total * 0.10m;

            return (total, deposit);
        }
        public (decimal total, decimal deposit) CalculateHourlyPrice(int carId, double hours)
        {
            // Günlük fiyatı decimal olarak alıyoruz (GetDailyPrice'ın dönüşü double ise başına (decimal) ekledik)
            decimal gunlukFiyat = (decimal)_carRepo.GetDailyPrice(carId);

            // 24.0'ın yanına 'm' koyarak onu da decimal yapıyoruz
            decimal saatlikFiyat = gunlukFiyat / 24.0m;

            // hours double olduğu için onu da işleme sokarken (decimal) diye dönüştürüyoruz
            decimal total = saatlikFiyat * (decimal)hours;

            // 0.10'un yanına 'm' koyarak decimal çarpması yapıyoruz
            decimal deposit = total * 0.10m;

            return (total, deposit);
        }


        public void ConfirmAndSave(User user, Rental rental)
        {
            _rentalRepo.Add(rental);
            _rentalRepo.SaveChanges();

            var car = _carRepo.GetCarById(rental.CarId);

            if (car == null)
            {
                Console.WriteLine("CAR BULUNAMADI");
                return;
            }

            Console.WriteLine("CAR OWNER ID: " + car.UserId);

            var owner = _userService.GetById(car.UserId);

            if (owner == null)
            {
                Console.WriteLine("OWNER NULL");
                return;
            }

            if (owner.UserInfo == null)
            {
                Console.WriteLine("OWNER USERINFO NULL");
                return;
            }

            Console.WriteLine("OWNER EMAIL: " + owner.UserInfo.Email);

            rental.Car = car;

            // kiralayana
            _notificationService.RentalCreated(user, rental);
            _notificationService.DepositPaid(user, rental);

            // araç sahibine
            _notificationService.NewRentalRequest(owner, rental);
            _notificationService.OwnerDepositInfo(owner, rental);
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
            Rental rental = _rentalRepo.GetById(rentalId);

            if (rental != null)
            {
                var user = _rentalRepo.GetUserByRental(rental.Id);

                if (rental.Status == "OnayBekliyor")
                {
                    _rentalRepo.Delete(rentalId);

                    if (user != null)
                    {
                        _notificationService.RentalRejected(user, rental);
                    }
                }
            }
        }

        public Rental GetRentalById(int id)
        {
            var rental = _rentalRepo.GetById(id);

            if (rental != null)
            {
                if (rental.Status == "Onaylandı" && DateTime.Now >= rental.Date && DateTime.Now <= rental.ReturnDate)
                {
                    rental.Status = "Aktif";
                    _rentalRepo.Update(rental);
                    _rentalRepo.SaveChanges();
                }
            }

            return rental;
        }
        public void UpdateRental(Rental rental)
        {

            if (!CanUserRentCar(rental.UserId))
                throw new Exception("User already has active rental");

            _rentalRepo.Update(rental);
            _rentalRepo.SaveChanges();

        }

        public User GetUserByRental(int rentalId)
        {
            return _rentalRepo.GetUserByRental(rentalId);
        }
        public void ApproveRental(int rentalId)
        {
            Console.WriteLine("APPROVE RENTAL CALISTI");

            var rental = _rentalRepo.GetById(rentalId);

            if (rental == null)
            {
                Console.WriteLine("Rental bulunamadı");
                return;
            }

            rental.Status = "Onaylandı";

            _rentalRepo.Update(rental);
            _rentalRepo.SaveChanges();

            Console.WriteLine("STATUS UPDATE EDILDI");

            var user = _rentalRepo.GetUserByRental(rentalId);

            if (user == null)
            {
                Console.WriteLine("USER NULL");
                return;
            }

            if (user.UserInfo == null)
            {
                Console.WriteLine("USERINFO NULL");
                return;
            }

            Console.WriteLine("MAIL GONDERILIYOR: " + user.UserInfo.Email);

            _notificationService.RentalApproved(user, rental);
        }
        public void RejectedRental(int rentalId)
        {
            Console.WriteLine("REJECT RENTAL CALISTI");

            var rental = _rentalRepo.GetById(rentalId);

            if (rental == null)
                return;

            rental.Status = "Reddedildi";

            _rentalRepo.Update(rental);
            _rentalRepo.SaveChanges();

            var user = _rentalRepo.GetUserByRental(rentalId);

            if (user?.UserInfo?.Email == null)
            {
                Console.WriteLine("EMAIL BULUNAMADI");
                return;
            }

            Console.WriteLine("MAIL GONDERILIYOR: " + user.UserInfo.Email);

            _notificationService.RentalRejected(user, rental);
        }

        public void CheckLateRentals()
        {
            var rentals = _rentalRepo.GetAllActiveRentals();

            foreach (var rental in rentals)
            {
                if (DateTime.Now > rental.ReturnDate && rental.Status == "Aktif")
                {
                    rental.Status = "Gecikmis";

                    _rentalRepo.Update(rental);
                    _rentalRepo.SaveChanges();

                    var user = _userService.TGetById(rental.UserId);

                    _notificationService.LatePenalty(user, rental);
                }
            }
        }

        public void CheckEndingSoonRentals()
        {
            var rentals = _rentalRepo.GetAllActiveRentals();

            foreach (var rental in rentals)
            {
                var timeLeft = rental.ReturnDate - DateTime.Now;

                if (timeLeft.TotalMinutes <= 60 && !rental.ReminderSent)
                {
                    var user = _userService.TGetById(rental.UserId);

                    _notificationService.RentalEndingSoon(user, rental);

                    rental.ReminderSent = true;

                    _rentalRepo.Update(rental);
                    _rentalRepo.SaveChanges();
                }
            }
        }
        public bool CanUserRentCar(int userId)
        {
            return !_rentalRepo.HasActiveRentalForUser(userId);
        }


    }
}