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
        /// Processes the imported Aseprite asset by iterating through each frame and relevant chunks,
        /// generating collision data, points, and root motion based on the file's layers and animation clips.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing all extracted data from the Aseprite import.</param>
        internal static void ProcessAsepriteAsset(this ImportFileData fileData)
        {
            // Initialize the frame processor to reset previous root point and clear collision rectangles before processing frames
            FrameProcessor.Initialize();
            
            // Iterate backwards through the frames to process collision boxes and points before root motion
            for (int frameDataIndex = fileData.FrameData.Count - 1; frameDataIndex >= 0; frameDataIndex--)
            {
                // Process the frame for root motion and collision boxes and points
                FrameData data = fileData.FrameData[frameDataIndex];
                fileData.InterateThroughChunks(data, frameDataIndex, false); // Collision Boxes and Points
            }

            // Iterate through the frames to process root motion
            for (int frameDataIndex = 0; frameDataIndex < fileData.FrameData.Count; frameDataIndex++)
            {
                // Process the frame for root motion and collision boxes and points
                FrameData data = fileData.FrameData[frameDataIndex];
                fileData.InterateThroughChunks(data, frameDataIndex, true); // Root Motion
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
        /// <param name="rootMotion">If true, generates root motion; otherwise, generates collision boxes and points.</param>
        private static void InterateThroughChunks(this ImportFileData fileData, FrameData data, int frameDataIndex, bool rootMotion)
        {
            // Iterate through each chunk in the current frame to process collision, point, and root motion data
            for (int chunkIndex = 0; chunkIndex < data.chunkCount; chunkIndex++)
            {
                if (!fileData.ProcessChunkForFrame(data, chunkIndex, frameDataIndex, out RectInt rect, out AnimationData animationData, out LayerData layerData, out AnimationClip clip))
                {
                    continue;
                }

                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, AnimationUtilities.SpriteBinding);

                // Calculate the time in and time out for the current frame/layer/animation
                float timeIn = (frameDataIndex - animationData.fromFrame) / (float)(animationData.toFrame - animationData.fromFrame) * clip.length - clip.length / (animationData.toFrame - animationData.fromFrame) * 0.5f;
                timeIn = frames.GetNearestFrameTime(timeIn);
                float timeOut = frames.GetNextNearestFrameTime(timeIn);

                // Create ImportFrameData for this frame/layer/animation
                ImportFrameData frameData = new()
                {
                    args = fileData.args,
                    layerData = layerData,
                    clip = clip,
                    timeIn = timeIn,
                    timeOut = timeOut,
                    fileDimensions = fileData.Size,
                    filePivot = fileData.args.GetPivot(),
                    frameIndex = frameDataIndex,
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
        /// <param name="data">The FrameData containing the frame data.</param>
        /// <param name="chunkIndex">The index of the chunk to process.</param>
        /// <param name="frameDataIndex">The index of the frame to process.</param>
        /// <param name="rect">The rectangle of the cell chunk.</param>
        /// <param name="animationData">The animation data for the current frame.</param>
        /// <param name="layerData">The layer data for the current layer.</param>
        /// <param name="clip">The animation clip for the current animation.</param>
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
    }
}
