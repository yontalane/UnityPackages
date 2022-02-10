using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Entity")]
    [RequireComponent(typeof(MapProperties))]
    public class MapEntity : MonoBehaviour
    {
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
            Gizmos.color = Color.white;
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "Packages/com.yontalane.layouttilemap/Runtime/Gizmos/MapMarkerGizmo.png", true);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
        }
    }
}