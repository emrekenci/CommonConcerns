namespace CommonConcerns.Services
{
    using System.Threading.Tasks;

    public interface ILogService
    {
        void Write(string message, LogType type, LogDestination destination = LogDestination.None);
        Task WriteAsync(string message, LogType type, LogDestination destination = LogDestination.None);
    }

    /// <summary>
    /// The Log type enum
    /// </summary>
    public enum LogType
    {
        ActionRequired,
        Error,
        Warning,
        Info,
    }

    /// <summary>
    /// The log destination enum
    /// </summary>
    public enum LogDestination
    {
        None = 0,
        TextFile,
        Loggly
    }
}
