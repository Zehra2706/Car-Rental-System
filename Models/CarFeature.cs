using System.ComponentModel.DataAnnotations;
using car.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace carFeature.Models
{
    public class CarFeature
    {
        [Key]
        public int Id { get; set; }
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        public double enginePower { get; set; }

        public enum TransmissionType { Automatic, Manual }
        public TransmissionType Transmission { get; set; }

        public enum FuelType { Gasoline, Diesel, Electric }
        public required FuelType fuelType { get; set; }

        public required string motorInsurance { get; set; }

        public required bool IsChauffeured { get; set; }


    }
}