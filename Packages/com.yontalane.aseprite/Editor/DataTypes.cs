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
        public string name;
        public int fromFrame;
        public int toFrame;
    }

    /// <summary>
    /// Represents data for a layer in an Aseprite file, including its name and type (such as Normal, Collision, Trigger, Point, or RootMotion).
    /// </summary>
    internal struct LayerData
    {
        public string name;
        public LayerType type;
    }

    /// <summary>
    /// Holds all necessary data for importing an Aseprite file, including the file's dimensions,
    /// main object, and layer and animation data.
    /// </summary>
    internal class ImportFileData
    {
        public AsepriteImporter.ImportEventArgs args;
        public readonly List<LayerData> layers = new();
        public readonly List<RectInt> frameRects = new();
        public readonly List<Object> objects = new();
        public readonly List<AnimationData> animations = new();
        public GameObject rootObject;
        public Animator animator;
        public SpriteRenderer spriteObject;

        public IReadOnlyList<FrameData> FrameData => args.importer.asepriteFile.frameData;

        public Vector2Int Size => new(args.importer.asepriteFile.width, args.importer.asepriteFile.height);

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
        public AsepriteImporter.ImportEventArgs args;
        public LayerData layerData;
        public AnimationClip clip;
        public float timeIn;
        public float timeOut;
        public Vector2Int fileDimensions;
        public int frameIndex;
        public Vector2 filePivot;
        public RectInt cellRect;
        public RectInt frameRect;
        public float pixelsPerUnit;
    }
}
