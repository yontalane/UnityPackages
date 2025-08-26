#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Yontalane.LayoutTilemap
{
    /// <summary>
    /// Stores and manages settings for the LayoutTilemap system, including default and special case map entity data.
    /// </summary>
    [FilePath("ProjectSettings/LayoutTilemapSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class LayoutTilemapSettings : ScriptableSingleton<LayoutTilemapSettings>
    {
        [Tooltip("The default visual and pointer configuration data for map entities.")]
        public MapEntityData defaultData;

        [Tooltip("An array of special-case map entity data, each associated with a specific name.")]
        public NamedMapEntityData[] specialCaseData;

        public LayoutTilemapSettings()
        {
            defaultData = new MapEntityData()
            {
                scale = 0.75f,
                outerColor = Color.gray,
                innerColor = Color.white,
                pointerLength = 1.5f,
                pointerScale = 0.5f,
                thickness = 2
            };

            specialCaseData = new NamedMapEntityData[0];
        }

        /// <summary>
        /// Saves the current LayoutTilemapSettings asset to disk.
        /// </summary>
        public void Save()
        {
            Save(true);
        }

        /// <summary>
        /// Retrieves the MapEntityData associated with the specified name.
        /// If no special-case data matches, returns the default data.
        /// </summary>
        public MapEntityData GetData(string name)
        {
            foreach (NamedMapEntityData namedData in specialCaseData)
            {
                if (namedData.name.Contains(name))
                {
                    return namedData.data;
                }
            }
            return defaultData;
        }
    }
}
#endif