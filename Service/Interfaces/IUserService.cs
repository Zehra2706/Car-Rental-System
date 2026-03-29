using car.ViewModels;
using user.Models;

public interface IUserService
{
    User Login(string email, string password);
    void Register(RegisterViewModel model);
    EditProfileViewModel GetProfileForEdit(string email);
    
    void UpdateProfile(EditProfileViewModel model, string email);

    List<User> GetAllUsers();
    void DeleteUser(int id);
    void AddUser(AdminCreateUserViewModel model);
}