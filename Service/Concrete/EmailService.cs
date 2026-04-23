using System.Net;
using System.Net.Mail;
using car.Data;

public class EmailService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly SmtpClient _smtpClient;

    public EmailService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        _smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(
                "rentacar.systemm@gmail.com",
                "cbri ilmw cbfd xusp"
            ),
            EnableSsl = true,
            Timeout = 10000
        };
    }

    public void SendEmail(string to, string subject, string body)
    {   Console.WriteLine("EMAIL GONDERME BASLADI");
        Console.WriteLine("TO: " + to);
        var log = new MailLog
        {
            ToEmail = to,
            Subject = subject,
            Body = body,
            SentAt = DateTime.Now
        };

        try
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("rentacar.systemm@gmail.com");
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            _smtpClient.Send(mail);

            log.IsSuccess = true;
        }
        catch (Exception ex)
        {
            log.IsSuccess = false;
            log.ErrorMessage = ex.Message;

            Console.WriteLine("EMAIL HATA:");
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            _dbContext.MailLogs.Add(log);
            _dbContext.SaveChanges();
        }
    }
}