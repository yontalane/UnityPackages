using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Node")]
    [RequireComponent(typeof(Renderer))]
    public class GridNode : MonoBehaviour
    {
        private bool m_highlighting = false;

        #region Serialized Fields
        [Header("Materials")]

        [Tooltip("Material representing a pathable node.")]
        [SerializeField]
        private Material m_pathableMaterial;

        [Tooltip("Material representing a non-pathable node.")]
        [SerializeField]
        private Material m_nonPathableMaterial;

        [Header("Blockers")]

        [Tooltip("A blocker between nodes.")]
        [SerializeField]
        private GameObject m_blockerN;

        [Tooltip("A blocker between nodes.")]
        [SerializeField]
        private GameObject m_blockerE;

        [Tooltip("A blocker between nodes.")]
        [SerializeField]
        private GameObject m_blockerS;

        [Tooltip("A blocker between nodes.")]
        [SerializeField]
        private GameObject m_blockerW;
        #endregion

        #region Accessors
        public Vector2Int Coordinate { get; set; }

        public bool IsPathable
        {
            get => GetComponent<Renderer>().sharedMaterial == m_pathableMaterial;
            set => GetComponent<Renderer>().sharedMaterial = value ? m_pathableMaterial : m_nonPathableMaterial;
        }

        public bool BlockerN
        {
            get => m_blockerN.activeSelf;
            set
            {
                m_blockerN.SetActive(value);
            }
        }

        public bool BlockerE
        {
            get => m_blockerE.activeSelf;
            set
            {
                m_blockerE.SetActive(value);
            }
        }

        public bool BlockerS
        {
            get => m_blockerS.activeSelf;
            set
            {
                m_blockerS.SetActive(value);
            }
        }

        public bool BlockerW
        {
            get => m_blockerW.activeSelf;
            set
            {
                m_blockerW.SetActive(value);
            }
        }
        #endregion

        private void Reset()
        {
            m_pathableMaterial = null;
            m_nonPathableMaterial = null;

            m_blockerN = null;
            m_blockerE = null;
            m_blockerS = null;
            m_blockerW = null;
        }

        private void Awake()
        {
            //BlockerN = Random.value < 0.05f;
            //BlockerE = Random.value < 0.05f;
            //BlockerS = Random.value < 0.05f;
            //BlockerW = Random.value < 0.05f;
            ClearBlockers();
        }

        public void ClearBlockers()
        {
            BlockerN = false;
            BlockerE = false;
            BlockerS = false;
            BlockerW = false;
        }

        public void Highlight()
        {
            if (m_highlighting)
            {
                return;
            }

            StartCoroutine(HighlightAnimation());
        }

        private IEnumerator HighlightAnimation()
        {
            m_highlighting = true;

            float offset = 0.1f;
            Vector3 originalPosition = transform.position;
            transform.position = originalPosition + new Vector3()
            {
                x = Random.Range(-offset, offset),
                y = Random.Range(-offset, offset),
                z = Random.Range(-offset, offset)
            };
            yield return new WaitForEndOfFrame();
            transform.position = originalPosition;

            m_highlighting = false;
        }
    }
}