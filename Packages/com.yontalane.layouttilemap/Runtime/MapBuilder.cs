using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Yontalane.LayoutTilemap
{
    #region Structs
    public struct TileData
    {
        public bool exists;
        public string name;
        public Vector3 worldPosition;
    }
    
    public struct EntityData
    {
        public string name;
        public Vector3 position;
        public Vector3 eulerAngles;
        public bool hasBounds;
        public Bounds bounds;
        public MapEntity entity;
        public MapPropertyDictionary properties;
        public GameObject gameObject;
    }

    public struct MapData
    {
        public string name;
        public TileDataCollection tileDataCollection;
        public List<EntityData> entities;
        public MapPropertyDictionary properties;
        public Bounds bounds;
        public Transform mapParent;
    }
    #endregion

    #region Extra Classes
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
                name = mapToLoad,
                entities = new List<EntityData>()
            };

            m_gridInstance = Instantiate(m_gridPrefab);
            m_gridInstance.transform.position = Vector3.zero;
            m_gridInstance.transform.localEulerAngles = Vector3.zero;
            m_gridInstance.transform.localScale = Vector3.one;

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
                    EntityData entityData = new EntityData
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