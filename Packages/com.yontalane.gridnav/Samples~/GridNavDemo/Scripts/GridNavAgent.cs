using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Nav Agent")]
    public class GridNavAgent : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        [Range(0f, 1f)]
        private float m_speed;

        [SerializeField]
        private bool m_go;

        [Header("UI")]

        [SerializeField]
        private InputField m_inputX;

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

        private void Start()
        {
            m_gridGenerator = FindObjectOfType<GridGenerator>();

            m_gridNavigator = new GridNavigator(new Vector2Int(m_gridGenerator.GridArray.GetLength(0), m_gridGenerator.GridArray.GetLength(1)), NodeIsValid);
            m_gridNavigator.OnComplete += OnPathingComplete;

            m_initialized = true;
        }

        public void ClickGo() => m_go = true;

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

        private bool NodeIsValid(Vector2Int coordinate)
        {
            if (coordinate.x >= 0 && coordinate.x < m_gridGenerator.GridArray.GetLength(0))
            {
                if (coordinate.y >= 0 && coordinate.y < m_gridGenerator.GridArray.GetLength(1))
                {
                    GridNode gridNode = m_gridGenerator.GridArray[coordinate.x, coordinate.y];
                    return gridNode != null && gridNode.IsPathable;
                }
            }
            return false;
        }

        private void OnPathingComplete(bool foundExists)
        {
            if (foundExists)
            {
                StartCoroutine(NavigatePath());
            }
        }

        private IEnumerator NavigatePath()
        {
            m_shouldDrawGizmos = true;
            for (int i = 0; i < m_gridNavigator.PathCount; i++)
            {
                Vector2Int node = m_gridNavigator.GetPathNode(i);
                transform.position = m_gridGenerator.GridArray[node.x, node.y].transform.position;
                yield return new WaitForSeconds(m_speed);
            }
            m_shouldDrawGizmos = false;
            m_startCoord = m_endCoord;
        }

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