using UnityEngine;

namespace Yontalane.GridNav
{
    public interface IGridNode
    {
        public Vector2Int GetCoordinate();
    }
}