// using user.Models;
// using userConnections.Models;
// using userInfo.Models;
// using static user.Models.User;

// public class UserService : IUserService
// {
//     private readonly IUserRepository _userRepository;

//     public UserService(IUserRepository userRepository)
//     {
//         _userRepository = userRepository;
//     }

//     // 🔹 LOGIN
// public User Login(string email, string password)
// {
//     var user = _userRepository.GetByEmail(email);

//     if (user?.UserInfo == null)
//         return null;

//     return user.UserInfo.Password == password ? user : null;
// }

//     // 🔹 REGISTER
// public void Register(RegisterViewModel model)
// {
//     // email var mı kontrol
//     var existingUser = _userRepository.GetByEmail(model.Email);

//     if (existingUser != null)
//         throw new Exception("Bu email zaten kayıtlı");

//     if (model.Password != model.ConfirmPassword)
//         throw new Exception("Şifreler uyuşmuyor");

//     var user = new User
//     {
//         Name = model.Name,
//         Surname = model.Surname,
//         UserRole = Role.Customer,
//         Date = DateTime.Now,

//         UserInfo = new UserInfo
//         {
//             Email = model.Email,
//             Password = model.Password
//         },

//         UserConnections = new UserConnections
//         {
//             Adress = model.Address,
//             Number = model.PhoneNumber
//         }
//     };

//     _userRepository.AddUser(user);
// }
// }
using licence.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
using static user.Models.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // 🔹 LOGIN
    public User Login(string email, string password)
    {
        User user = _userRepository.GetByEmail(email);

        // null kontrolü
        if (user == null )
            return null;

        // şifre kontrolü
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
            Password = model.Password // 🔥 BURASI KRİTİK
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
}