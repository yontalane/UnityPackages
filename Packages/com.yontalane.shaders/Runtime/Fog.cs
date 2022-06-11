using UnityEngine;

namespace Yontalane.Shaders
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Shaders/Fog")]
    public class Fog : ScreenEffect
    {
        [SerializeField]
        [Range(0f, 10f)]
        private float m_start = 0.75f;

        [SerializeField]
        [Range(0f, 10f)]
        private float m_end = 0.25f;

        [SerializeField]
        private Color m_color = default;

        protected override void InitializeMaterial(Material material) { }

        protected override void UpdateMaterial(Material material)
        {
            material.SetFloat("_Start", m_start * 0.01f);
            material.SetFloat("_End", m_end * 0.01f);
            material.SetColor("_Color", m_color);
        }
    }
}