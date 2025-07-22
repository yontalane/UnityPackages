using UnityEditor;
using UnityEditor.U2D.Aseprite;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// CustomAsepriteImporter is an AssetPostprocessor that extends the Unity import pipeline for Aseprite files.
    /// It hooks into the Aseprite import process to extract and process additional data such as collision layers,
    /// animation data, and root motion from imported Aseprite assets, enabling advanced 2D workflows.
    /// </summary>
    public class CustomAsepriteImporter : AssetPostprocessor
    {
        private ImportFileData m_fileData;

        /// <summary>
        /// Registers the OnPostAsepriteImport event handler when an Aseprite file is imported.
        /// </summary>
        void OnPreprocessAsset()
        {
            if (assetImporter is AsepriteImporter aseImporter)
            {
                aseImporter.OnPostAsepriteImport += OnPostAsepriteImport;
            }
        }

        /// <summary>
        /// Processes the Aseprite file after import, preparing the file data and processing the Aseprite asset.
        /// </summary>
        /// <param name="args">The import event arguments containing the Aseprite file and import context.</param>
        private void OnPostAsepriteImport(AsepriteImporter.ImportEventArgs args)
        {
            args.PrepareFileData(ref m_fileData);
            m_fileData.ProcessAsepriteAsset();
        }
    }
}