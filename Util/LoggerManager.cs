using NLog;
using System;
using System.Runtime.CompilerServices;

namespace SifizPlanning.Util
{
    public static class LoggerManager
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Logger.Info($"{message} | Method: {memberName} | File: {sourceFilePath} | Line: {sourceLineNumber}");
        }

        public static void LogError(Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Logger.Error(ex, $"Error in {memberName} | File: {sourceFilePath} | Line: {sourceLineNumber}");
        }

        public static void LogSensitive(string operation, string details,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Logger.Info($"SENSITIVE - Operation: {operation} | Details: {details} | Method: {memberName} | File: {sourceFilePath} | Line: {sourceLineNumber}");
        }

        public static void LogWarning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Logger.Warn($"{message} | Method: {memberName} | File: {sourceFilePath} | Line: {sourceLineNumber}");
        }

        public static void LogDebug(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Logger.Debug($"{message} | Method: {memberName} | File: {sourceFilePath} | Line: {sourceLineNumber}");
        }

        public static void LogSensitiveOperation(string operation, string details, string user)
        {
            var logMessage = $"SENSITIVE OPERATION - {operation} | User: {user} | Details: {details}";
            Logger.Info(logMessage);
        }

        public static void LogUserManagement(string action, string targetUser, string performedBy)
        {
            var logMessage = $"USER MANAGEMENT - Action: {action} | Target: {targetUser} | Performed by: {performedBy}";
            Logger.Info(logMessage);
        }

        public static void LogPermissionChange(string user, string permissions, string performedBy)
        {
            var logMessage = $"PERMISSION CHANGE - User: {user} | New Permissions: {permissions} | Changed by: {performedBy}";
            Logger.Info(logMessage);
        }

        public static void LogConfigurationChange(string setting, string oldValue, string newValue, string user)
        {
            var logMessage = $"CONFIG CHANGE - Setting: {setting} | Old: {oldValue} | New: {newValue} | Changed by: {user}";
            Logger.Info(logMessage);
        }

        public static void LogHRAction(string action, string employee, string details, string performedBy)
        {
            var logMessage = $"HR ACTION - Type: {action} | Employee: {employee} | Details: {details} | Performed by: {performedBy}";
            Logger.Info(logMessage);
        }
    }
}
