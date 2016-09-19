namespace Examples
{
    using System.Net.Mail;
    using System;

    using CommonConcerns.Aop;
    using CommonConcerns.Services;

    class Program
    {
        static void Main(string[] args)
        {
            ISmtpService smtpService = new SmtpService();
            var mail = new MailMessage
            {
                Body = "TEST BODY",
                From = new MailAddress("test@yourdomain.com", "Your Name"),
                Subject = "TEST SUBJECT",
            };

            mail.To.Add(new MailAddress("testreceiver@yourdomain.com", "Test Receiver"));
            smtpService.Send(mail);

            ExceptionHandlingExample.Test();

            ILogService logService = new LogService();
            logService.Write("TEST LOG", LogType.Info);
            logService.WriteAsync("TEST ASYNC LOG", LogType.Warning);

            Console.ReadLine();
        }
    }

    [HandleExceptions]
    public class ExceptionHandlingExample
    {
        public static void Test()
        {
            throw new Exception("TEST");
        }
    }
}
