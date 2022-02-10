using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Yontalane.LayoutTilemap
{
    #region Structs
    public struct EntityData
    {
        public GameObject gameObject;
        public MapEntity entity;
        public MapProperties properties;
    }

    public struct MapData
    {
        public List<EntityData> entities;
        public MapProperties mapProperties;
    }
    #endregion

    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Builder")]
    public sealed class MapBuilder : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        [Tooltip("The fallback map to load. If the LoadMap method receives a mapToLoad parameter, then that parameter takes precedence over this.")]
        private string m_mapToLoad = "";
        
        [SerializeField]
        [Tooltip("The map will be built as child of this Transform. If left null, the map will be built the MapBuilder Transform.")]
        private Transform m_mapParent = null;
        
        [SerializeField]
        [Tooltip("Should the MapBuilder attempt to load entities from Resources? Note that tiles are always loaded from Resources.")]
        private bool m_loadEntityResources = true;
        #endregion

        #region Private Fields
        private readonly Dictionary<string, GameObject> m_loadedPrefabs = new Dictionary<string, GameObject>();

        private Grid m_gridPrefab;
        private Grid m_gridInstance;
        private Tilemap[] m_tilemaps;
        private Transform m_tilemapContainer;
        private BoundsInt m_gridBounds;
        private BoundsInt m_mapBounds;
        private Vector3 m_offset;
        private TileBase[] m_allTiles;
        private TileBase m_tile;
        private GameObject m_prefab;
        private GameObject m_instance;
        private MapEntity[] m_entities;
        #endregion

        /// <summary>
        /// This method loads a UnityEngine.Tilemap. It scrapes through all Tiles and Entities within the map, replaces them with instantiated prefabs, and then deletes the Tilemap. In other words, you can use this method to build a 3D level using a 2D Tilemap as your blueprint.
        /// </summary>
        /// <param name="callback">Invoke this action when the map has finished loading.</param>
        /// <param name="mapToLoad">The name of the Tilemap to load from Resources. If this parameter is left blank, MapBuilder will fall back on <c>mapToLoad</c>.</param>
        public void LoadMap(Action<MapData> callback, string mapToLoad = null)
        {
            if (string.IsNullOrEmpty(mapToLoad))
            {
                mapToLoad = m_mapToLoad;
            }

            m_gridPrefab = Resources.Load<Grid>(mapToLoad);
            if (m_gridPrefab == null)
            {
                Debug.LogError($"Level \"{mapToLoad}\" could not be found.");
                return;
            }

            if (m_mapParent == null)
            {
                m_mapParent = transform;
            }

            MapData mapData = new MapData
            {
                entities = new List<EntityData>()
            };

            m_gridInstance = Instantiate(m_gridPrefab);
            m_gridInstance.transform.position = Vector3.zero;
            m_gridInstance.transform.localEulerAngles = Vector3.zero;
            m_gridInstance.transform.localScale = Vector3.one;

            mapData.mapProperties = m_gridInstance.GetComponent<MapProperties>();

            m_tilemaps = m_gridInstance.GetComponentsInChildren<Tilemap>();
            for (int i = 0; i < m_tilemaps.Length; i++)
            {
                if (i == 0)
                {
                    m_gridBounds = m_tilemaps[i].cellBounds;
                }
                m_mapBounds = m_tilemaps[i].cellBounds;
                m_offset = m_mapBounds.position - m_gridBounds.position;
                m_allTiles = m_tilemaps[i].GetTilesBlock(m_mapBounds);

                m_tilemapContainer = new GameObject().transform;
                m_tilemapContainer.name = m_tilemaps[i].name;
                m_tilemapContainer.transform.SetParent(m_mapParent);
                m_tilemapContainer.transform.position = new Vector3(m_offset.x, m_offset.z, m_offset.y);
                m_tilemapContainer.transform.localEulerAngles = Vector3.zero;
                m_tilemapContainer.transform.localScale = Vector3.one;
                m_tilemapContainer.gameObject.isStatic = true;

                for (int x = 0; x < m_mapBounds.size.x; x++)
                {
                    for (int y = 0; y < m_mapBounds.size.y; y++)
                    {
                        m_tile = m_allTiles[x + y * m_mapBounds.size.x];
                        if (m_tile == null) continue;
                        CreateObject(m_tile.name, m_tilemapContainer, m_tilemaps[i].CellToWorld(new Vector3Int(x, y, 0)), Vector3.zero, true);
                    }
                }

                m_entities = m_tilemaps[i].GetComponentsInChildren<MapEntity>();
                foreach (MapEntity entity in m_entities)
                {
                    EntityData entityData = new EntityData
                    {
                        entity = entity,
                        properties = entity.MapProperties
                    };

                    if (m_loadEntityResources)
                    {
                        entityData.gameObject = CreateObject
                            (entity.name,
                             m_mapParent,
                             m_tilemaps[i].MapLocalToGridLocal(entity.transform.localPosition, m_gridBounds, m_gridInstance.cellSwizzle),
                             entity.transform.localEulerAngles,
                             false);
                        entityData.gameObject.name = entity.name;
                    }

                    mapData.entities.Add(entityData);
                }
            }

            Destroy(m_gridInstance.gameObject);

            callback?.Invoke(mapData);
        }

        #region Private Methods
        /// <summary>
        /// Instantiate a new GameObject by loading it from Resources.
        /// </summary>
        /// <param name="name">The prefab name.</param>
        /// <param name="parent">Where to place the new object.</param>
        /// <param name="localPosition">The new object's position.</param>
        /// <param name="localEulerAngles">The new object's rotation.</param>
        /// <param name="isStatic">Should the new object be marked as static?</param>
        /// <returns></returns>
        private GameObject CreateObject(string name, Transform parent, Vector3 localPosition, Vector3 localEulerAngles, bool isStatic)
        {
            if (m_loadedPrefabs.TryGetValue(name, out GameObject prefab))
            {
                m_prefab = prefab;
            }
            else
            {
                m_prefab = Resources.Load<GameObject>(name);
                if (m_prefab == null) return null;
                m_loadedPrefabs.Add(name, m_prefab);
            }

            m_instance = Instantiate(m_prefab);
            m_instance.transform.SetParent(parent);
            m_instance.transform.localPosition = localPosition;
            m_instance.transform.localEulerAngles = localEulerAngles;
            m_instance.transform.localScale = Vector3.one;
            m_instance.isStatic = isStatic;

            return m_instance;
        }
        #endregion
    }
}