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

        [SerializeField]
        [Tooltip("What 3D space data do we want to include in the screen capture?")]
        private DepthTextureMode m_depthTextureMode = DepthTextureMode.None;

        private Camera m_camera = null;
        public Camera Camera
        {
            get
            {
                if (m_camera == null)
                {
                    m_camera = GetComponent<Camera>();
                }
                return m_camera;
            }
        }

        private void Start()
        {
            if (m_depthTextureMode != DepthTextureMode.None)
            {
                Camera.depthTextureMode |= m_depthTextureMode;
            }

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
            if (Material == null)
            {
                Debug.LogWarning($"{GetType().Name} is missing a shader or a material.");
                return;
            }

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