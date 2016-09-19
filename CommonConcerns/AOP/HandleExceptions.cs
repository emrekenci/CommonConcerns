namespace CommonConcerns.Aop
{
    using System;
    using System.Diagnostics;
    using System.Configuration;

    using PostSharp.Aspects;

    using CommonConcerns.Services;

    [Serializable]
    public class HandleExceptions : OnExceptionAspect
    {
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

                var className = args.Method.DeclaringType?.Name ?? "Clas name not found";

                var logMessage = "Exception in " + args.Method.Name + " method of class: " + className +
                                 ". Exception: " + args.Exception + " Stack trace: " + args.Exception.StackTrace +
                                 ". Inner Exception: " + args.Exception.InnerException +
                                 ". Method was called with arguments: " + arguments;

                LogService.WriteLog(logMessage, LogType.Error);

                args.FlowBehavior = FlowBehavior.Continue;
            }
            catch (Exception e)
            {
                Console.WriteLine("CRITICAL ERROR: Exception on OnException." + e);
                Debug.WriteLine("CRITICAL ERROR: Exception on OnException." + e);
                LogService.WriteLog("Received exception in HandleExceptions. Actual error message could not be logged. Exception: " + e, LogType.ActionRequired);
                args.FlowBehavior = FlowBehavior.Continue;
            }
        }
    }
}