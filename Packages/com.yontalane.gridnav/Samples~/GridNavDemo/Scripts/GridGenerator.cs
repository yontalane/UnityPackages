using UnityEngine;
using UnityEngine.InputSystem;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Generator")]
    public sealed class GridGenerator : MonoBehaviour
    {
        #region Private Variables
        private GridNavAgent m_agent;
        private bool m_mouseDown = false;
        #endregion

        #region Serialized Fields
        [SerializeField]
        private Vector2Int m_gridSize;

        [Tooltip("Distance between nodes.")]
        [SerializeField]
        private float m_scale;

        [SerializeField]
        private GridNode m_gridNodePrefab;

        [Tooltip("Where to start the grid.")]
        [SerializeField]
        private Vector3 m_leftBottomPosition;

        [Tooltip("Likelihood of blocking node.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_pathableChance;
        #endregion

        #region Accessors
        public GridNode[,] GridArray { get; private set; }
        #endregion

        private void Reset()
        {
            m_gridSize = new Vector2Int(10, 10);
            m_scale = 1f;
            m_gridNodePrefab = null;
            m_leftBottomPosition = new Vector3();
            m_pathableChance = 0.15f;
        }

        private void Awake()
        {
            m_agent = FindObjectOfType<GridNavAgent>();

            GridArray = new GridNode[m_gridSize.x, m_gridSize.y];
            for (int x = 0; x < m_gridSize.x; x++)
            {
                for (int y = 0; y < m_gridSize.y; y++)
                {
                    GridNode gridNode = Instantiate(m_gridNodePrefab.gameObject, new Vector3(m_leftBottomPosition.x + m_scale * x, m_leftBottomPosition.y, m_leftBottomPosition.z + m_scale * y), Quaternion.identity).GetComponent<GridNode>();
                    gridNode.transform.SetParent(gameObject.transform);
                    gridNode.Coordinate = new Vector2Int(x, y);
                    if (x == 0 && y == 0)
                    {
                        gridNode.IsPathable = true;
                        gridNode.ClearBlockers();
                    }
                    else
                    {
                        gridNode.IsPathable = Random.value < m_pathableChance;
                    }
                    GridArray[x, y] = gridNode;
                }
            }
        }

        private void Update()
        {
            bool highlightingNode = false;
            GridNode node = null;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value, Camera.MonoOrStereoscopicEye.Mono);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out node))
                {
                    highlightingNode = true;
                    node.Highlight();
                }
            }

            if (Mouse.current.leftButton.isPressed)
            {
                if (!m_mouseDown && highlightingNode)
                {
                    m_agent.GoTo(node.Coordinate);
                }

                m_mouseDown = true;
            }
            else
            {
                m_mouseDown = false;
            }
        }
    }
}