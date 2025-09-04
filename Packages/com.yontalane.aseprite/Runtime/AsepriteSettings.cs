#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Yontalane.Aseprite
{
    [System.Serializable]
    public struct AsepriteGizmoInfo
    {
        public Color colliderColor;
        public Color triggerColor;
        public Color pointColor;
        public float pointRadius;
    }

    [FilePath("ProjectSettings/YontalaneAsepriteSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AsepriteSettings : ScriptableSingleton<AsepriteSettings>
    {
        public AsepriteGizmoInfo gizmoInfo;

        public AsepriteSettings()
        {
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