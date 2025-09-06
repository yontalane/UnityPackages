using UnityEngine;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// Provides utility methods for logging debug messages, warnings, and errors for the Yontalane Aseprite integration.
    /// Logging is controlled by the <see cref="AsepriteSettings.debugLog"/> setting.
    /// </summary>
    public static class DebugUtility
    {
        /// <summary>
        /// Logs a message to the Unity Console if debug logging is enabled in <see cref="AsepriteSettings"/>.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Log(object message)
        {
            if (!AsepriteSettings.instance.debugLog)
            {
                return;
            }

            Debug.Log(message);
        }

        /// <summary>
        /// Logs a warning message to the Unity Console if debug logging is enabled in <see cref="AsepriteSettings"/>.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public static void LogWarning(object message)
        {
            if (!AsepriteSettings.instance.debugLog)
            {
                return;
            }

            Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message to the Unity Console if debug logging is enabled in <see cref="AsepriteSettings"/>.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public static void LogError(object message)
        {
            if (!AsepriteSettings.instance.debugLog)
            {
                return;
            }

            Debug.LogError(message);
        }
    }
}
