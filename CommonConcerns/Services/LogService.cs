namespace CommonConcerns.Services
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CommonConcerns.Aop;

    [HandleExceptions]
    public class LogService : ILogService
    {
        /// <summary>
        /// Non static method to allow mocking this class, just calls the static WriteLog method
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="type"></param>
        public void Write(string message, LogType type)
        {
            WriteLog(message, type);
        }

        /// <summary>
        /// Non static method to allow mocking this class, just calls the static WriteLogAsync method
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="type"></param>
        public async Task WriteAsync(string message, LogType type)
        {
            await Task.Run(() => WriteLog(message, type));
        }

        /// <summary>
        /// Write the given message as the given log type to the log destination specified in web/app.config (Set as "Loggly" or "TextFile")
        /// Use the LogglySource configuration if there is a certain Source setup in your Loggly account. Sources help organize logs.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="type">Log type</param>
        public static void WriteLog(string message, LogType type)
        {
            var logDestination = GetDestination();

            switch (logDestination)
            {
                case LogDestination.TextFile:
                    {
                        LogToTextFile(message, type);
                        break;
                    }
                case LogDestination.Loggly:
                    {
                        LogToLoggly(message, type);
                        break;
                    }
            }
        }

        /// <summary>
        /// Write the log message to the destination specified with LogDestination setting in web/app.config
        /// Use the LogglySource configuration if there is a certain Source setup in your Loggly account. Sources help organize logs.
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="type">Log type</param>
        public static async Task WriteLogAsync(string message, LogType type)
        {
            await Task.Run(() => WriteLog(message, type));
        }

        private static void LogToTextFile(string logMessage, LogType type)
        {
            try
            {
                using (var logWriter = !File.Exists("log.txt") ? new StreamWriter("log.txt") : File.AppendText("log.txt"))
                {
                    logWriter.WriteLineAsync(DateTime.UtcNow + ". Type: " + GetTypeDesc(type) + Environment.NewLine +
                                                   "Log message: " + logMessage + Environment.NewLine).Wait();

                    logWriter.Flush();
                    logWriter.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("LogToTextFile received an error. Exception: " + e);
            }
        }

        private static void LogToLoggly(string message, LogType type)
        {
            try
            {
                var logglyUrl = "http://logs-01.loggly.com/inputs/" + ConfigurationManager.ConnectionStrings["LogglyToken"] + "/tag/http," + ConfigurationManager.AppSettings["LogglySource"] + "/";
                var appName = ConfigurationManager.AppSettings["LogAppName"];
                object log = new
                {
                    Message = message,
                    LogType = GetTypeDesc(type),
                    AppName = appName,
                    MachineName = Environment.MachineName
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.PostAsJsonAsync(logglyUrl, log).Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("LogToLoggly received an error. Exception: " + e);
            }
        }

        private static string GetTypeDesc(LogType type)
        {
            switch (type)
            {
                case LogType.ActionRequired:
                    {
                        return "Action Required";
                    }
                case LogType.Error:
                    {
                        return "Error";
                    }
                case LogType.Warning:
                    {
                        return "Warning";
                    }
                case LogType.Info:
                    {
                        return "Info";
                    }
            }
            return null;
        }

        private static LogDestination GetDestination()
        {
            var destinationName = ConfigurationManager.AppSettings["LogDestination"];

            if (string.IsNullOrEmpty(destinationName))
            {
                Debug.WriteLine("LogDestination is not set in app.config. Will write log to text file: log.txt");
                Console.WriteLine("LogDestination is not set in app.config. Will write log to text file: log.txt");
                return LogDestination.TextFile;
            }

            if (destinationName.Equals("textfile"))
            {
                return LogDestination.TextFile;
            }

            if (destinationName.Equals("loggly"))
            {
                return LogDestination.Loggly;
            }

            throw new Exception("Log destionatin name could not be mapped to an enum value. Check your configuration file.");
        }
    }
}
