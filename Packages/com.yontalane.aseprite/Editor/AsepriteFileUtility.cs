using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// Provides utility methods for working with Aseprite file data, including collision layer detection,
    /// rectangle extraction, and pivot point calculation.
    /// </summary>
    internal static class AsepriteFileUtility
    {
        private const float PIXEL_COLOR_MIN = 0.05f;

        private static readonly List<Color32> s_pixels = new();

        /// <summary>
        /// Attempts to cast a BaseChunk to a CellChunk and checks if it belongs to a custom or normal layer.
        /// </summary>
        /// <param name="chunk">The chunk to cast.</param>
        /// <param name="layers">The list of layer data to determine the layer type.</param>
        /// <param name="cellChunk">The resulting CellChunk if the cast and layer check succeed; otherwise, null.</param>
        /// <param name="desireCustomLayer">If true, only succeeds for custom (non-Normal) layers; if false, only for Normal layers.</param>
        /// <returns>True if the chunk is a CellChunk and matches the desired layer type; otherwise, false.</returns>
        internal static bool TryCastToCell(this BaseChunk chunk, IReadOnlyList<LayerData> layers, out CellChunk cellChunk, bool desireCustomLayer)
        {
            cellChunk = null;

            // Check if the chunk is a CellChunk.
            if (chunk.chunkType != ChunkTypes.Cell)
            {
                return false;
            }

            // Check if the chunk is a CellChunk.
            if (chunk is not CellChunk c)
            {
                return false;
            }

            // Get the layer index of the CellChunk.
            int layerIndex = c.layerIndex;

            // Check if the layer index is valid.
            if (layerIndex < 0 || layerIndex >= layers.Count)
            {
                return false;
            }

            // Check if the layer is a collision layer.
            bool isCustomLayer = layers[layerIndex].type != LayerType.Normal;

            if (desireCustomLayer != isCustomLayer)
            {
                return false;
            }

            cellChunk = c;
            return true;
        }

        /// <summary>
        /// Attempts to extract the bounding rectangle from a CellChunk.
        /// </summary>
        /// <param name="cellChunk">The CellChunk to extract the rectangle from.</param>
        /// <param name="useAlpha">Whether to use the alpha channel for the color threshold.</param>
        /// <param name="rect">The resulting rectangle if successful, otherwise null.</param>
        internal static bool TryGetRect(this CellChunk cellChunk, bool useAlpha, out RectInt rect)
        {
            rect = new()
            {
                width = 0,
                height = 0,
            };

            s_pixels.Clear();
            s_pixels.AddRange(cellChunk.image);

            bool minExists = TryGetMin(s_pixels, cellChunk.posX, cellChunk.posY, cellChunk.width, cellChunk.height, useAlpha, out Vector2Int min);

            if (!minExists)
            {
                return false;
            }

            _ = TryGetMax(s_pixels, cellChunk.posX, cellChunk.posY, cellChunk.width, cellChunk.height, useAlpha, out Vector2Int max);

            rect = new()
            {
                min = min,
                max = max,
            };

            return true;
        }

        /// <summary>
        /// Attempts to find the minimum point of a collision rectangle in a list of colors.
        /// </summary>
        /// <param name="colors">The list of colors to search through.</param>
        /// <param name="posX">The x-coordinate of the starting position.</param>
        /// <param name="posY">The y-coordinate of the starting position.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="useAlpha">Whether to use the alpha channel for the color threshold.</param>
        /// <param name="value">The resulting minimum point if successful, otherwise null.</param>
        private static bool TryGetMin(IReadOnlyList<Color32> colors, int posX, int posY, int width, int height, bool useAlpha, out Vector2Int value)
        {
            Vector2Int a = default;
            Vector2Int b = default;
            value = default;

            bool shouldBreak = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = (y * width) + x;
                    Color color = colors[i];

                    if ((useAlpha && color.a >= PIXEL_COLOR_MIN) || (!useAlpha && color.r >= PIXEL_COLOR_MIN))
                    {
                        a = new(x, y);
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak)
                {
                    break;
                }
            }

            if (!shouldBreak)
            {
                return false;
            }

            shouldBreak = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = (y * width) + x;
                    Color color = colors[i];

                    if ((useAlpha && color.a >= PIXEL_COLOR_MIN) || (!useAlpha && color.r >= PIXEL_COLOR_MIN))
                    {
                        b = new(x, y);
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak)
                {
                    break;
                }
            }

            value = new()
            {
                x = posX + b.x,
                y = posY + a.y,
            };

            return true;
        }

        /// <summary>
        /// Attempts to find the maximum point of a collision rectangle in a list of colors.
        /// </summary>
        /// <param name="colors">The list of colors to search through.</param>
        /// <param name="posX">The x-coordinate of the starting position.</param>
        /// <param name="posY">The y-coordinate of the starting position.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="useAlpha">Whether to use the alpha channel for the color threshold.</param>
        /// <param name="value">The resulting maximum point if successful, otherwise null.</param>
        private static bool TryGetMax(IReadOnlyList<Color32> colors, int posX, int posY, int width, int height, bool useAlpha, out Vector2Int value)
        {
            Vector2Int a = default;
            Vector2Int b = default;
            value = default;

            bool shouldBreak = false;

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    int i = (y * width) + x;
                    Color color = colors[i];

                    if ((useAlpha && color.a >= PIXEL_COLOR_MIN) || (!useAlpha && color.r >= PIXEL_COLOR_MIN))
                    {
                        a = new(x, y);
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak)
                {
                    break;
                }
            }

            if (!shouldBreak)
            {
                return false;
            }

            shouldBreak = false;

            for (int x = width - 1; x >= 0; x--)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    int i = (y * width) + x;
                    Color color = colors[i];

                    if ((useAlpha && color.a >= PIXEL_COLOR_MIN) || (!useAlpha && color.r >= PIXEL_COLOR_MIN))
                    {
                        b = new(x, y);
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak)
                {
                    break;
                }
            }

            value = new()
            {
                x = posX + b.x + 1,
                y = posY + a.y + 1,
            };

            return true;
        }

        /// <summary>
        /// Returns the pivot point for the imported sprite based on the importer's pivot alignment setting.
        /// </summary>
        /// <param name="args">The import event arguments containing the importer and its settings.</param>
        /// <returns>A Vector2 representing the normalized pivot position.</returns>
        internal static Vector2 GetPivot(this AsepriteImporter.ImportEventArgs args)
        {
            return args.importer.pivotAlignment switch
            {
                SpriteAlignment.BottomLeft => new(0f, 0f),
                SpriteAlignment.BottomCenter => new(0.5f, 0f),
                SpriteAlignment.BottomRight => new(1f, 0f),
                SpriteAlignment.LeftCenter => new(0f, 0.5f),
                SpriteAlignment.Center => new(0.5f, 0.5f),
                SpriteAlignment.RightCenter => new(1f, 0.5f),
                SpriteAlignment.TopLeft => new(0f, 1f),
                SpriteAlignment.TopCenter => new(0.5f, 1f),
                SpriteAlignment.TopRight => new(1f, 1f),
                _ => args.importer.customPivotPosition,
            };
        }
    }
}
