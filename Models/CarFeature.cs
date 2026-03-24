using System.ComponentModel.DataAnnotations;
using car.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace carFeature.Models
{
    public class CarFeature
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int CarId { get; set; } // Bu sadece sayı (Veritabanındaki sütun)

        [ForeignKey("CarId")] // 2. "Yukarıdaki CarId, Car tablosuna gider" diyoruz.
        public Car? Car { get; set; } // Bu da o arabanın kendisi (Java'daki nesne referansı)

        public double engineSize { get; set; }

        public enum TransmissionType { Automatic, Manual }
        public TransmissionType Transmission { get; set; }

        public enum FuelType { Gasoline, Diesel, Electric }
        public required FuelType fuelType { get; set; }

        public required string motorInsurance { get; set; }

        public required bool IsChauffeured { get; set; } // Şoförlü mü?


    }
}