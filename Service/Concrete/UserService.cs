using licence.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
using car.ViewModels; // 🔥 ViewModel klasörünü buraya ekledik
using static user.Models.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // 🔹 LOGIN
    public User? Login(string email, string password)
    {
        // Detaylı getiriyoruz ki UserInfo.Password'e erişebilelim
        User? user = _userRepository.GetUserWithDetails(email);

        if (user == null) return null;

        if (user.UserInfo.Password != password)
            return null;

        return user;
    }

    // 🔹 REGISTER
    public void Register(RegisterViewModel model)
    {
        if (_userRepository.EmailExists(model.Email))
            throw new Exception("Bu email zaten kayıtlı");

        if (_userRepository.PhoneExists(model.PhoneNumber))
            throw new Exception("Bu telefon numarası zaten kayıtlı");

        if (_userRepository.LicenseExists(model.LicenseNumber))
            throw new Exception("Bu ehliyet numarası zaten kayıtlı");

        var user = new User
        {
            Name = model.Name,
            Surname = model.Surname,
            UserRole = Role.Customer,
            Date = DateTime.Now,

            UserInfo = new UserInfo
            {
                Email = model.Email,
                Password = model.Password
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

        _userRepository.AddUser(user);
    }

    // 🔹 GET PROFILE FOR EDIT
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

    // 🔹 UPDATE PROFILE
    public void UpdateProfile(EditProfileViewModel model, string email)
    {
        // 1. Önce veritabanındaki ASIL kullanıcıyı bul (ID'siyle beraber gelir)
        var existingUser = _userRepository.GetByEmail(email);

        if (existingUser != null)
        {
            // 2. Sadece değişen alanları üzerine yaz
            existingUser.Name = model.Name;
            existingUser.Surname = model.Surname;

            if (existingUser.UserConnections != null)
            {
                existingUser.UserConnections.Adress = model.Address;
                existingUser.UserConnections.Number = model.PhoneNumber;
            }

            // 3. Repository'deki Update'e gönder
            _userRepository.Update(existingUser);
        }
    }
}
