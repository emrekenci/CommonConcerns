namespace CommonConcerns.Services
{
    using System.Net.Mail;
    using System.Threading.Tasks;

    public interface ISmtpService
    {
        void Send(MailMessage email);
        Task SendAsync(MailMessage email);
    }
}
