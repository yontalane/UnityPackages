using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.LayoutTilemap
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Map Entity")]
    [RequireComponent(typeof(MapProperties))]
    public class MapEntity : MonoBehaviour
    {
        public string entityTag = string.Empty;

        private MapProperties m_mapProperties = null;
        public MapProperties MapProperties
        {
            get
            {
                if (m_mapProperties == null)
                {
                    m_mapProperties = GetComponent<MapProperties>();
                }
                return m_mapProperties;
            }
        }

        private void OnDrawGizmos()
        {
            float scale, pointerLength, pointerScale;
            Color outerColor, innerColor;

            MapEntityConfig config = MapEntitySettings.GetOrCreateSettings().Config;

            if (config != null)
            {
                MapEntityConfig.MapEntityData data = !string.IsNullOrWhiteSpace(entityTag) ? config.GetData(entityTag) : config.defaultData;
                scale = data.scale;
                outerColor = data.outerColor;
                innerColor = data.innerColor;
                pointerLength = data.pointerLength;
                pointerScale = data.pointerScale;
            }
            else
            {
                scale = 0.75f;
                outerColor = Color.gray;
                innerColor = Color.white;
                pointerLength = 1.5f;
                pointerScale = 0.5f;
            }

            Gizmos.color = innerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.26f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.28f);
            Gizmos.color = outerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.3f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.32f);

            Gizmos.color = innerColor;
            for (int i = 0; i < 5; i++)
            {
                Gizmos.DrawWireSphere(transform.position, scale * (0.8f - i * 0.02f));
            }
            Gizmos.color = outerColor;
            for (int i = 0; i < 5; i++)
            {
                Gizmos.DrawWireSphere(transform.position, scale * (0.82f + i * 0.02f));
            }

            Gizmos.color = innerColor;
            Gizmos.DrawLine(transform.position, transform.position + pointerLength * scale * transform.forward);

            for (int i = 0; i < 3; i++)
            {
                Gizmos.DrawWireSphere(transform.position + transform.forward * pointerLength * scale, pointerScale * scale * (0.8f - i * 0.02f));
            }
            Gizmos.color = outerColor;
            for (int i = 0; i < 3; i++)
            {
                Gizmos.DrawWireSphere(transform.position + transform.forward * pointerLength * scale, pointerScale * scale * (0.82f + i * 0.02f));
            }

            if (TryGetComponent(out Collider collider))
            {
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }
        }
    }
}