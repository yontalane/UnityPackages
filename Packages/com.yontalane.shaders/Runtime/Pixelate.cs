using UnityEngine;

namespace Yontalane.Shaders
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Shaders/Pixelate")]
    public class Pixelate : ScreenEffect
	{
        [SerializeField]
        [Min(1)]
		private int m_dimensions = 64;

        protected override void InitializeMaterial(Material material) { }

        protected override void UpdateMaterial(Material material)
        {
            material.SetInt("_Amount", m_dimensions);
        }
    }
}