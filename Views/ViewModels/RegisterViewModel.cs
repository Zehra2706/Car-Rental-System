using System.ComponentModel.DataAnnotations;
public class RegisterViewModel
{
    public required string Name { get; set; }

    [Required(ErrorMessage = "Email zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf, 1 sayı ve 1 özel karakter içermelidir")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor")]
    public string ConfirmPassword { get; set; }
    public required string Surname { get; set; }

    public required string Address { get; set; }
    
    [Required(ErrorMessage = "Telefon zorunlu")]
    [RegularExpression(@"^0\d{10}$", ErrorMessage = "Telefon 0 ile başlamalı ve 11 haneli olmalı")]
    public required string PhoneNumber { get; set; }

    [Required(ErrorMessage = "TC zorunlu")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC 11 haneli olmalı")]
    public string TC { get; set; }

    [Required(ErrorMessage = "Ehliyet numarası zorunlu")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Ehliyet numarası 6 haneli olmalı")]
    public required string LicenseNumber { get; set; }
}