using UnityEngine;

namespace Spotify4Unity
{
    /// <summary>
    /// Class to handle debug information and logging for the Spotify4Unity plugin
    /// Can enabled all logs from the plugin by adding "S4U_LOGS" into the Scripting Define Symbols inside Unity Player Settings
    /// </summary>
    public class Analysis
    {
        /// <summary>
        /// Types of logs for Spotify4Unity
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// No messages at all
            /// </summary>
            None = 0,
            /// <summary>
            /// Important log, should be shown unless not wanted
            /// </summary>
            Vital = 1,
            /// <summary>
            /// All messages about state changes, info, etc
            /// </summary>
            All = 2,
        }

        /// <summary>
        /// Level at which logs should be shown and outputted to the user
        /// </summary>
        public static LogLevel LogsLevel { get; set; } = LogLevel.Vital;

        /// <summary>
        /// Name to prefix before debug messages
        /// </summary>
        static string PLUGIN_NAME = "Spotify4Unity";

        /// <summary>
        /// Logs a normal message out to the Unity console
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        public static void Log(string message, LogLevel logType)
        {
            if(LogsLevel >= logType)
                Debug.Log(GetFormat(message));
        }

        /// <summary>
        /// Logs a warning message out to the Unity console
        /// </summary>
        /// <param name="message"></param>
        /// /// <param name="logType"></param>
        public static void LogWarning(string message, LogLevel logType)
        {
            if (LogsLevel >= logType)
                Debug.LogWarning(GetFormat(message));
        }

        /// <summary>
        /// Logs a warning message out to the Unity console
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        public static void LogError(string message, LogLevel logType)
        {
            if (LogsLevel >= logType)
                Debug.LogError(GetFormat(message));
        }

        private static string GetFormat(string message)
        {
            return $"{PLUGIN_NAME} - {message}";
        }
    }
}