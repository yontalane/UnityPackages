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
            float scale;
            Color outerColor, innerColor;
            MapEntityManager mapEntityManager = GetComponentInParent<MapEntityManager>();
            if (mapEntityManager != null)
            {
                MapEntityManager.MapEntityData data = !string.IsNullOrWhiteSpace(entityTag) ? mapEntityManager.GetData(entityTag) : mapEntityManager.defaultData;
                scale = data.scale;
                outerColor = data.outerColor;
                innerColor = data.innerColor;
            }
            else
            {
                scale = 1f;
                outerColor = Color.gray;
                innerColor = Color.white;
            }

            Gizmos.color = innerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.26f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.28f);
            Gizmos.color = outerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.3f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.32f);

            Gizmos.color = innerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.78f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.8f);
            Gizmos.color = outerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.82f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.84f);

            Gizmos.color = innerColor;
            Gizmos.DrawLine(transform.position, transform.position + scale * transform.forward);

            Gizmos.DrawWireSphere(transform.position + transform.forward * scale, scale * 0.15f);
            Gizmos.DrawWireSphere(transform.position + transform.forward * scale, scale * 0.17f);
            Gizmos.color = outerColor;
            Gizmos.DrawWireSphere(transform.position + transform.forward * scale, scale * 0.19f);
            Gizmos.DrawWireSphere(transform.position + transform.forward * scale, scale * 0.21f);

            if (TryGetComponent(out Collider collider))
            {
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }
        }
    }
}