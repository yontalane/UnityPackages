using UnityEngine;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Generator")]
    public sealed class GridGenerator : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Vector2Int m_gridSize;

        [Tooltip("Distance between nodes.")]
        [SerializeField]
        private int m_scale;

        [SerializeField]
        private GridNode m_gridNodePrefab;

        [Tooltip("Where to start the grid.")]
        [SerializeField]
        private Vector3 m_leftBottomPosition;

        [Tooltip("Likelihood of null node.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_chanceToNotCreate;
        #endregion

        #region Accessors
        public GridNode[,] GridArray { get; private set; }
        #endregion

        private void Reset()
        {
            m_gridSize = new Vector2Int(10, 10);
            m_scale = 1;
            m_gridNodePrefab = null;
            m_leftBottomPosition = new Vector3();
            m_chanceToNotCreate = 0.15f;
        }

        private void Awake()
        {
            GridArray = new GridNode[m_gridSize.x, m_gridSize.y];
            for (int x = 0; x < m_gridSize.x; x++)
            {
                for (int y = 0; y < m_gridSize.y; y++)
                {
                    if (Random.value < m_chanceToNotCreate)
                    {
                        continue;
                    }
                    GridNode gridNode = Instantiate(m_gridNodePrefab.gameObject, new Vector3(m_leftBottomPosition.x + m_scale * x, m_leftBottomPosition.y, m_leftBottomPosition.z + m_scale * y), Quaternion.identity).GetComponent<GridNode>();
                    gridNode.transform.SetParent(gameObject.transform);
                    gridNode.coordinate = new Vector2Int(x, y);
                    GridArray[x, y] = gridNode;
                }
            }
        }
    }
}