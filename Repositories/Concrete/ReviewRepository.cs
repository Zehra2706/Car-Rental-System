using car.Data;
using car.Models;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;
    public ReviewRepository(ApplicationDbContext context) => _context = context;
    public Review GetReviewById(int id)
    {
        return _context.Reviews.FirstOrDefault(r => r.Id == id);
    }
    public List<Review> GetReviewsByUserId(int userId)
    {
        return _context.Reviews
            .Include(r => r.Car)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedDate)
            .ToList();
    }
    List<Review> IReviewRepository.GetAllReviews()
    {
        return _context.Reviews
            .Include(r => r.Car)
            .OrderByDescending(r => r.CreatedDate)
            .ToList();
    }
    public void DeleteReview(int id)
    {
        var review = _context.Reviews.Find(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            _context.SaveChanges();
        }
    }

    public void UpdateReview(Review review)
    {
        _context.Reviews.Update(review);
        _context.SaveChanges();
    }
    public void AddReview(Review review)
    {
        _context.Reviews.Add(review);
        _context.SaveChanges();
    }

    public List<Review> GetReviewsByCarId(int carId)
    {
        return _context.Reviews.Where(r => r.CarId == carId).OrderByDescending(x => x.CreatedDate).ToList();
    }
}