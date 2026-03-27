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

}