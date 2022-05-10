using UnityEngine;

namespace Yontalane.Shaders
{
    [System.Serializable]
    public struct ScreenEffectConfig
    {
        public bool userSetShader;
        public Shader shader;
        public Material material;
    }

    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    [AddComponentMenu("Yontalane/Shaders/Screen Effect")]
    public class ScreenEffect : MonoBehaviour
    {
        private bool m_wasSetThisSession = false;

        [SerializeField]
        private ScreenEffectConfig m_config = new ScreenEffectConfig();

        private void Start()
        {
            if (m_config.material != null)
            {
                m_config.shader = null;
            }
            else if (m_config.shader == null || !m_config.shader.isSupported)
            {
                enabled = false;
            }
        }

        private Material Material
        {
            get
            {
                if (m_config.material == null && m_config.shader != null)
                {
                    m_config.material = new Material(m_config.shader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }
                return m_config.material;
            }
        }

        private void OnDisable()
        {
            if (m_config.userSetShader && m_config.material != null)
            {
                DestroyImmediate(m_config.material);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Material == null) return;

            if (!m_wasSetThisSession)
            {
                InitializeMaterial(Material);
                m_wasSetThisSession = true;
            }

            UpdateMaterial(Material);
            Graphics.Blit(source, destination, Material);
        }

        protected virtual void InitializeMaterial(Material material) { }

        protected virtual void UpdateMaterial(Material material) { }
    }
}