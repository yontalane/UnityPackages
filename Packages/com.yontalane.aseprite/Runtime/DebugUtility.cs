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
        public static void Log(string message)
        {
#if UNITY_EDITOR
            if (!AsepriteSettings.instance.debugSettings.log)
            {
                return;
            }

            if (!string.IsNullOrEmpty(AsepriteSettings.instance.debugSettings.filter) && !message.Contains(AsepriteSettings.instance.debugSettings.filter))
            {
                return;
            }

            Debug.Log(message);
#endif
        }

        /// <summary>
        /// Logs a warning message to the Unity Console if debug logging is enabled in <see cref="AsepriteSettings"/>.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public static void LogWarning(string message)
        {
#if UNITY_EDITOR
            if (!AsepriteSettings.instance.debugSettings.log)
            {
                return;
            }

            if (!string.IsNullOrEmpty(AsepriteSettings.instance.debugSettings.filter) && !message.Contains(AsepriteSettings.instance.debugSettings.filter))
            {
                return;
            }

            Debug.LogWarning(message);
#endif
        }

        /// <summary>
        /// Logs an error message to the Unity Console if debug logging is enabled in <see cref="AsepriteSettings"/>.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public static void LogError(string message)
        {
#if UNITY_EDITOR
            if (!AsepriteSettings.instance.debugSettings.log)
            {
                return;
            }

            if (!string.IsNullOrEmpty(AsepriteSettings.instance.debugSettings.filter) && !message.Contains(AsepriteSettings.instance.debugSettings.filter))
            {
                return;
            }

            Debug.LogError(message);
#endif
        }
    }
}
