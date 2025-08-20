using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// Provides methods for processing imported Aseprite assets, including generating collision data,
    /// points, and root motion.
    /// </summary>
    internal static class AssetProcesser
    {
        /// <summary>
        /// A reusable list to temporarily store layer data during asset processing.
        /// As we process each layer, we remove it from the list. The remaining layers will need a 'off' key set for the current frame.
        /// </summary>
        private static readonly List<LayerData> s_remainingLayers = new();

        /// <summary>
        /// Processes the imported Aseprite asset by iterating through each frame and relevant chunks,
        /// generating collision data, points, and root motion based on the file's layers and animation clips.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing all extracted data from the Aseprite import.</param>
        internal static void ProcessAsepriteAsset(this ImportFileData fileData)
        {
            // Iterate through the frames to process collision boxes and points before root motion
            for (int frameDataIndex = 0; frameDataIndex < fileData.FrameData.Count; frameDataIndex++)
            {
                // Clear the list of layers and add all layers from the file data
                s_remainingLayers.Clear();
                s_remainingLayers.AddRange(fileData.layers);

                // Process the frame for collision boxes and points
                FrameData data = fileData.FrameData[frameDataIndex];
                fileData.InterateThroughChunks(data, frameDataIndex, s_remainingLayers, false, out float time); // Collision Boxes and Points

                // Set the 'off' key for the remaining layers
                fileData.HideEmptyLayerObjects(frameDataIndex, time, s_remainingLayers);
            }

            // Iterate through the frames to process root motion
            for (int frameDataIndex = 0; frameDataIndex < fileData.FrameData.Count; frameDataIndex++)
            {
                // Process the frame for root motion and collision boxes and points
                FrameData data = fileData.FrameData[frameDataIndex];
                fileData.InterateThroughChunks(data, frameDataIndex, s_remainingLayers, true, out _); // Root Motion
            }

            // Ensure all AnimationClips have their boolean properties initialized to 'off' and add an on start and on complete event
            foreach (Object childObject in fileData.objects)
            {
                if (childObject is AnimationClip clip)
                {
                    clip.EnsureBoolPropertiesBeginOff();
                    clip.AddOnStartEvent();
                    clip.AddOnCompleteEvent();
                }
            }
        }

        /// <summary>
        /// Iterates through each chunk in the specified frame and processes it to generate collision data,
        /// points, or root motion, depending on the provided flag. For each valid chunk, this method calculates
        /// the animation timing, constructs an <see cref="ImportFrameData"/> object, and invokes the appropriate
        /// generation methods for collision, points, or root motion.
        /// </summary>
        /// <param name="fileData">The <see cref="ImportFileData"/> containing all imported Aseprite data.</param>
        /// <param name="data">The <see cref="FrameData"/> for the current frame.</param>
        /// <param name="frameDataIndex">The index of the current frame being processed.</param>
        /// <param name="layers">The list of all layers. As we process each layer, we remove it from the list. The remaining layers will need a 'off' key set for the current frame.</param>
        /// <param name="rootMotion">If true, generates root motion; otherwise, generates collision boxes and points.</param>
        /// <param name="time">The time at which to process the chunk.</param>
        private static void InterateThroughChunks(this ImportFileData fileData, FrameData data, int frameDataIndex, List<LayerData> layers, bool rootMotion, out float time)
        {
            time = 0f;

            // Iterate through each chunk in the current frame to process collision, point, and root motion data
            for (int chunkIndex = 0; chunkIndex < data.chunkCount; chunkIndex++)
            {
                // Skip this chunk if it cannot be processed for the current frame
                if (!fileData.ProcessChunkForFrame(data, chunkIndex, frameDataIndex, out RectInt rect, out AnimationData animationData, out LayerData layerData, out AnimationClip clip))
                {
                    continue;
                }

                // Remove the layer from the list if it is the same type and name as the current layer
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    if (layers[i].type != layerData.type || layers[i].name != layerData.name)
                    {
                        continue;
                    }

                    layers.RemoveAt(i);
                }

                // Get the object reference curve for the current animation clip
                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, AsepriteAnimationUtility.SpriteBinding);

                // Calculate the time in and time out for the current frame/layer/animation
                time = (frameDataIndex - animationData.fromFrame) / (float)(animationData.toFrame - animationData.fromFrame) * clip.length - clip.length / (animationData.toFrame - animationData.fromFrame) * 0.5f;
                time = frames.GetNearestFrameTime(time);

                // Create ImportFrameData for this frame/layer/animation
                ImportFrameData frameData = new()
                {
                    args = fileData.args,
                    layerData = layerData,
                    clip = clip,
                    time = time,
                    fileDimensions = fileData.Size,
                    filePivot = fileData.args.GetPivot(),
                    frameIndex = frameDataIndex,
                    animationData = animationData,
                    cellRect = rect,
                    frameRect = fileData.frameRects[frameDataIndex],
                    pixelsPerUnit = fileData.args.importer.spritePixelsPerUnit,
                };

                // Generate collision data, points, and root motion for the frame/layer/animation
                if (rootMotion)
                {
                    _ = fileData.TryGenerateRootMotion(frameData);
                }
                else
                {
                    _ = fileData.TryGenerateCollisionBoxes(frameData);
                    _ = fileData.TryGeneratePoint(frameData);
                }
            }
        }


        /// <summary>
        /// Processes a chunk for a frame, returning the cell chunk, rectangle, animation data, layer data, and animation clip.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing all extracted data from the Aseprite import.</param>
        /// <param name="data">The FrameData containing the frame data.</param>
        /// <param name="chunkIndex">The index of the chunk to process.</param>
        /// <param name="frameDataIndex">The index of the frame to process.</param>
        /// <param name="rect">The rectangle of the cell chunk.</param>
        /// <param name="animationData">The animation data for the current frame.</param>
        /// <param name="layerData">The layer data for the current layer.</param>
        /// <param name="clip">The animation clip for the current animation.</param>
        /// <returns>True if the chunk is processed successfully; otherwise, false.</returns>
        private static bool ProcessChunkForFrame(this ImportFileData fileData, FrameData data, int chunkIndex, int frameDataIndex, out RectInt rect, out AnimationData animationData, out LayerData layerData, out AnimationClip clip)
        {
            rect = default;
            animationData = default;
            layerData = default;
            clip = null;

            // Try to cast the chunk to a CellChunk and check if it belongs to a custom layer
            if (!data.chunks[chunkIndex].TryCastToCell(fileData.layers, out CellChunk cellChunk, true))
            {
                return false;
            }

            // Try to get the rectangle of the cell chunk
            if (!cellChunk.TryGetRect(false, out rect))
            {
                return false;
            }

            // Try to get the animation data for the current frame
            if (!fileData.TryGetAnimationForFrame(frameDataIndex, out animationData))
            {
                return false;
            }

            // Get the layer data for the current layer
            layerData = fileData.layers[cellChunk.layerIndex];

            // Try to get the animation clip for the current animation
            if (!fileData.TryGetAnimationClip(animationData.name, out clip))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hides objects in the specified layers for a given frame and time by disabling their associated components or GameObjects.
        /// For collision and trigger layers, disables the BoxCollider2D; for point layers, deactivates the GameObject.
        /// </summary>
        /// <param name="fileData">The imported Aseprite file data.</param>
        /// <param name="frameDataIndex">The index of the frame to process.</param>
        /// <param name="time">The time at which to hide the objects.</param>
        /// <param name="layers">The list of layers to process.</param>
        private static void HideEmptyLayerObjects(this ImportFileData fileData, int frameDataIndex, float time, List<LayerData> layers)
        {
            // Try to get the animation data for the specified frame; if unsuccessful, return early.
            if (!fileData.TryGetAnimationForFrame(frameDataIndex, out AnimationData animationData))
            {
                return;
            }

            // Try to get the animation clip for the specified animation; if unsuccessful, return early.
            if (!fileData.TryGetAnimationClip(animationData.name, out AnimationClip clip))
            {
                return;
            }

            // Iterate through each layer to process
            foreach (LayerData layer in layers)
            {
                // Get the binding path for the current layer
                string path = fileData.GetBindingPath(layer.name);

                // Determine the type of layer and set the appropriate property in the animation clip
                switch (layer.type)
                {
                    case LayerType.Collision:
                    case LayerType.Trigger:
                        {
                            clip.SetEnabledKey<BoxCollider2D>(path, time, false);
                            break;
                        }

                    case LayerType.Point:
                        {
                            clip.SetIsActiveKey(path, time, false);
                            break;
                        }
                }
            }
        }
    }
}
