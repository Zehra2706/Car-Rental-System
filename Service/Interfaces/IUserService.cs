using user.Models;

public interface IUserService
{
    User Login(string email, string password);
    void Register(RegisterViewModel model);
}