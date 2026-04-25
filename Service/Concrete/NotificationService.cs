using rental.Models;
using user.Models;

public class NotificationService : INotificationService
{
    private readonly EmailService _emailService;


    public NotificationService(EmailService emailService)
    {
        _emailService = emailService;
    
    }
    public void RentalCreated(User user, Rental rental)
    {   
        Console.WriteLine("MAIL TETIKLENDI");
        var body = $@"
        <h2>Kiralama Talebiniz Alındı</h2>
        <p>Sayın {user.Name},</p>
        <p>{rental.Date} - {rental.ReturnDate} tarihleri arasında araç kiralama talebiniz alınmıştır.</p>
        <p>Depozito: <b>{rental.Deposit} TL</b></p>
        <p>Durum: <b>Onay bekliyor</b></p>
        ";
        if(user?.UserInfo?.Email == null)
        {
            Console.WriteLine("EMAIL BULUNAMADI");
            return;
        }
        _emailService.SendEmail(user.UserInfo.Email, "Kiralama Talebi Alındı", body);
    }
    public void RentalApproved(User user, Rental rental)
{
    Console.WriteLine("APPROVED MAIL TETIKLENDI");
    var body = $@"
    <h2>Kiralama Talebiniz Kabul Edildi</h2>
    <p>Sayın {user.Name},</p>
    <p>{rental.Date} - {rental.ReturnDate} tarihleri arasındaki kiralama talebiniz ONAYLANMIŞTIR.</p>
    <p>İyi yolculuklar dileriz.</p>
    ";
        if(user?.UserInfo?.Email == null)
        {
            Console.WriteLine("EMAIL BULUNAMADI");
            return;
        }
    _emailService.SendEmail(user.UserInfo.Email, "Kiralama Talebi Onaylandı", body);
}
public void RentalRejected(User user, Rental rental)
{
    Console.WriteLine("REJECT MAIL TETIKLENDI");
    var body = $@"
    <h2>Kiralama Talebiniz Reddedildi</h2>
    <p>Sayın {user.Name},</p>
    <p>{rental.Date} - {rental.ReturnDate} tarihleri arasındaki kiralama talebiniz REDDEDİLMİŞTİR.</p>
    <p>Depozitonuz varsa iade süreci başlatılmıştır.</p>
    ";

    if(user?.UserInfo?.Email == null)
    {
        Console.WriteLine("EMAIL BULUNAMADI");
        return;
    }
    _emailService.SendEmail(user.UserInfo.Email, "Kiralama Talebi Reddedildi", body);
}
public void NewRentalRequest(User owner, Rental rental)
{
    var subject = "Aracınız için yeni kiralama talebi";

    var body = $@"
Merhaba {owner.Name},

Aracınız için yeni bir kiralama talebi oluşturuldu.

Başlangıç: {rental.Date}
Bitiş: {rental.ReturnDate}

Lütfen panelinizden talebi onaylayın veya reddedin.
";
     if(owner?.UserInfo?.Email == null)
    {
        Console.WriteLine("OWNER EMAIL YOK");
        return;
    }
    _emailService.SendEmail(owner.UserInfo.Email, subject, body);
}

public void DepositPaid(User user, Rental rental)
{
    var subject = "Depozito ödemeniz alındı";

    var body = $@"
Merhaba {user.Name},

{rental.Car.Brand} {rental.Car.ModelName} için depozito ödemeniz başarıyla alınmıştır.

Kiralama başlangıcı: {rental.Date}

İyi yolculuklar dileriz.
";

    _emailService.SendEmail(user.UserInfo.Email, subject, body);
}

public void OwnerDepositInfo(User owner, Rental rental)
{
    var subject = "Kiralama depozitosu ödendi";

    var body = $@"
Merhaba {owner.Name},

Aracınız için yapılan kiralamada depozito ödemesi tamamlandı.

Araç: {rental.Car.Brand} {rental.Car.ModelName}

Kiralama yakında başlayacaktır.
";

    _emailService.SendEmail(owner.UserInfo.Email, subject, body);
}

public void RentalFinished(User user, Rental rental)
{
    var subject = "Kiralama tamamlandı";

    var body = $@"
Merhaba {user.Name},

{rental.Car.Brand} {rental.Car.ModelName} kiralamanız tamamlandı.

Toplam ödeme alınmıştır.

Bizi tercih ettiğiniz için teşekkür ederiz.
";

    _emailService.SendEmail(user.UserInfo.Email, subject, body);
}

public void OwnerRentalFinished(User owner, Rental rental)
{
    var subject = "Kiralama tamamlandı";

    var body = $@"
Merhaba {owner.Name},

Aracınızın kiralama süresi tamamlandı.

Araç: {rental.Car.Brand} {rental.Car.ModelName}

Kiralama başarıyla sonlandırıldı ve ödeme işlemleri tamamlandı.

İyi günler dileriz.
";

    _emailService.SendEmail(owner.UserInfo.Email, subject, body);
}
public void LatePenalty(User user, Rental rental)
{
    var subject = "Kiralama Süresi Geçti";

    var body = $@"
    <h2>Kiralama Süreniz Geçmiştir</h2>

    <p>Merhaba {user.Name},</p>

    <p>{rental.Car.Brand} {rental.Car.ModelName} aracının kiralama süresi sona ermiştir.</p>

    <p>Planlanan iade tarihi: {rental.ReturnDate}</p>

    <p>Lütfen aracı en kısa sürede iade ediniz. Gecikme durumunda ek ücret uygulanacaktır.</p>
    ";

    if(user?.UserInfo?.Email == null)
        return;

    _emailService.SendEmail(user.UserInfo.Email, subject, body);
}

public void RentalEndingSoon(User user, Rental rental)
{
    var subject = "Kiralamanız Yakında Sona Eriyor";

    var body = $@"
    <h2>Kiralama Süresi Yakında Bitiyor</h2>

    <p>Merhaba {user.Name},</p>

    <p>{rental.Car.Brand} {rental.Car.ModelName} aracının kiralama süresi
    <b>1 saat sonra</b> sona erecektir.</p>

    <p>Planlanan iade zamanı: {rental.ReturnDate}</p>

    <p>Lütfen gecikme yaşamamak için aracı zamanında iade edin.</p>
    ";

    if (user?.UserInfo?.Email == null)
        return;

    _emailService.SendEmail(user.UserInfo.Email, subject, body);
}

public void WelcomeMail(User user)
{
    var subject = "Araç Kiralama Sistemine Hoş Geldiniz";

    var body = $@"
    <h2>Hoş Geldiniz {user.Name}</h2>

    <p>Hesabınız başarıyla oluşturuldu.</p>

    <p>Artık platformumuz üzerinden araç kiralayabilir veya aracınızı kiraya verebilirsiniz.</p>

    <p>İyi yolculuklar dileriz 🚗</p>
    ";

    if (user?.UserInfo?.Email == null)
        return;

    _emailService.SendEmail(user.UserInfo.Email, subject, body);
}

public void AccountDeleted(User user)
{
    var subject = "Hesabınız Silindi";

    var body = $@"
    <h2>Hesabınız Silinmiştir</h2>

    <p>Merhaba {user.Name},</p>

    <p>Araç Kiralama Sistemindeki hesabınız başarıyla silinmiştir.</p>

    <p>Eğer bu işlemi siz yapmadıysanız lütfen bizimle iletişime geçin.</p>

    <p>Platformumuzu kullandığınız için teşekkür ederiz.</p>
    ";

    if (user?.UserInfo?.Email == null)
        return;

    _emailService.SendEmail(user.UserInfo.Email, subject, body);
}

}