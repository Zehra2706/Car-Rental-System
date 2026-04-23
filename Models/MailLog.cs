public class MailLog
{
    public int Id { get; set; }

    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }

    public DateTime SentAt { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}