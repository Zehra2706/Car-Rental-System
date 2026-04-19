using car.ViewModels;
using user.Models;
using rental.Models;
using System.Collections.Generic;

public interface IUserService
{
    User? Login(string email, string password);
    void Register(RegisterViewModel model);
    EditProfileViewModel GetProfileForEdit(string email);
    void UpdateProfile(EditProfileViewModel model, string email);
    List<rental.Models.Rental> GetIncomingRequests(int ownerId);
    void CancelRentalRequest(int rentalId);

    User TGetById(int id);
    void UpdateRentalStatus(int rentalId, string status);
    void ReturnCar(int rentalId);
    List<Rental> GetMyRentalRequests(int userId);
    rental.Models.Rental GetReturnCalculation(int rentalId);
    void ConfirmReturnAndPayment(int rentalId, double totalPaid);
    List<User> GetAllUsers();
    void DeleteUser(int id);
    void AddUser(AdminCreateUserViewModel model);
    List<car.Models.Car> GetAllCarsForUser();
    List<rental.Models.Rental> GetAllRentals();

}