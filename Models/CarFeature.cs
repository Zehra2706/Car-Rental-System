using System.ComponentModel.DataAnnotations;

namespace carFeature.Models 
{
    public class CarFeature
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int AracId { get; set; } // (Foreign Key)

        public double engineSize { get; set; } 

        public enum transmission { Automatic, Manual }
        public transmission Transmission { get; set; }

        public enum FuelType { Gasoline, Diesel, Electric } 
        public FuelType fuelType { get; set; }

        public string motorInsurance { get; set; }

        public bool IsChauffeured { get; set; } // Şoförlü mü?


    }
}