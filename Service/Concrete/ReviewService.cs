using car.Models;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    public ReviewService(IReviewRepository reviewRepository) => _reviewRepository = reviewRepository;
    public void UpdateReview(int reviewId, int userId, int rating, string comment)
    {
        var review = _reviewRepository.GetReviewById(reviewId);


        if (review != null && review.UserId == userId)
        {
            review.Rating = rating;
            review.Comment = comment;
            review.CreatedDate = DateTime.Now;
            _reviewRepository.UpdateReview(review);
        }
    }
    public void DeleteReview(int reviewId, int userId)
    {
        var review = _reviewRepository.GetReviewById(reviewId);

        if (review != null && review.UserId == userId)
        {
            _reviewRepository.DeleteReview(reviewId);
        }
    }
    public List<Review> GetAllReviews()
    {
        return _reviewRepository.GetAllReviews();
    }
    public List<Review> GetUserReviews(int userId)
    {
        return _reviewRepository.GetReviewsByUserId(userId);
    }
    public Review GetReview(int id) => _reviewRepository.GetReviewById(id);
    public void PostReview(int carId, int userId, int rating, string comment)
    {
        var review = new Review
        {
            CarId = carId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedDate = DateTime.Now
        };
        _reviewRepository.AddReview(review);
    }

    public List<Review> GetCarReviews(int carId) => _reviewRepository.GetReviewsByCarId(carId);
}