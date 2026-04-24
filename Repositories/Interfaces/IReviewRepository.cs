using car.Models;

public interface IReviewRepository
{
    void AddReview(Review review);
    List<Review> GetReviewsByCarId(int carId);
    Review GetReviewById(int id);
    void UpdateReview(Review review);
    List<Review> GetReviewsByUserId(int userId);
    void DeleteReview(int id);
    List<Review> GetAllReviews();
}