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
    public User? GetUserWithDetails(string email)
    {
        return _context.Users
            .Include(x => x.UserInfo)
            .Include(x => x.UserConnections)
            .Include(x => x.Licence)
            .FirstOrDefault(x => x.UserInfo.Email == email);
    }

    public List<User> GetAllUsers()
    {
        return _context.Users
            .Include(x => x.UserInfo)
            .ToList();
    }

    public void DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
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