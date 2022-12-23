using UnityEngine;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Node")]
    [RequireComponent(typeof(Renderer))]
    public class GridNode : MonoBehaviour
    {
        #region Serialized Fields
        [Tooltip("Material representing a pathable node.")]
        [SerializeField]
        private Material m_pathableMaterial;

        [Tooltip("Material representing a non-pathable node.")]
        [SerializeField]
        private Material m_nonPathableMaterial;
        #endregion

        #region Accessors
        public Vector2Int Coordinate { get; set; }

        public bool IsPathable
        {
            get => GetComponent<Renderer>().sharedMaterial == m_pathableMaterial;
            set => GetComponent<Renderer>().sharedMaterial = value ? m_pathableMaterial : m_nonPathableMaterial;
        }
        #endregion

        private void Reset()
        {
            m_pathableMaterial = null;
            m_nonPathableMaterial = null;
        }
    }
}