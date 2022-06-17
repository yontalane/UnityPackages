using System.Diagnostics;

namespace Yontalane
{
    public static class Logger
    {
        /// <summary>
        /// Invoke UnityEngine.Debug.Log, but only if the conditional ENABLE_LOGS is active.
        /// </summary>
        [Conditional("ENABLE_LOGS")]
        public static void Log(string text) => UnityEngine.Debug.Log(text);

        /// <summary>
        /// Invoke UnityEngine.Debug.LogWarning, but only if the conditional ENABLE_LOGS is active.
        /// </summary>
        [Conditional("ENABLE_LOGS")]
        public static void LogWarning(string text) => UnityEngine.Debug.LogWarning(text);

        /// <summary>
        /// Invoke UnityEngine.Debug.LogError, but only if the conditional ENABLE_LOGS is active.
        /// </summary>
        [Conditional("ENABLE_LOGS")]
        public static void LogError(string text) => UnityEngine.Debug.LogError(text);

        /// <summary>
        /// Invoke UnityEngine.Debug.Log, but only if in the editor.
        /// </summary>
        public static void EditorLog(string text)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(text);
#endif
        }

        /// <summary>
        /// Invoke UnityEngine.Debug.LogWarning, but only if in the editor.
        /// </summary>
        public static void EditorLogWarning(string text)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(text);
#endif
        }

        /// <summary>
        /// Invoke UnityEngine.Debug.LogError, but only if in the editor.
        /// </summary>
        public static void EditorLogError(string text)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(text);
#endif
        }

    }
}
