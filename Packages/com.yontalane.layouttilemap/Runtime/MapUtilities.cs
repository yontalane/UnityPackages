using UnityEngine;
using UnityEngine.Tilemaps;

namespace Yontalane.LayoutTilemap
{
    /// <summary>
    /// Provides utility methods for working with maps and tilemaps in the LayoutTilemap system.
    /// </summary>
    public static class MapUtilities
    {
        public static Vector3 MapLocalToGridLocal(this Tilemap tilemap, Vector3 localPosition, BoundsInt mapBounds, GridLayout.CellSwizzle swizzle)
        {
            Vector3Int cell = tilemap.LocalToCell(localPosition);

            Vector3 a = new()
            {
                x = tilemap.transform.localPosition.x + (cell.x - mapBounds.xMin) * tilemap.cellSize.x,
                y = tilemap.transform.localPosition.z + (cell.y - mapBounds.yMin) * tilemap.cellSize.y,
                z = tilemap.transform.localPosition.y + (cell.z - mapBounds.zMin) * tilemap.cellSize.z
            };
            
            Vector3 b = new();

            switch (swizzle)
            {
                case GridLayout.CellSwizzle.XYZ:
                    b.x = a.x;
                    b.y = a.y;
                    b.z = a.z;
                    break;
                case GridLayout.CellSwizzle.XZY:
                    b.x = a.x;
                    b.y = a.z;
                    b.z = a.y;
                    break;
                case GridLayout.CellSwizzle.YXZ:
                    b.x = a.y;
                    b.y = a.x;
                    b.z = a.z;
                    break;
                case GridLayout.CellSwizzle.YZX:
                    b.x = a.y;
                    b.y = a.z;
                    b.z = a.x;
                    break;
                case GridLayout.CellSwizzle.ZXY:
                    b.x = a.z;
                    b.y = a.x;
                    b.z = a.y;
                    break;
                case GridLayout.CellSwizzle.ZYX:
                    b.x = a.z;
                    b.y = a.y;
                    b.z = a.x;
                    break;
            }

            return b;
        }
    }
}