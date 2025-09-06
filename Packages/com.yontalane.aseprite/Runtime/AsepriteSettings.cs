#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// Contains color and size settings for drawing gizmos related to Aseprite objects in the Unity Editor.
    /// </summary>
    [System.Serializable]
    public struct GizmoInfo
    {
        [Tooltip("Color used to draw collider gizmos.")]
        public Color colliderColor;

        [Tooltip("Color used to draw trigger gizmos.")]
        public Color triggerColor;

        [Tooltip("Color used to draw point gizmos.")]
        public Color pointColor;

        [Tooltip("Radius of the point gizmos.")]
        public float pointRadius;
    }

    /// <summary>
    /// Enable or disable debug logging for the Yontalane Aseprite integration.
    /// </summary>
    [System.Serializable]
    public struct DebugSettings
    {
        [Tooltip("Enable or disable debug logging.")]
        public bool log;

        [Tooltip("Only log debug messages that contain this filter.")]
        public string filter;
    }

    /// <summary>
    /// Stores project-wide settings for the Yontalane Aseprite integration, including debug logging and gizmo appearance.
    /// </summary>
    [FilePath("ProjectSettings/YontalaneAsepriteSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AsepriteSettings : ScriptableSingleton<AsepriteSettings>
    {
        [Tooltip("Enable or disable debug logging for the Yontalane Aseprite integration.")]
        public DebugSettings debugSettings;

        [Tooltip("Settings for gizmo colors and sizes used in the Unity Editor for Aseprite objects.")]
        public GizmoInfo gizmoInfo;

        public AsepriteSettings()
        {
            debugSettings = new()
            {
                log = false,
                filter = string.Empty,
            };

            gizmoInfo = new()
            {
                colliderColor = Color.green,
                triggerColor = Color.cyan,
                pointColor = Color.cyan,
                pointRadius = 0.05f,
            };
        }

        public void Save() => Save(true);
    }
}
#endif