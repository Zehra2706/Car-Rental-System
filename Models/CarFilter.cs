using static carFeature.Models.CarFeature;

public class CarFilter
{
    public string Brand { get; set; }
    public string ModelName { get; set; }
    public string Location { get; set; }

    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }

    public TransmissionType? Transmission { get; set; } // 0 = Manuel, 1 = Otomatik
    public FuelType? FuelType { get; set; }    // 0 = Benzin, 1 = Dizel 

    public bool? IsChauffeured { get; set; }
    public double? MinPrice { get; set; }
    public double? MaxPrice { get; set; }
    public string? PriceType { get; set; }

}