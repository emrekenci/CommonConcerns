namespace CommonConcerns.Aop
{
    using System;
    using System.Configuration;

    using PostSharp.Aspects;

    using CommonConcerns.Services;

    [Serializable]
    public class HandleExceptions : OnExceptionAspect
    {
        /// <summary>
        /// Allows to overwrite the LogDestination in the config file
        /// </summary>
        public LogDestination LogDestination { get; set; }

        /// <summary>
        /// If it is set to true, the log messages will be written asynchronously.
        /// Be careful using this. If the application context dies before the LogService completes writing the log, 
        /// the log message may never reach the log destination.
        /// </summary>
        public bool WriteLogsAsync { get; set; }

        /// <summary>
        /// Invoked when there is an exception in methods that are decorated with this aspect.
        /// Parses the MethodExecutionArgs and writes an error log. 
        /// The LogService used for logging the exception must be configured from the .config file.
        /// The behaviour is set as FlowBehavior.Continue by default
        /// Can be disabled by adding 'DisableHandleExceptions = "true"' in the config file
        /// When disabled, this will throw the exception
        /// </summary>
        /// <param name="args">Object containing the details of the exeption and the location where it was thrown</param>
        public override void OnException(MethodExecutionArgs args)
        {
            var disableSetting = ConfigurationManager.AppSettings["DisableHandleExceptions"];
            var disabled = !string.IsNullOrEmpty(disableSetting) && disableSetting == "true";
            if (disabled)
            {
                args.FlowBehavior = FlowBehavior.ThrowException;
                return;
            }

            try
            {
                var arguments = "";
                for (var i = 0; i < args.Arguments.Count; i++)
                {
                    arguments += args.Arguments[i];
                    if (i != args.Arguments.Count - 1)
                    {
                        arguments += ", ";
                    }
                }

                var className = args.Method.DeclaringType?.Name ?? "Class name not found";

                var logMessage = "Exception in " + args.Method.Name + " method of class: " + className +
                                 ". Exception: " + args.Exception + " Stack trace: " + args.Exception.StackTrace +
                                 ". Inner Exception: " + args.Exception.InnerException +
                                 ". Method was called with arguments: " + arguments;
                
                if(WriteLogsAsync)
                {
                    LogService.WriteLogAsync(logMessage, LogType.Error, this.LogDestination);
                }
                else
                {
                    LogService.WriteLog(logMessage, LogType.Error, this.LogDestination);
                }

                args.FlowBehavior = FlowBehavior.Continue;
            }
            catch (Exception e)
            {
                throw new Exception("Received an error in HandleExceptions attribute. This is likely due to a configuration error." + e.Message);
            }
        }
    }
}