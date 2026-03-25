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


    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public User GetById(int id)
    {
        return _context.Users.Find(id);
    }

    public UserInfo GetByEmail(string email)
    {
        return _context.UserInfo
            .Include(x => x.User) // User tablosunu da çekiyoruz
            .FirstOrDefault(x => x.Email == email);
    }


    User IUserRepository.GetById(int id)
    {
        throw new NotImplementedException();
    }
}