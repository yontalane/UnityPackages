#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// Contains color and size settings for drawing gizmos related to Aseprite objects in the Unity Editor.
    /// </summary>
    [System.Serializable]
    public struct AsepriteGizmoInfo
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
    /// Stores project-wide settings for the Yontalane Aseprite integration, including debug logging and gizmo appearance.
    /// </summary>
    [FilePath("ProjectSettings/YontalaneAsepriteSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AsepriteSettings : ScriptableSingleton<AsepriteSettings>
    {
        [Tooltip("Enable or disable debug logging for the Yontalane Aseprite integration.")]
        public bool debugLog;

        [Tooltip("Settings for gizmo colors and sizes used in the Unity Editor for Aseprite objects.")]
        public AsepriteGizmoInfo gizmoInfo;

        public AsepriteSettings()
        {
            debugLog = false;

            gizmoInfo = new AsepriteGizmoInfo()
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