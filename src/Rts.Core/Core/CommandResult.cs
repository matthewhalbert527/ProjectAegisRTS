using System.Collections.Generic;

namespace ProjectAegisRTS.Core
{
    public sealed class CommandResult
    {
        public bool Success { get; private set; }
        public string ErrorCode { get; private set; }
        public string Message { get; private set; }
        public IReadOnlyList<string> Details { get; private set; }

        private CommandResult(bool success, string errorCode, string message, IReadOnlyList<string> details)
        {
            Success = success;
            ErrorCode = errorCode;
            Message = message;
            Details = details;
        }

        public static CommandResult Ok(string message)
        {
            return new CommandResult(true, string.Empty, message, new string[0]);
        }

        public static CommandResult Fail(string errorCode, string message)
        {
            return new CommandResult(false, errorCode, message, new string[0]);
        }

        public static CommandResult Fail(string errorCode, string message, IReadOnlyList<string> details)
        {
            return new CommandResult(false, errorCode, message, details);
        }
    }
}
