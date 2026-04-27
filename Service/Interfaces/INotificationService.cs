using rental.Models;
using user.Models;

public interface INotificationService
{
    void RentalCreated(User user, Rental rental);
    void RentalApproved(User user, Rental rental);
    void RentalRejected(User user, Rental rental);
    void NewRentalRequest(User carOwner, Rental rental);
    void DepositPaid(User user, Rental rental);
    void OwnerDepositInfo(User owner, Rental rental);
    void RentalFinished(User user, Rental rental);

    void OwnerRentalFinished(User owner, Rental rental);
    void LatePenalty(User user, Rental rental);

    void RentalEndingSoon(User user, Rental rental);

    void WelcomeMail(User user);

    void AccountDeleted(User user);
    void EmailVerification(User user, string code);

}