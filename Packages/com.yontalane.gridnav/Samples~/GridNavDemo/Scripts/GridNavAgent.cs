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
        private GridNavigator<GridNode> m_gridNavigator;
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
                m_gridNavigator.OnFoundPath += GridBehavior_OnFoundPath;
            }
        }

        private void OnDisable()
        {
            if (m_initialized)
            {
                m_gridNavigator.OnFoundPath -= GridBehavior_OnFoundPath;
            }
        }

        private void Start()
        {
            m_gridGenerator = FindObjectOfType<GridGenerator>();

            m_gridNavigator = new GridNavigator<GridNode>();
            m_gridNavigator.OnFoundPath += GridBehavior_OnFoundPath;

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

                m_gridNavigator.FindPath(m_startCoord, m_endCoord, m_gridGenerator.GridArray);
                m_go = false;
            }
        }

        private void GridBehavior_OnFoundPath(bool foundExists)
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
                transform.position = m_gridNavigator.GetPathNode(i).transform.position;
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
                    Gizmos.DrawLine(m_gridNavigator.GetPathNode(i + 1).transform.position, m_gridNavigator.GetPathNode(i).transform.position);
                }
            }
        }
    }
}