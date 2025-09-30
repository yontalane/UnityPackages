using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using Yontalane.Aseprite;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// Provides methods for building and processing the ImportFileData object during the Aseprite import process.
    /// </summary>
    public static class FileDataBuilder
    {
        private static readonly List<BoxCollider2D> s_colliders = new();
        private static readonly List<BoxCollider2D> s_triggers = new();
        private static readonly List<Transform> s_points = new();

        /// <summary>
        /// Prepares and populates the ImportFileData object with all relevant data extracted from the Aseprite import event arguments.
        /// This includes animation tags, layer information, frame rectangles, asset objects, colliders, points, and root motion data.
        /// </summary>
        /// <param name="args">The import event arguments containing the Aseprite file and import context.</param>
        /// <param name="fileData">The ImportFileData object to be initialized and filled with extracted data.</param>
        internal static void PrepareFileData(this AsepriteImporter.ImportEventArgs args, ref ImportFileData fileData)
        {
            AsepriteFile file = args.importer.asepriteFile;

            // Check if the Aseprite file is null before proceeding
            if (file == null)
            {
                Debug.LogError("File is null.");
                return;
            }

            // Create a new ImportFileData object with the import event arguments
            fileData = new(args);

            // Populate the ImportFileData object with all necessary data and assets extracted from the Aseprite import.
            fileData.GetMainObject();
            fileData.GetAnimations();
            fileData.GetLayerData();
            fileData.GetFrameRects();
            fileData.GetAssetObjects();
            fileData.FixAnimations();
            fileData.MakeColliders();
            fileData.MakePoints();
            fileData.AddAnimationBridge();
        }

        /// <summary>
        /// Initializes the main GameObject and SpriteRenderer for the imported Aseprite file,
        /// sets up the root object, assigns the SpriteRenderer, and configures the Animator if present.
        /// </summary>
        internal static void GetMainObject(this ImportFileData fileData)
        {
            // Ensure the main object in the import context is a GameObject before proceeding
            if (fileData.args.context.mainObject is not GameObject mainObject)
            {
                return;
            }

            // Ensure the main object has a SpriteRenderer component before proceeding
            if (!mainObject.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                return;
            }

            // Store the SpriteRenderer component for later use
            fileData.spriteObject = spriteRenderer;

            // Create a new root GameObject for the imported Aseprite file
            string name = mainObject.name;
            GameObject rootObject = new();

            // Add the root object to the import context as an asset and set it as the main object
            fileData.args.context.AddObjectToAsset(rootObject.name, rootObject);
            fileData.args.context.SetMainObject(rootObject);

            // Store the root object for later use
            fileData.rootObject = rootObject;

            // Set the SpriteRenderer as a child of the root object and name it "Sprite"
            spriteRenderer.transform.SetParent(rootObject.transform);

            // Give the root object the original mainObject's name and the SpriteRenderer the name "Sprite"
            spriteRenderer.name = "Sprite";
            rootObject.name = name;

            // If the SpriteRenderer has an Animator component, then pass its AnimatorController to a new Animator component on the root object
            if (spriteRenderer.TryGetComponent(out Animator originalAnimator))
            {
                fileData.animator = rootObject.AddComponent<Animator>();
                fileData.animator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
                originalAnimator.runtimeAnimatorController = null;
                originalAnimator.enabled = false;
            }
        }

        /// <summary>
        /// Extracts animation tag data from the provided frame data and populates the m_animations list.
        /// Each animation is defined by its name and the range of frames it covers.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the extracted animation data.</param>
        internal static void GetAnimations(this ImportFileData fileData)
        {
            fileData.animations.Clear();

            // Iterate through each frame in the Aseprite file
            for (int frameDataIndex = 0; frameDataIndex < fileData.FrameData.Count; frameDataIndex++)
            {
                FrameData data = fileData.FrameData[frameDataIndex];

                // Iterate through each chunk in the frame
                for (int chunkIndex = 0; chunkIndex < data.chunkCount; chunkIndex++)
                {
                    BaseChunk chunk = data.chunks[chunkIndex];

                    // If the chunk is not a tags chunk, skip it
                    if (chunk.chunkType != ChunkTypes.Tags)
                    {
                        continue;
                    }

                    // If the chunk is not a tags chunk, skip it
                    if (chunk is not TagsChunk tagsChunk)
                    {
                        continue;
                    }

                    // Iterate through each tag in the tags chunk
                    foreach (TagData tagData in tagsChunk.tagData)
                    {
                        // Create a new animation data object, populate it with the tag data, and add it to the animations list
                        AnimationData animationData = new()
                        {
                            name = tagData.name,
                            fromFrame = tagData.fromFrame,
                            toFrame = tagData.toFrame,
                        };

                        fileData.animations.Add(animationData);
                    }
                }
            }
        }

        /// <summary>
        /// Fixes the animations by setting the SpriteRenderer's sprite to the correct sprite for each animation.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the fixed animations.</param>
        internal static void FixAnimations(this ImportFileData fileData)
        {
            // Iterate through each animation in the animations list
            foreach(AnimationData animationData in fileData.animations)
            {
                // If the animation clip is not found, skip it
                if (!fileData.TryGetAnimationClip(animationData.name, out AnimationClip clip))
                {
                    continue;
                }

                // Create a new editor curve binding for the sprite property
                EditorCurveBinding originalBinding = new()
                {
                    path = string.Empty,
                    propertyName = "m_Sprite",
                    type = typeof(SpriteRenderer),
                };

                // Create a new editor curve binding for the enabled property
                EditorCurveBinding originalEnabledBinding = new()
                {
                    path = string.Empty,
                    propertyName = "m_Enabled",
                    type = typeof(SpriteRenderer),
                };

                // For all curves bound to the main object sprite property, set them instead to the child object sprite property
                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, originalBinding);
                AnimationUtility.SetObjectReferenceCurve(clip, originalBinding, null);
                AnimationUtility.SetObjectReferenceCurve(clip, AsepriteAnimationUtility.SpriteBinding, frames);

                // For all curves that enable or disable the sprite renderer on the main object, set them instead to the child object
                AnimationCurve enabledFrames = AnimationUtility.GetEditorCurve(clip, originalEnabledBinding);
                if (enabledFrames != null)
                {
                    AnimationUtility.SetEditorCurve(clip, originalEnabledBinding, null);
                    AnimationUtility.SetEditorCurve(clip, AsepriteAnimationUtility.SpriteEnabledBinding, enabledFrames);
                }
            }
        }

        /// <summary>
        /// Identifies and records the names of collision layers from the provided frame data.
        /// For each layer in each frame, adds the layer name to m_layerCollision if it contains "collision" (case-insensitive),
        /// otherwise adds an empty string.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the extracted layer data.</param>
        internal static void GetLayerData(this ImportFileData fileData)
        {
            fileData.layers.Clear();

            // Iterate through each frame in the Aseprite file
            for (int frameDataIndex = 0; frameDataIndex < fileData.FrameData.Count; frameDataIndex++)
            {
                FrameData data = fileData.FrameData[frameDataIndex];

                // Iterate through each chunk in the frame
                for (int chunkIndex = 0; chunkIndex < data.chunkCount; chunkIndex++)
                {
                    BaseChunk chunk = data.chunks[chunkIndex];

                    // If the chunk is not a layer chunk, skip it
                    if (chunk.chunkType != ChunkTypes.Layer)
                    {
                        continue;
                    }

                    // If the chunk is not a layer chunk, skip it
                    if (chunk is not LayerChunk layerChunk)
                    {
                        continue;
                    }

                    // Get the name of the layer
                    string layerName = layerChunk.name;
                    string layerNameLower = layerName.ToLower();

                    // Determine the type of the layer
                    LayerType type;
                    if (layerNameLower.Contains("trigger"))
                    {
                        type = LayerType.Trigger;
                    }
                    else if (layerNameLower.Contains("collision"))
                    {
                        type = LayerType.Collision;
                    }
                    else if (layerNameLower.Contains("point"))
                    {
                        type = LayerType.Point;
                    }
                    else if (layerNameLower.Contains("root"))
                    {
                        type = LayerType.RootMotion;
                    }
                    else
                    {
                        type = LayerType.Normal;
                    }

                    // Create a new layer data object, populate it with the layer name and type, and add it to the layers list
                    LayerData layerData = new()
                    {
                        name = layerName,
                        type = type,
                    };

                    fileData.layers.Add(layerData);
                }
            }
        }

        /// <summary>
        /// Extracts the bounding rectangles for each frame from the provided frame data and populates the m_frameRects list.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the extracted frame rectangles.</param>
        internal static void GetFrameRects(this ImportFileData fileData)
        {
            fileData.frameRects.Clear();

            // Iterate through each frame in the Aseprite file
            for (int frameDataIndex = 0; frameDataIndex < fileData.FrameData.Count; frameDataIndex++)
            {
                FrameData data = fileData.FrameData[frameDataIndex];

                // Initialize the most min and most max vectors
                Vector2Int mostMin = new(int.MaxValue, int.MaxValue);
                Vector2Int mostMax = new(int.MinValue, int.MinValue);

                // Initialize the rect
                RectInt rect = new()
                {
                    width = 0,
                    height = 0,
                };

                // Iterate through each chunk in the frame
                for (int chunkIndex = 0; chunkIndex < data.chunkCount; chunkIndex++)
                {
                    // If the chunk is not a cell chunk, skip it
                    if (!data.chunks[chunkIndex].TryCastToCell(fileData.layers, out CellChunk cellChunk, false))
                    {
                        continue;
                    }

                    // If the cell chunk does not have a rect, skip it
                    if (!cellChunk.TryGetRect(true, out RectInt r))
                    {
                        continue;
                    }

                    // Update the rect to the smallest and largest values
                    rect.min = Vector2Int.Min(r.min, mostMin);
                    rect.max = Vector2Int.Max(r.max, mostMax);
                }

                // Add the rect to the frame rects list
                fileData.frameRects.Add(rect);
            }
        }

        /// <summary>
        /// Retrieves all asset objects from the import context and stores them in the m_objects list.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the extracted asset objects.</param>
        internal static void GetAssetObjects(this ImportFileData fileData)
        {
            fileData.objects.Clear();

            // Get all asset objects from the import context
            fileData.args.context.GetObjects(fileData.objects);
        }

        /// <summary>
        /// Creates and configures BoxCollider2D components for each collision layer in the import context.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the created colliders.</param>
        internal static void MakeColliders(this ImportFileData fileData)
        {
            s_colliders.Clear();
            s_triggers.Clear();

            // Iterate through each layer in the layers list
            foreach (LayerData layerData in fileData.layers)
            {
                // If the layer is not a collision or trigger layer, skip it
                if (layerData.type != LayerType.Collision && layerData.type != LayerType.Trigger)
                {
                    continue;
                }

                // If the main object is not a game object, skip it (this should never happen)
                if (fileData.MainObject is not GameObject _)
                {
                    return;
                }

                // Create a new GameObject for the collision layer and add a BoxCollider2D component
                GameObject go = new(layerData.name);
                BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
                collider.enabled = false;
                collider.isTrigger = layerData.type == LayerType.Trigger;
                // Add the new GameObject to the import context as an asset
                fileData.args.context.AddObjectToAsset(go.name, go);
                // Set the new GameObject as a child of the main imported object
                go.transform.SetParent(fileData.spriteObject.transform);

                if (layerData.type == LayerType.Collision)
                {
                    s_colliders.Add(collider);
                }
                else
                {
                    s_triggers.Add(collider);
                }
            }
        }

        /// <summary>
        /// Creates GameObjects for each point layer in the import context.
        /// </summary>
        /// <param name="fileData">The ImportFileData object to store the created points.</param>
        internal static void MakePoints(this ImportFileData fileData)
        {
            s_points.Clear();

            // Iterate through each layer in the layers list
            foreach (LayerData layerData in fileData.layers)
            {
                // If the layer is not a point layer, skip it
                if (layerData.type != LayerType.Point)
                {
                    continue;
                }

                // If the main object is not a game object, skip it (this should never happen)
                if (fileData.MainObject is not GameObject _)
                {
                    return;
                }

                // Create a new GameObject to represent the point
                GameObject go = new(layerData.name);
                go.SetActive(false);
                // Add the new GameObject to the import context as an asset
                fileData.args.context.AddObjectToAsset(go.name, go);
                // Set the new GameObject as a child of the main imported object
                go.transform.SetParent(fileData.spriteObject.transform);

                s_points.Add(go.transform);
            }
        }

        /// <summary>
        /// Adds an AsepriteAnimationBridge component to the main imported GameObject and populates it
        /// with references to all colliders, triggers, and points created during the import process.
        /// </summary>
        internal static void AddAnimationBridge(this ImportFileData fileData)
        {
            // If the main object is not a game object, skip it (this should never happen)
            if (fileData.MainObject is not GameObject mainObject)
            {
                return;
            }

            // Add an AsepriteAnimationBridge component to the main object
            AsepriteAnimationBridge bridge = mainObject.AddComponent<AsepriteAnimationBridge>();

            // Ensure the lists on the bridge are initialized if they are null
            bridge.Colliders ??= new();
            bridge.Triggers ??= new();
            bridge.Points ??= new();

            // Add all colliders, triggers, and points collected during import to the bridge
            bridge.Colliders.AddRange(s_colliders);
            bridge.Triggers.AddRange(s_triggers);
            bridge.Points.AddRange(s_points);
        }

        /// <summary>
        /// Adds or updates the SpriteObjectInfo list on the AsepriteAnimationBridge component attached to the main imported GameObject.
        /// This method ensures the bridge's SpriteObjectInfo list is initialized, cleared, and populated with the provided info.
        /// </summary>
        /// <param name="fileData">The import file data containing the main GameObject.</param>
        /// <param name="info">The list of SpriteObjectInfo to assign to the bridge.</param>
        internal static void AddSpriteObjectInfo(ref ImportFileData fileData, IReadOnlyList<SpriteObjectInfo> info)
        {
            // Attempt to cast the main object to a GameObject; if not possible, exit early.
            if (fileData.MainObject is not GameObject mainObject)
            {
                return;
            }

            // Try to get the AsepriteAnimationBridge component from the main GameObject; if not found, exit early.
            if (!mainObject.TryGetComponent(out AsepriteAnimationBridge bridge))
            {
                return;
            }

            // Ensure the SpriteObjectInfo list is initialized.
            bridge.SpriteObjectInfo ??= new();

            // Clear any existing data in the SpriteObjectInfo list.
            bridge.SpriteObjectInfo.Clear();

            // Add the provided info to the SpriteObjectInfo list.
            bridge.SpriteObjectInfo.AddRange(info);
        }
    }
}
