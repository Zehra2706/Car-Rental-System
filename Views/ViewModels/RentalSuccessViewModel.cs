namespace car.ViewModels
{
    public class RentalSuccessViewModel
    {
        public int CarId { get; set; }
        public bool ShowReview { get; set; } = true;
        public string Brand { get; set; }
        public string ModelName { get; set; }
        public decimal TotalPrice { get; set; }
        // İhtiyacın olursa buraya rezervasyon numarası vb. de ekleyebilirsin
    }
}