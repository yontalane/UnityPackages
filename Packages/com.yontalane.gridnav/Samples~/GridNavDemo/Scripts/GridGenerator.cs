using UnityEngine;
using UnityEngine.InputSystem;

namespace Yontalane.GridNav.Example
{
    /// <summary>
    /// Generates a grid of nodes for pathfinding, manages grid initialization, and provides access to the grid array.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Generator")]
    public sealed class GridGenerator : MonoBehaviour
    {
        #region Private Variables
        private GridNavAgent m_agent;
        private bool m_mouseDown = false;
        #endregion

        #region Serialized Fields
        [Tooltip("The size of the grid in X (width) and Y (height) dimensions.")]
        [SerializeField]
        private Vector2Int m_gridSize;

        [Tooltip("Distance between nodes.")]
        [SerializeField]
        private float m_scale;

        [Tooltip("Prefab for the grid node to instantiate for each cell in the grid.")]
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
        /// <summary>
        /// Gets the 2D array representing the grid of nodes used for pathfinding.
        /// </summary>
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

        /// <summary>
        /// Initializes the grid by instantiating grid nodes, setting their positions, and configuring their initial pathability and blockers.
        /// </summary>
        private void Awake()
        {
            m_agent = FindAnyObjectByType<GridNavAgent>();

            GridArray = new GridNode[m_gridSize.x, m_gridSize.y];
            // Loop through each grid coordinate and instantiate a GridNode at the correct world position.
            // For each node:
            //   - Set its parent to this GameObject.
            //   - Assign its grid coordinate.
            //   - If it's the bottom-left node (0,0), ensure it's pathable and clear all blockers.
            //   - Otherwise, randomly determine if it's pathable based on m_pathableChance.
            //   - Store the node in the GridArray for later access.
            for (int x = 0; x < m_gridSize.x; x++)
            {
                for (int y = 0; y < m_gridSize.y; y++)
                {
                    GridNode gridNode = Instantiate(
                        m_gridNodePrefab.gameObject,
                        new Vector3(m_leftBottomPosition.x + m_scale * x, m_leftBottomPosition.y, m_leftBottomPosition.z + m_scale * y),
                        Quaternion.identity
                    ).GetComponent<GridNode>();
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

        /// <summary>
        /// Handles mouse input for highlighting grid nodes and triggering agent movement to a selected node.
        /// </summary>
        private void Update()
        {
            // Summary:
            // This block handles mouse input for interacting with grid nodes in the scene.
            // - It casts a ray from the mouse position into the scene to detect if a GridNode is under the cursor.
            // - If a GridNode is detected, it highlights the node visually.
            // - If the left mouse button is pressed while hovering over a node (and wasn't already pressed), it instructs the agent to move to that node's coordinate.
            // - Tracks the mouse button state to prevent repeated triggers while holding the button.

            bool highlightingNode = false;
            GridNode node = null;

            // Cast a ray from the mouse position to detect a grid node under the cursor
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value, Camera.MonoOrStereoscopicEye.Mono);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out node))
                {
                    highlightingNode = true;
                    node.Highlight();
                }
            }

            // Handle left mouse button input for selecting a node
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