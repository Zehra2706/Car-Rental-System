using System.ComponentModel.DataAnnotations.Schema;
using car.Models;
using user.Models;
using System.ComponentModel.DataAnnotations;

namespace rental.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }

        public required DateTime Date { get; set; } // Başlangıç
        public required DateTime ReturnDate { get; set; } // Planlanan Bitiş
        public DateTime? RealReturnDate { get; set; }

        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // 🚩 double ve int yerine decimal yaparak İyzico'daki kuruş farkını bitiriyoruz
        public decimal Forecast { get; set; } // Toplam Tutar
        public decimal Deposit { get; set; }

        public bool IsReturned { get; set; } = false;
        public string Status { get; set; } = "OnayBekliyor";

        public bool IsContractApproved { get; set; }
        public bool ReminderSent { get; set; } = false;
        public bool IsPaid { get; set; } = false;

        public bool IsCompleted { get; set; }

        // 🚩 EKLEDİĞİMİZ AKILLI DURUM: Ekranda süslü yazılar için
        [NotMapped]
        public string DisplayStatus
        {
            get
            {
                // Eğer yönetici onayladıysa ama kiralama saati henüz gelmediyse
                if (Status == "Onaylandı" && DateTime.Now < Date)
                    return "Rezerve Edildi";

                // Eğer onaylıysa ve şu an kiralama süresi içindeysek
                if (Status == "Onaylandı" && DateTime.Now >= Date && !IsReturned)
                    return "Kiralama Aktif";

                // Diğer durumlar (Onay Bekliyor, İptal vb.) aynen döner
                return Status;
            }
        }



    }
}