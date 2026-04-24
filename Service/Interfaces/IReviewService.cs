using car.Models;

public interface IReviewService
{
    void PostReview(int carId, int userId, int rating, string comment);
    List<Review> GetCarReviews(int carId);
    void UpdateReview(int reviewId, int userId, int rating, string comment);
    Review GetReview(int id);
    List<Review> GetUserReviews(int userId);
    List<Review> GetAllReviews();
    void DeleteReview(int reviewId, int userId);
}