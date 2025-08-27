using UnityEngine;
using UnityEngine.Tilemaps;

namespace Yontalane.LayoutTilemap
{
    /// <summary>
    /// Provides utility methods for working with maps and tilemaps in the LayoutTilemap system.
    /// </summary>
    public static class MapUtilities
    {
        /// <summary>
        /// Converts a local position within a Tilemap to a local position within the grid, taking into account the map bounds and cell swizzle order.
        /// </summary>
        /// <param name="tilemap">The Tilemap to convert from.</param>
        /// <param name="localPosition">The local position within the Tilemap.</param>
        /// <param name="mapBounds">The bounds of the map in cell coordinates.</param>
        /// <param name="swizzle">The cell swizzle order to apply to the resulting position.</param>
        /// <returns>The corresponding local position within the grid, after applying bounds and swizzle.</returns>
        public static Vector3 MapLocalToGridLocal(this Tilemap tilemap, Vector3 localPosition, BoundsInt mapBounds, GridLayout.CellSwizzle swizzle)
        {
            // Convert the given local position within the Tilemap to cell coordinates.
            Vector3Int cell = tilemap.LocalToCell(localPosition);

            // Calculate the grid-local position (before swizzle) based on the tilemap's transform, cell position, and map bounds.
            Vector3 a = new()
            {
                x = tilemap.transform.localPosition.x + (cell.x - mapBounds.xMin) * tilemap.cellSize.x,
                y = tilemap.transform.localPosition.z + (cell.y - mapBounds.yMin) * tilemap.cellSize.y,
                z = tilemap.transform.localPosition.y + (cell.z - mapBounds.zMin) * tilemap.cellSize.z
            };

            Vector3 b = new();

            // Apply the specified cell swizzle order to rearrange the coordinates as needed for the grid layout.
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