using UnityEngine;

namespace Yontalane.Shaders
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Shaders/Outline")]
    public class Outline : ScreenEffect
    {
        [Space]
        [SerializeField]
        private Color m_color = Color.black;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_cutoff = 0f;

        [Space]
        [SerializeField]
        [Range(0f, 4f)]
        private float m_normalOutlineMultiplier = 1f;

        [SerializeField]
        [Range(1f, 4f)]
        private float m_normalOutlineBias = 1f;

        [Space]
        [SerializeField]
        [Range(0f, 4f)]
        private float m_depthOutlineMultiplier = 1f;

        [SerializeField]
        [Range(1f, 4f)]
        private float m_depthOutlineBias = 1f;

        protected override void InitializeMaterial(Material material) { }

        protected override void UpdateMaterial(Material material)
        {
            material.SetMatrix("_viewToWorld", Camera.cameraToWorldMatrix);

            material.SetColor("_Color", m_color);
            material.SetFloat("_Cutoff", m_cutoff);

            material.SetFloat("_NormalMultiplier", m_normalOutlineMultiplier);
            material.SetFloat("_NormalBias", m_normalOutlineBias);

            material.SetFloat("_DepthMultiplier", m_depthOutlineMultiplier);
            material.SetFloat("_DepthBias", m_depthOutlineBias);
        }
    }
}