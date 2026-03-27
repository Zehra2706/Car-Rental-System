using user.Models;
using userInfo.Models;


public interface IUserRepository
{
    void AddUser(User user);
    // void AddUserInfo(UserInfo userInfo);
    User GetById(int id);
    User GetByEmail(string email);
    bool EmailExists(string email);
    bool PhoneExists(string phone);
    bool LicenseExists(string license);
    User GetUserWithDetails(string email); // UserInfo ve Connection ile birlikte getirir
    void Update(User user);
    void Save();

}