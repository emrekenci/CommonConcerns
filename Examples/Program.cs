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
            ExceptionHandlingExample.Test("Testing async and with overwriten log destination exception handling.");

            ILogService logService = new LogService();
            logService.Write("TEST LOG", LogType.Info);
            logService.WriteAsync("TEST ASYNC LOG", LogType.Warning);

            Console.ReadLine();
        }
    }

    public class ExceptionHandlingExample
    {
        /// <summary>
        /// Exception handling with default values.
        /// Logs will be written synchronously to the LogDestination specified in the config file.
        /// </summary>
        [HandleExceptions]
        public static void Test()
        {
            throw new Exception("Testing exception handling with default parameters");
        }

        /// <summary>
        /// 
        /// </summary>
        [HandleExceptions(WriteLogsAsync = true, LogDestination = LogDestination.TextFile)]
        public static void Test(string exceptionMessage)
        {
            throw new Exception(exceptionMessage);
        }
    }
}
