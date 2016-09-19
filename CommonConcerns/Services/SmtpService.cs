namespace CommonConcerns.Services
{
    using System.IO;
    using System.Linq;
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Configuration;
    using System.Threading.Tasks;
    using CommonConcerns.Aop;

    [HandleExceptions]
    public class SmtpService : ISmtpService
    {
        /// <summary>
        /// Synchronously sends the email for the given EmailInfo object
        /// The hardcoded client settings are EnableSsl=true, Port=587, Timeout=10000(10 seconds), DeliveryMethod=Network
        /// </summary>
        /// <param name="email">Email to be sent</param>
        public void Send(MailMessage email)
        {
            SendEmail(email);
        }

        /// <summary>
        /// Asynchronously sends the email for the given EmailInfo object
        /// The hardcoded client settings are EnableSsl=true, Port=587, Timeout=10000(10 seconds), DeliveryMethod=Network
        /// </summary>
        /// <param name="email">Email to be sent</param>
        /// <returns>Returns a task to enable usage of this method inside Asp.net controllers</returns>
        public async Task SendAsync(MailMessage email)
        {
            await Task.Run(() => SendEmail(email));
        }

        /// <summary>
        /// Static method that asynchronously sends the email for the given EmailInfo object
        /// The hardcoded client settings are EnableSsl=true, Port=587, Timeout=10000(10 seconds), DeliveryMethod=Network
        /// </summary>
        /// <param name="email">Email to be sent</param>
        /// <returns>Returns a task to enable usage of this method inside Asp.net controllers</returns>
        public static async Task SendEmailAsync(MailMessage email)
        {
            await Task.Run(() => SendEmail(email));
        }

        /// <summary>
        /// Static method that synchronously sends the email for the given MailMessage object
        /// The hardcoded client settings are EnableSsl=true, Port=587, Timeout=10000(10 seconds), DeliveryMethod=Network
        /// </summary>
        /// <param name="email">Email to be sent</param>
        public static void SendEmail(MailMessage email)
        {
            var hostConnectionString = ConfigurationManager.ConnectionStrings["SmtpHost"];
            var userNameConnectionString = ConfigurationManager.ConnectionStrings["SmtpUsername"];
            var passwordConnectionString = ConfigurationManager.ConnectionStrings["SmtpPassword"];
            var userName = userNameConnectionString.ConnectionString;
            var password = passwordConnectionString.ConnectionString;

            if (hostConnectionString == null || userNameConnectionString == null || passwordConnectionString == null)
            {
                throw new Exception("SmtpHost, SmtpUsername, SmtpPassword settings must be set " +
                                    "in connectionStrings section of your app.config or web.config");
            }

            using (var client = new SmtpClient())
            {
                client.Host = hostConnectionString.ConnectionString;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Port = 587;
                client.Credentials = new NetworkCredential(userName, password);
                client.Timeout = 10000;
                client.Send(email);
            }
        }

        /// <summary>
        /// Places the values in the given dictionary inside the proper places in the template and returns the final string.
        /// </summary>
        /// <param name="templateUri">The uri of the template file. Like "/EmailTemplates/template1.html"</param>
        /// <param name="pairs">The dictionary of values that will be places into the template</param>
        /// <returns></returns>
        public static string ConstructHtmlStringFromTemplate(string templateUri, Dictionary<string, string> pairs)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(templateUri, ".htm"));
            var body = File.ReadAllText(filePath);

            return pairs.Aggregate(body, (current, pair) => current.Replace($"<%={pair.Key}%>", pair.Value));
        }
    }
}