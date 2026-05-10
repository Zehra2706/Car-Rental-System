using car.Data;
using car.Models;
using Microsoft.EntityFrameworkCore;
using user.Models;
using userInfo.Models;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public UserRepository(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();

        var role = new Roles
        {
            UserId = user.Id,
            RoleName = "User"
        };

        _context.Roles.Add(role);
        _context.SaveChanges();
    }

    public User GetByEmail(string email)
    {
        return _context.Users
            .Include(x => x.UserInfo)
            .Include(x => x.UserConnections)
            .FirstOrDefault(x => x.UserInfo.Email == email);
    }

    public User GetById(int id)
    {
        return _context.Users
            .Include(x => x.UserInfo)
            .FirstOrDefault(x => x.Id == id);
    }
    public bool EmailExists(string email)
    {
        return _context.UserInfo.Any(x => x.Email == email);
    }

    public bool PhoneExists(string phone)
    {
        return _context.UserConnections.Any(x => x.Number == phone);
    }

    public bool LicenseExists(string license)
    {
        return _context.Licences.Any(x => x.LicenceNumber == license);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }
    public bool TCExists(string tc)
    {
        return _context.Users.Any(x => x.TC == tc);
    }
    public User? GetUserWithDetails(string email)
    {
        return _context.Users
            .Include(x => x.UserInfo)
            .Include(x => x.UserConnections)
            .Include(x => x.Licence)
            .FirstOrDefault(x => x.UserInfo.Email == email);
    }
    public void DeleteRolesByUserId(int userId)
    {
        var roles = _context.Roles.Where(r => r.UserId == userId);
        _context.Roles.RemoveRange(roles);
    }
    public List<User> GetAllUsers()
    {
        return _context.Users
            .Include(x => x.UserInfo)
            .ToList();
    }
    public void DeleteUser(int userId)
    {
        var user = _context.Users
            .Include(x => x.UserInfo)
            .Include(x => x.UserConnections)
            .Include(x => x.Licence)
            .FirstOrDefault(x => x.Id == userId);

        if (user == null)
            throw new Exception("Kullanıcı bulunamadı");

        // kullanıcının arabaları
        var cars = _context.Cars
            .Where(c => c.UserId == userId)
            .ToList();

        var carIds = cars.Select(c => c.Id).ToList();

        // arabaya ait veriler
        var prices = _context.Prices.Where(p => carIds.Contains(p.CarId));
        var features = _context.CarFeatures.Where(f => carIds.Contains(f.CarId));
        var carRentals = _context.Rentals.Where(r => carIds.Contains(r.CarId));
        var carReviews = _context.Reviews.Where(r => carIds.Contains(r.CarId));

        // kullanıcının kendi kiralamaları
        var userRentals = _context.Rentals.Where(r => r.UserId == userId);

        // kullanıcının yaptığı yorumlar
        var userReviews = _context.Reviews.Where(r => r.UserId == userId);

        // silme işlemleri
        _context.Reviews.RemoveRange(carReviews);
        _context.Reviews.RemoveRange(userReviews);

        _context.Rentals.RemoveRange(carRentals);
        _context.Rentals.RemoveRange(userRentals);

        _context.Prices.RemoveRange(prices);
        _context.CarFeatures.RemoveRange(features);


        _context.Cars.RemoveRange(cars);

        _context.Users.Remove(user);

        _context.SaveChanges();

        _notificationService.AccountDeleted(user);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public string GeneratePasswordResetToken(string email)
    {
        throw new NotImplementedException();
    }

    public User GetUserByResetToken(string token)
    {
        throw new NotImplementedException();
    }

    public void ResetPassword(string token, string newPassword)
    {
        throw new NotImplementedException();
    }

    public void SendPasswordResetEmail(string email)
    {
        throw new NotImplementedException();
    }

}