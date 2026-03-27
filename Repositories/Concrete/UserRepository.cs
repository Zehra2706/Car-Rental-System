using car.Data;
using Microsoft.EntityFrameworkCore;
using user.Models;
using userInfo.Models;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }


    // public void AddUserInfo(UserInfo userInfo)
    // {
    //     throw new NotImplementedException();
    // }

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
        // _context.Users.Add(user); <--- Sakın bunu yapma!
        _context.Users.Update(user); // Bu "olanı bul ve değiştir" demektir.
        _context.SaveChanges();
    }
    public User? GetUserWithDetails(string email)
    {
        // Mevcut GetByEmail ile aynı işi yapar ama 
        // isimlendirme karmaşası olmasın diye bunu da ekliyoruz
        return _context.Users
            .Include(x => x.UserInfo)
            .Include(x => x.UserConnections)
            .Include(x => x.Licence) // Ehliyet tablosu için
            .FirstOrDefault(x => x.UserInfo.Email == email);
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}