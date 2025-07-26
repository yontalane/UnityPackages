using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Nav Agent")]
    /// <summary>
    /// Controls an agent that navigates a grid using pathfinding, manages user input for destination coordinates, and handles movement logic and UI integration.
    /// </summary>
    public class GridNavAgent : MonoBehaviour
    {
        private bool m_navigating = false;

        #region Serialized Fields
        [Tooltip("The movement speed of the agent (0 = stationary, 1 = maximum speed).")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_speed;

        [Tooltip("If true, the agent will begin navigating to the specified destination.")]
        [SerializeField]
        private bool m_go;

        [Header("UI")]

        [Tooltip("Input field for the X coordinate of the destination.")]
        [SerializeField]
        private InputField m_inputX;

        [Tooltip("Input field for the Y coordinate of the destination.")]
        [SerializeField]
        private InputField m_inputY;
        #endregion

        #region Private Variables
        private bool m_initialized = false;
        private GridGenerator m_gridGenerator;
        private GridNavigator m_gridNavigator;
        private Vector2Int m_startCoord = Vector2Int.zero;
        private Vector2Int m_endCoord;
        private bool m_shouldDrawGizmos = false;
        #endregion

        private void Reset()
        {
            m_speed = 0.1f;
            m_go = false;
            m_inputX = null;
            m_inputY = null;
        }

        private void OnEnable()
        {
            if (m_initialized)
            {
                m_gridNavigator.OnComplete += OnPathingComplete;
            }
        }

        private void OnDisable()
        {
            if (m_initialized)
            {
                m_gridNavigator.OnComplete -= OnPathingComplete;
            }
        }

        /// <summary>
        /// Initializes the grid generator and grid navigator, and subscribes to the pathing complete event.
        /// </summary>
        private void Start()
        {
            m_gridGenerator = FindAnyObjectByType<GridGenerator>();

            m_gridNavigator = new GridNavigator(new Vector2Int(m_gridGenerator.GridArray.GetLength(0), m_gridGenerator.GridArray.GetLength(1)), NodeIsValid, StepIsValid);
            m_gridNavigator.OnComplete += OnPathingComplete;

            m_initialized = true;
        }

        /// <summary>
        /// Called by UI to start navigation if not already navigating.
        /// </summary>
        public void ClickGo()
        {
            if (m_navigating)
            {
                return;
            }

            m_go = true;
        }

        /// <summary>
        /// Sets the destination coordinates and triggers navigation.
        /// </summary>
        /// <param name="target">The target grid coordinate to navigate to.</param>
        public void GoTo(Vector2Int target)
        {
            if (m_navigating)
            {
                return;
            }

            m_inputX.text = target.x.ToString();
            m_inputY.text = target.y.ToString();
            m_go = true;
        }

        /// <summary>
        /// Checks for navigation trigger and initiates pathfinding if requested.
        /// </summary>
        private void Update()
        {
            if (m_go)
            {
                if (int.TryParse(m_inputX.text, out int x))
                {
                    m_endCoord.x = x;
                }
                if (int.TryParse(m_inputY.text, out int y))
                {
                    m_endCoord.y = y;
                }

                m_gridNavigator.FindPath(m_startCoord, m_endCoord);

                m_go = false;
            }
        }

        /// <summary>
        /// Determines if a node at the given coordinates is valid and pathable.
        /// </summary>
        private bool NodeIsValid(int x, int y)
        {
            if (x >= 0 && x < m_gridGenerator.GridArray.GetLength(0))
            {
                if (y >= 0 && y < m_gridGenerator.GridArray.GetLength(1))
                {
                    GridNode gridNode = m_gridGenerator.GridArray[x, y];
                    return gridNode != null && gridNode.IsPathable;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if a step between two nodes is valid, considering blockers and adjacency.
        /// </summary>
        private bool StepIsValid(int startX, int startY, int endX, int endY)
        {
            // Only restrict steps that are directly adjacent (not diagonals)
            if (!(startX == endX && Mathf.Abs(startY - endY) == 1) && !(startY == endY && Mathf.Abs(startX - endX) == 1))
            {
                return true;
            }

            // Out of bounds checks
            if (startX < 0 || startX >= m_gridGenerator.GridArray.GetLength(0))
            {
                return true;
            }
            if (startY < 0 || startY >= m_gridGenerator.GridArray.GetLength(1))
            {
                return true;
            }
            if (endX < 0 || endX >= m_gridGenerator.GridArray.GetLength(0))
            {
                return true;
            }
            if (endY < 0 || endY >= m_gridGenerator.GridArray.GetLength(1))
            {
                return true;
            }

            GridNode startNode = m_gridGenerator.GridArray[startX, startY];
            GridNode endNode = m_gridGenerator.GridArray[endX, endY];

            if (startNode == null || endNode == null)
            {
                return true;
            }

            // Check for blockers between nodes
            if (Mathf.Approximately(startNode.transform.position.x, endNode.transform.position.x))
            {
                if (startNode.transform.position.z > endNode.transform.position.z)
                {
                    return !startNode.BlockerS && !endNode.BlockerN;
                }
                else
                {
                    return !endNode.BlockerS && !startNode.BlockerN;
                }
            }
            else
            {
                if (startNode.transform.position.x > endNode.transform.position.x)
                {
                    return !startNode.BlockerW && !endNode.BlockerE;
                }
                else
                {
                    return !endNode.BlockerW && !startNode.BlockerE;
                }
            }
        }

        /// <summary>
        /// Callback for when pathfinding completes; starts navigation if a path was found.
        /// </summary>
        /// <param name="foundExists">Whether a valid path was found.</param>
        private void OnPathingComplete(bool foundExists)
        {
            if (foundExists)
            {
                StartCoroutine(NavigatePath());
            }
        }

        /// <summary>
        /// Coroutine that moves the agent along the found path, animating step by step.
        /// </summary>
        private IEnumerator NavigatePath()
        {
            m_navigating = true;

            m_shouldDrawGizmos = true;
            for (int i = 0; i < m_gridNavigator.PathCount; i++)
            {
                Vector2Int node = m_gridNavigator.GetPathNode(i);
                transform.position = m_gridGenerator.GridArray[node.x, node.y].transform.position;
                yield return new WaitForSeconds(m_speed);
            }
            m_shouldDrawGizmos = false;
            m_startCoord = m_endCoord;

            m_navigating = false;
        }

        /// <summary>
        /// Draws gizmos in the editor to visualize the current path.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (m_shouldDrawGizmos && m_gridNavigator.PathCount > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < m_gridNavigator.PathCount - 1; i++)
                {
                    Vector2Int nodeA = m_gridNavigator.GetPathNode(i + 1);
                    Vector2Int nodeB = m_gridNavigator.GetPathNode(i);
                    Gizmos.DrawLine(m_gridGenerator.GridArray[nodeA.x, nodeA.y].transform.position, m_gridGenerator.GridArray[nodeB.x, nodeB.y].transform.position);
                }
            }
        }
    }
}