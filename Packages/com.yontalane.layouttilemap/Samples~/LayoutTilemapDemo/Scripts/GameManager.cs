using UnityEngine;
using Yontalane.LayoutTilemap;

namespace Yontalane.Demos.LayoutTilemap
{
    /// <summary>
    /// Manages the initialization and setup of the layout tilemap demo, including loading the map and applying entity-specific properties.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Layout Tilemap")]
    [RequireComponent(typeof(MapBuilder))]
    public sealed class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Called on script start. Initializes the MapBuilder and loads the map, specifying a callback for when loading is complete.
        /// </summary>
        private void Start()
        {
            MapBuilder builder = GetComponent<MapBuilder>();
            builder.LoadMap(OnLoadComplete);
        }

        /// <summary>
        /// Callback invoked when the map has finished loading.
        /// Iterates through all entities in the map data and applies a tilt to those with the "Tilt" property set to true.
        /// </summary>
        /// <param name="data">The loaded map data containing entities and their properties.</param>
        private void OnLoadComplete(MapData data)
        {
            // Iterate through all entities in the loaded map data
            foreach(EntityData entity in data.entities)
            {
                // Check if the entity has a "Tilt" property set to true
                if (entity.properties.TryGet("Tilt", out MapPropertyValue tiltProperty) && tiltProperty.boolValue)
                {
                    Vector3 angles = entity.gameObject.transform.localEulerAngles;
                    angles.x += 5f;
                    entity.gameObject.transform.localEulerAngles = angles;
                }
            }
        }
    }
}