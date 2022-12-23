using UnityEngine;

namespace Yontalane.GridNav
{
    public interface IGridNode
    {
        public Transform transform { get; }
        public Vector2Int GetCoordinate();
    }
}