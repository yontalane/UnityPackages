using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Yontalane.LayoutTilemap
{
    #region Structs
    /// <summary>
    /// Represents data for a single tile in the map, including its existence, name, and world position.
    /// </summary>
    public struct TileData
    {
        /// <summary>
        /// Indicates whether a tile exists at the specified location.
        /// </summary>
        public bool exists;

        /// <summary>
        /// The name or identifier of the tile.
        /// </summary>
        public string name;

        /// <summary>
        /// The world position of the tile in 3D space.
        /// </summary>
        public Vector3 worldPosition;
    }
    
    /// <summary>
    /// Contains information about an entity placed on the map, such as its name, position, rotation, bounds, reference to the entity, properties, and associated GameObject.
    /// </summary>
    public struct EntityData
    {
        /// <summary>
        /// The name or identifier of the entity.
        /// </summary>
        public string name;

        /// <summary>
        /// The world position of the entity in 3D space.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The rotation of the entity, expressed in Euler angles.
        /// </summary>
        public Vector3 eulerAngles;

        /// <summary>
        /// Indicates whether the entity has valid bounds information.
        /// </summary>
        public bool hasBounds;

        /// <summary>
        /// The bounding box of the entity, if available.
        /// </summary>
        public Bounds bounds;

        /// <summary>
        /// Reference to the MapEntity component associated with this entity.
        /// </summary>
        public MapEntity entity;

        /// <summary>
        /// A dictionary of custom properties associated with the entity.
        /// </summary>
        public MapPropertyDictionary properties;

        /// <summary>
        /// The GameObject instance representing the entity in the scene.
        /// </summary>
        public GameObject gameObject;
    }

    /// <summary>
    /// Holds all relevant data for a map, including its name, tile data collection, entities, properties, bounds, and parent transform.
    /// </summary>
    public struct MapData
    {
        /// <summary>
        /// The name or identifier of the map.
        /// </summary>
        public string name;

        /// <summary>
        /// The collection of tile data for all tilemaps in the map.
        /// </summary>
        public TileDataCollection tileDataCollection;

        /// <summary>
        /// The list of entities placed within the map.
        /// </summary>
        public List<EntityData> entities;

        /// <summary>
        /// A dictionary of custom properties associated with the map.
        /// </summary>
        public MapPropertyDictionary properties;

        /// <summary>
        /// The bounding box that encapsulates the map area.
        /// </summary>
        public Bounds bounds;

        /// <summary>
        /// The parent transform under which the map is organized in the scene hierarchy.
        /// </summary>
        public Transform mapParent;
    }
    #endregion

    #region Extra Classes
    /// <summary>
    /// Manages a hierarchical collection of tile data, organized by tilemaps and rows, for use in map construction and manipulation.
    /// </summary>
    public class TileDataCollection
    {
        #region Private Variables
        private readonly List<List<List<TileData>>> m_list = new List<List<List<TileData>>>();
        #endregion

        #region Constructor
        public TileDataCollection() => m_list.Clear();
        #endregion

        public void AddTilemap() => m_list.Add(new List<List<TileData>>());

        public void AddRow()
        {
            if (m_list.Count == 0)
            {
                AddTilemap();
            }

            List<List<TileData>> tilemap = m_list[m_list.Count - 1];
            tilemap.Add(new List<TileData>());
        }

        public void AddTileData(TileData tileData)
        {
            if (m_list.Count == 0)
            {
                AddTilemap();
            }

            List<List<TileData>> tilemap = m_list[m_list.Count - 1];

            if (tilemap.Count == 0)
            {
                AddRow();
            }

            List<TileData> row = tilemap[tilemap.Count - 1];

            row.Add(tileData);
        }

        public int GetTilemapCount() => m_list.Count;

        public int GetRowCount(int tilemapIndex)
        {
            if (tilemapIndex < 0 || tilemapIndex >= GetTilemapCount())
            {
                return -1;
            }

            return m_list[tilemapIndex].Count;
        }

        public int GetColumnCount(int tilemapIndex, int rowIndex)
        {
            if (tilemapIndex < 0 || tilemapIndex >= GetTilemapCount())
            {
                return -1;
            }

            if (rowIndex < 0 || rowIndex >= GetRowCount(tilemapIndex))
            {
                return -1;
            }

            return m_list[tilemapIndex][rowIndex].Count;
        }

        public bool TryGetTileData(int tilemapIndex, int rowIndex, int columnIndex, out TileData tileData)
        {
            int columnCount = GetColumnCount(tilemapIndex, rowIndex);

            if (columnCount <= columnIndex)
            {
                tileData = default;
                return false;
            }

            tileData = m_list[tilemapIndex][rowIndex][columnIndex];
            return true;
        }

        public TileData GetTileData(int tilemapIndex, int rowIndex, int columnIndex)
        {
            if (TryGetTileData(tilemapIndex, rowIndex, columnIndex, out TileData tileData))
            {
                return tileData;
            }
            else
            {
                return default;
            }
        }

        private T[,] GetArray<T>(int tilemapIndex, Func<Vector2Int, T> getItem)
        {
            if (tilemapIndex < 0 || tilemapIndex >= GetTilemapCount())
            {
                return default;
            }

            List<List<TileData>> tilemap = m_list[tilemapIndex];

            int longestRow = 0;
            foreach(List<TileData> row in tilemap)
            {
                longestRow = Mathf.Max(row.Count, longestRow);
            }

            T[,] array = new T[longestRow, tilemap.Count];

            for (int y = 0; y < array.GetLength(1); y++)
            {
                List<TileData> row = tilemap[y];
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    array[x, y] = getItem(new Vector2Int(x, y));
                }
            }

            return array;
        }

        public string[,] GetNamesArray(int tilemapIndex)
        {
            return GetArray<string>(tilemapIndex, (Vector2Int mapPosition) =>
            {
                List<TileData> row = m_list[tilemapIndex][mapPosition.y];
                return mapPosition.x < row.Count ? row[mapPosition.x].name : string.Empty;
            });
        }

        public bool[,] GetExistArray(int tilemapIndex)
        {
            return GetArray<bool>(tilemapIndex, (Vector2Int mapPosition) =>
            {
                List<TileData> row = m_list[tilemapIndex][mapPosition.y];
                return mapPosition.x < row.Count ? row[mapPosition.x].exists : false;
            });
        }
    }
    #endregion

    /// <summary>
    /// MapBuilder is a MonoBehaviour component that loads and builds maps from Unity Tilemaps,
    /// instantiating prefabs for tiles and entities, and managing their placement in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Layout Tilemap/Map Builder")]
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
        Vector3 m_tilePosition;
        Bounds m_tileBounds;
        Vector3 m_boundsMin;
        Vector3 m_boundsMax;
        private GameObject m_prefab;
        private GameObject m_instance;
        private MapEntity[] m_entities;
        private PersistentObject[] m_persistentObjects;
        #endregion

        /// <summary>
        /// This method loads a UnityEngine.Tilemap. It scrapes through all Tiles and Entities within the map, replaces them with instantiated prefabs, and then deletes the Tilemap. In other words, you can use this method to build a 3D level using a 2D Tilemap as your blueprint.
        /// </summary>
        /// <param name="callback">Invoke this action when the map has finished loading.</param>
        /// <param name="mapToLoad">The name of the Tilemap to load from Resources. If this parameter is left blank, MapBuilder will fall back on <c>mapToLoad</c>.</param>
        public void LoadMap(Action<MapData> callback) => LoadMap(callback, m_mapToLoad);

        /// <summary>
        /// This method loads a UnityEngine.Tilemap. It scrapes through all Tiles and Entities within the map, replaces them with instantiated prefabs, and then deletes the Tilemap. In other words, you can use this method to build a 3D level using a 2D Tilemap as your blueprint.
        /// </summary>
        /// <param name="callback">Invoke this action when the map has finished loading.</param>
        /// <param name="mapToLoad">The name of the Tilemap to load from Resources. If this parameter is left blank, MapBuilder will fall back on <c>mapToLoad</c>.</param>
        public void LoadMap(Action<MapData> callback, string mapToLoad)
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

            m_gridInstance = Instantiate(m_gridPrefab);
            m_gridInstance.name = mapToLoad;

            LoadMap(callback, m_gridInstance);
        }

        /// <summary>
        /// This method loads a UnityEngine.Tilemap. It scrapes through all Tiles and Entities within the map, replaces them with instantiated prefabs, and then deletes the Tilemap. In other words, you can use this method to build a 3D level using a 2D Tilemap as your blueprint.
        /// </summary>
        /// <param name="callback">Invoke this action when the map has finished loading.</param>
        /// <param name="mapToLoad">The name of the Tilemap to load from Resources. If this parameter is left blank, MapBuilder will fall back on <c>mapToLoad</c>.</param>
        public void LoadMap(Action<MapData> callback, Grid mapToLoad)
        {
            m_gridInstance = mapToLoad;

            m_gridInstance.transform.position = Vector3.zero;
            m_gridInstance.transform.localEulerAngles = Vector3.zero;
            m_gridInstance.transform.localScale = Vector3.one;

            MapData mapData = new MapData
            {
                name = m_gridInstance.name,
                entities = new List<EntityData>()
            };

            if (m_mapParent == null)
            {
                m_mapParent = transform;
            }

            MapProperties mapProperties = m_gridInstance.GetComponent<MapProperties>();
            mapData.properties = mapProperties != null ? mapProperties.Properties : new MapPropertyDictionary();

            mapData.tileDataCollection = new TileDataCollection();
            m_tilemaps = m_gridInstance.GetComponentsInChildren<Tilemap>();
            for (int i = 0; i < m_tilemaps.Length; i++)
            {
                mapData.tileDataCollection.AddTilemap();

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

                for (int y = 0; y < m_mapBounds.size.y; y++)
                {
                    mapData.tileDataCollection.AddRow();
                    for (int x = 0; x < m_mapBounds.size.x; x++)
                    {
                        m_tile = m_allTiles[x + y * m_mapBounds.size.x];
                        if (m_tile == null)
                        {
                            mapData.tileDataCollection.AddTileData(new TileData()
                            {
                                exists = false
                            });
                            continue;
                        }
                        m_tilePosition = m_tilemaps[i].CellToWorld(new Vector3Int(x, y, 0));
                        m_tileBounds = new Bounds(m_tilePosition, m_tilemaps[i].cellSize);
                        CreateObject(m_tile.name, m_tilemapContainer, m_tilePosition, Vector3.zero, true);

                        mapData.tileDataCollection.AddTileData(new TileData()
                        {
                            exists = true,
                            name = m_tile.name,
                            worldPosition = m_tilePosition
                        });

                        if (i == 0 && x == 0 && y == 0)
                        {
                            mapData.bounds = m_tileBounds;
                        }
                        else
                        {
                            mapData.bounds.Encapsulate(m_tileBounds);
                        }
                    }
                }

                m_entities = m_tilemaps[i].GetComponentsInChildren<MapEntity>();
                foreach (MapEntity entity in m_entities)
                {
                    EntityData entityData = new()
                    {
                        name = entity.name,
                        position = m_tilemaps[i].MapLocalToGridLocal(entity.transform.localPosition, m_gridBounds, m_gridInstance.cellSwizzle),
                        eulerAngles = entity.transform.localEulerAngles,
                        entity = entity,
                        properties = entity.MapProperties.Properties
                    };

                    if (entity.TryGetComponent(out Collider collider))
                    {
                        entityData.hasBounds = true;

                        m_boundsMin = m_tilemaps[i].transform.InverseTransformPoint(collider.bounds.min);
                        m_boundsMax = m_tilemaps[i].transform.InverseTransformPoint(collider.bounds.max);

                        m_boundsMin = m_tilemaps[i].MapLocalToGridLocal(m_boundsMin, m_gridBounds, m_gridInstance.cellSwizzle);
                        m_boundsMax = m_tilemaps[i].MapLocalToGridLocal(m_boundsMax, m_gridBounds, m_gridInstance.cellSwizzle);

                        entityData.bounds = new Bounds();
                        entityData.bounds.SetMinMax(m_boundsMin, m_boundsMax);
                    }
                    else
                    {
                        entityData.hasBounds = false;
                        entityData.bounds = new Bounds(entityData.position, Vector3.zero);
                    }

                    if (m_loadEntityResources)
                    {
                        entityData.gameObject = CreateObject
                            (entity.name,
                             m_mapParent,
                             entityData.position,
                             entityData.eulerAngles,
                             false);
                        if (entityData.gameObject != null)
                        {
                            entityData.gameObject.name = entity.name;
                        }
                    }

                    mapData.entities.Add(entityData);
                }

                m_persistentObjects = m_tilemaps[i].GetComponentsInChildren<PersistentObject>();
                foreach (PersistentObject persistentObject in m_persistentObjects)
                {
                    m_instance = Instantiate(persistentObject.gameObject);
                    m_instance.transform.SetParent(m_mapParent);
                    m_instance.transform.localPosition = m_tilemaps[i].MapLocalToGridLocal(persistentObject.transform.localPosition, m_gridBounds, m_gridInstance.cellSwizzle);
                    m_instance.transform.localEulerAngles = persistentObject.transform.localEulerAngles;
                    m_instance.transform.localScale = persistentObject.transform.localScale;
                    m_instance.isStatic = persistentObject.gameObject.isStatic;
                    m_instance.name = persistentObject.name;
                }
            }

            mapData.mapParent = m_mapParent;

            DestroyImmediate(m_gridInstance.gameObject);

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