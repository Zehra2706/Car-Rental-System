using System.ComponentModel.DataAnnotations;

public class AdminCreateUserViewModel
{
    public string Name { get; set; }
    public string Surname { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor!")]
    public string ConfirmPassword { get; set; }

    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string LicenseNumber { get; set; }
}