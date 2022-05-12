using UnityEngine;

namespace Yontalane.Shaders
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Shaders/Fade")]
    public class Fade : ScreenEffect
	{
        [SerializeField]
        [Range(0f, 1f)]
        private float m_darken = 0.5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_saturation = 0.5f;

        protected override void InitializeMaterial(Material material) { }

        protected override void UpdateMaterial(Material material)
        {
            material.SetFloat("_Dark", m_darken);
            material.SetFloat("_Sat", m_saturation);
        }
    }
}