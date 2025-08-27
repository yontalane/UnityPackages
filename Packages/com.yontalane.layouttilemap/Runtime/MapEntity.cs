using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    /// <summary>
    /// Contains visual and pointer configuration data for a map entity, such as scale, colors, pointer length/scale, and outline thickness.
    /// </summary>
    [System.Serializable]
    public struct MapEntityData
    {
        [Tooltip("The overall scale of the map entity's visual representation.")]
        [Min(0.01f)]
        public float scale;

        [Tooltip("The color used for the outer part of the entity's visual representation.")]
        public Color outerColor;

        [Tooltip("The color used for the inner part of the entity's visual representation.")]
        public Color innerColor;

        [Tooltip("The length of the pointer (direction indicator) for the map entity.")]
        [Min(0.01f)]
        public float pointerLength;

        [Tooltip("The scale of the pointer (direction indicator) for the map entity.")]
        [Min(0.01f)]
        public float pointerScale;

        [Tooltip("The thickness of the outline for the map entity's visual representation.")]
        [Range(1, 5)]
        public int thickness;
    }

    /// <summary>
    /// Associates a name with a specific set of MapEntityData, allowing for special-case entity appearance or behavior.
    /// </summary>
    [System.Serializable]
    public struct NamedMapEntityData
    {
        [Tooltip("The name associated with this set of MapEntityData.")]
        public string name;

        [Tooltip("The visual and pointer configuration data for this named map entity.")]
        public MapEntityData data;
    }

    /// <summary>
    /// MapEntity is a MonoBehaviour component that represents an entity within the LayoutTilemap system.
    /// It provides access to the entity's map properties and, in the Unity Editor, draws visual gizmos
    /// to represent the entity's appearance and orientation based on configurable MapEntityData.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Map Entity")]
    [RequireComponent(typeof(MapProperties))]
    public class MapEntity : MonoBehaviour
    {
        private MapProperties m_mapProperties = null;

        /// <summary>
        /// Gets the MapProperties component associated with this MapEntity.
        /// </summary>
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            MapEntityData data = LayoutTilemapSettings.instance.GetData(name);
            float scale = data.scale;
            Color outerColor = data.outerColor;
            Color innerColor = data.innerColor;
            float pointerLength = data.pointerLength;
            float pointerScale = data.pointerScale;
            int thickness = data.thickness;

            Gizmos.color = innerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.26f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.28f);
            Gizmos.color = outerColor;
            Gizmos.DrawWireSphere(transform.position, scale * 0.3f);
            Gizmos.DrawWireSphere(transform.position, scale * 0.32f);

            Gizmos.color = innerColor;
            for (int i = 0; i < 4 * thickness; i++)
            {
                Gizmos.DrawWireSphere(transform.position, scale * (0.8f - i * 0.02f));
            }
            Gizmos.color = outerColor;
            for (int i = 0; i < 4 * thickness; i++)
            {
                Gizmos.DrawWireSphere(transform.position, scale * (0.82f + i * 0.02f));
            }

            Gizmos.color = innerColor;
            Gizmos.DrawLine(transform.position, transform.position + pointerLength * scale * transform.forward);

            for (int i = 0; i < 2 * thickness; i++)
            {
                Gizmos.DrawWireSphere(transform.position + pointerLength * scale * transform.forward, pointerScale * scale * (0.8f - i * 0.02f));
            }
            Gizmos.color = outerColor;
            for (int i = 0; i < 2 * thickness; i++)
            {
                Gizmos.DrawWireSphere(transform.position + pointerLength * scale * transform.forward, pointerScale * scale * (0.82f + i * 0.02f));
            }

            if (TryGetComponent(out Collider collider))
            {
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }
        }
#endif
    }
}