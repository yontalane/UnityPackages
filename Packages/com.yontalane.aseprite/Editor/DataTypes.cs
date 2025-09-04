using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// Specifies the type of a layer in an Aseprite file, such as Normal, Collision, Trigger, Point, or RootMotion.
    /// </summary>
    internal enum LayerType
    {
        Normal = 0,
        Collision = 10,
        Trigger = 20,
        Point = 30,
        RootMotion = 40,
    }

    /// <summary>
    /// Represents animation tag data, including the animation's name and the range of frames it covers.
    /// </summary>
    internal struct AnimationData
    {
        /// <summary>
        /// The name of the animation tag.
        /// </summary>
        public string name;

        /// <summary>
        /// The index of the first frame included in this animation.
        /// </summary>
        public int fromFrame;

        /// <summary>
        /// The index of the last frame included in this animation.
        /// </summary>
        public int toFrame;
    }

    /// <summary>
    /// Represents data for a layer in an Aseprite file, including its name and type (such as Normal, Collision, Trigger, Point, or RootMotion).
    /// </summary>
    internal struct LayerData
    {
        /// <summary>
        /// The name of the layer.
        /// </summary>
        public string name;

        /// <summary>
        /// The type of the layer (e.g., Normal, Collision, Trigger, Point, RootMotion).
        /// </summary>
        public LayerType type;
    }

    /// <summary>
    /// Holds all necessary data for importing an Aseprite file, including the file's dimensions,
    /// main object, and layer and animation data.
    /// </summary>
    internal class ImportFileData
    {
        /// <summary>
        /// The import event arguments containing context and importer references.
        /// </summary>
        public AsepriteImporter.ImportEventArgs args;

        /// <summary>
        /// The list of layer data parsed from the Aseprite file.
        /// </summary>
        public readonly List<LayerData> layers = new();

        /// <summary>
        /// The list of rectangles representing the bounds of each frame in the sprite sheet.
        /// </summary>
        public readonly List<RectInt> frameRects = new();

        /// <summary>
        /// The list of Unity objects created or referenced during import.
        /// </summary>
        public readonly List<Object> objects = new();

        /// <summary>
        /// The list of animation data tags defined in the Aseprite file.
        /// </summary>
        public readonly List<AnimationData> animations = new();

        /// <summary>
        /// The root GameObject created for the imported asset.
        /// </summary>
        public GameObject rootObject;

        /// <summary>
        /// The Animator component associated with the imported asset.
        /// </summary>
        public Animator animator;

        /// <summary>
        /// The SpriteRenderer component associated with the imported asset.
        /// </summary>
        public SpriteRenderer spriteObject;

        /// <summary>
        /// The read-only list of frame data from the imported Aseprite file.
        /// </summary>
        public IReadOnlyList<FrameData> FrameData => args.importer.asepriteFile.frameData;

        /// <summary>
        /// The width and height of the imported Aseprite file.
        /// </summary>
        public Vector2Int Size => new(args.importer.asepriteFile.width, args.importer.asepriteFile.height);

        /// <summary>
        /// The main Unity object created or referenced during import.
        /// </summary>
        public Object MainObject => args.context.mainObject;

        public ImportFileData(AsepriteImporter.ImportEventArgs args)
        {
            this.args = args;
        }

        public ImportFileData()
        {

        }
    }

    /// <summary>
    /// Holds all necessary data for generating import-related animation curves and collision information
    /// for a specific layer and animation frame during the Aseprite import process.
    /// </summary>
    internal struct ImportFrameData
    {
        /// <summary>
        /// The import event arguments containing context and file information.
        /// </summary>
        public AsepriteImporter.ImportEventArgs args;

        /// <summary>
        /// The data for the current layer being processed.
        /// </summary>
        public LayerData layerData;

        /// <summary>
        /// The animation clip being generated or modified for this frame.
        /// </summary>
        public AnimationClip clip;

        /// <summary>
        /// The time (in seconds) of the current frame within the animation.
        /// </summary>
        public float time;

        /// <summary>
        /// The time (in seconds) of the previous frame within the animation.
        /// </summary>
        public float previousTime;

        /// <summary>
        /// The time (in seconds) of the next frame within the animation.
        /// </summary>
        public float nextTime;

        /// <summary>
        /// The width and height of the imported Aseprite file.
        /// </summary>
        public Vector2Int fileDimensions;

        /// <summary>
        /// The index of the current frame within the animation.
        /// </summary>
        public int frameIndex;

        /// <summary>
        /// The index of the previous frame within the animation.
        /// </summary>
        public int previousFrameIndex;

        /// <summary>
        /// The index of the next frame within the animation.
        /// </summary>
        public int nextFrameIndex;

        /// <summary>
        /// The animation data associated with the current animation.
        /// </summary>
        public AnimationData animationData;

        /// <summary>
        /// The pivot point of the file, in pixel coordinates.
        /// </summary>
        public Vector2 filePivot;

        /// <summary>
        /// The rectangle representing the cell area in the sprite sheet for this frame.
        /// </summary>
        public RectInt cellRect;

        /// <summary>
        /// The rectangle representing the frame area in the sprite sheet for this frame.
        /// </summary>
        public RectInt frameRect;

        /// <summary>
        /// The number of pixels per Unity unit for the imported sprites.
        /// </summary>
        public float pixelsPerUnit;
    }
}
