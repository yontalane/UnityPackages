using UnityEngine;

namespace Yontalane.GridNav.Example
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Grid Nav/Example/Grid Node")]
    public class GridNode : MonoBehaviour, IGridNode
    {
        public Vector2Int coordinate;
        public Vector2Int GetCoordinate() => coordinate;
    }
}