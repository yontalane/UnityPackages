using UnityEngine;
using Yontalane.LayoutTilemap;

namespace Yontalane.Demos.LayoutTilemap
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Demos/Layout Tilemap")]
    [RequireComponent(typeof(MapBuilder))]
    public sealed class GameManager : MonoBehaviour
    {
        private void Start()
        {
            MapBuilder builder = GetComponent<MapBuilder>();
            builder.LoadMap(OnLoadComplete);
        }

        private void OnLoadComplete(MapData data)
        {
            foreach(EntityData entity in data.entities)
            {
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