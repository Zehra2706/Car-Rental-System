using user.Models;
using userInfo.Models;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }


    public User Login(string email, string password)
    {
        var userInfo = _userRepository.GetByEmail(email);

        if (userInfo == null)
            return null;

        if (userInfo.Password != password)
            return null;
        
        return userInfo.User; // 🔥 artık User döndürüyoruz
    }


    public void Register(User user)
    {
        _userRepository.Add(user);
    }
}