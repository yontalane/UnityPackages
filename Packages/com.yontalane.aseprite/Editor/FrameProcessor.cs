using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yontalane.Aseprite;

namespace YontalaneEditor.Aseprite
{
    /// <summary>
    /// Provides methods for processing individual frames of an Aseprite file, including generating collision data,
    /// points, and root motion.
    /// </summary>
    internal static class FrameProcessor
    {
        /// <summary>
        /// Stores root motion data for a single frame, including position and animation name.
        /// </summary>
        private struct RootData
        {
            /// <summary>
            /// The position of the root motion for this frame.
            /// </summary>
            public Vector2 position;

            /// <summary>
            /// The name of the animation associated with this root motion data.
            /// </summary>
            public string animation;
        }

        private static RootData s_previousRoot = new();

        /// <summary>
        /// Generates animation curves for a collision or trigger layer, setting the BoxCollider2D's enabled state and
        /// position/size for the specified animation frame range. Returns true if the layer is of type Collision or Trigger
        /// and curves are generated.
        /// </summary>
        /// <param name="frameData">The data describing the layer and frame.</param>
        /// <param name="spriteObjectInfo">The list of <see cref="SpriteObjectInfo"/> to be populated with information about sprite-attached objects (such as points) during processing.</param>
        /// <returns>True if the layer is of type Collision or Trigger and curves are generated; otherwise, false.</returns>
        internal static bool TryGenerateCollisionBoxes(this ImportFileData fileData, ImportFrameData frameData, ref List<SpriteObjectInfo> spriteObjectInfo)
        {
            // Only process if the layer is of type Collision or Trigger
            if (frameData.layerData.type != LayerType.Collision && frameData.layerData.type != LayerType.Trigger)
            {
                return false;
            }

            // Extract current and next frame indices and their corresponding times for animation keying.
            int currIndex = frameData.frameIndex;
            int nextIndex = frameData.nextFrameIndex;
            float currTime = frameData.time;
            float nextTime = frameData.nextTime;

            // Set the enabled state of the BoxCollider2D to on for the current frame only
            {
                // Get the binding path for the current layer and enable the BoxCollider2D at the current frame time
                string path = fileData.GetBindingPath(frameData.layerData.name);
                frameData.clip.SetEnabledKey<BoxCollider2D>(path, currTime, true);

                // Add information about this collider or trigger to the spriteObjectInfo list for animation export
                spriteObjectInfo.AddInfo(frameData.layerData.name, frameData.layerData.type == LayerType.Collision ? SpriteObjectType.Collider : SpriteObjectType.Trigger, frameData.animationData.name, frameData.animationData.toFrame - frameData.animationData.fromFrame + 1, frameData.clip.length, currIndex - frameData.animationData.fromFrame + 1, currTime);

                // If there is a next frame and it doesn't already have a key, set the BoxCollider2D to disabled at the next frame time
                if (nextIndex > currIndex && !frameData.clip.HasEnabledKey<BoxCollider2D>(path, nextTime))
                {
                    frameData.clip.SetEnabledKey<BoxCollider2D>(path, nextTime, false);
                }
            }

            // Set the center of the BoxCollider2D
            {
                Vector2 offset = Vector2.Lerp(frameData.cellRect.min, frameData.cellRect.max, 0.5f);

                offset.x -= frameData.fileDimensions.x * frameData.filePivot.x;
                offset.x /= frameData.pixelsPerUnit;

                offset.y = frameData.fileDimensions.y - offset.y;
                offset.y -= frameData.fileDimensions.y * frameData.filePivot.y;
                offset.y /= frameData.pixelsPerUnit;

                string path = fileData.GetBindingPath(frameData.layerData.name);
                frameData.clip.SetOffsetKey(path, currTime, offset, true);
            }

            // Set the size of the BoxCollider2D
            {
                Vector2 size = frameData.cellRect.size;
                size /= frameData.pixelsPerUnit;

                string path = fileData.GetBindingPath(frameData.layerData.name);
                frameData.clip.SetSizeKey(path, currTime, size, true);
            }

            return true;
        }

        /// <summary>
        /// Generates animation curves for a point-type layer, setting the GameObject's active state and position
        /// for the specified animation frame range. Returns true if the layer is of type Point and curves are generated.
        /// </summary>
        /// <param name="frameData">The data describing the layer and frame.</param>
        /// <param name="spriteObjectInfo">The list of <see cref="SpriteObjectInfo"/> to be populated with information about sprite-attached objects (such as points) during processing.</param>
        /// <returns>True if the layer is of type Point and curves are generated; otherwise, false.</returns>
        internal static bool TryGeneratePoint(this ImportFileData fileData, ImportFrameData frameData, ref List<SpriteObjectInfo> spriteObjectInfo)
        {
            // Only process if the layer is of type Point
            if (frameData.layerData.type != LayerType.Point)
            {
                return false;
            }

            // Extract current and next frame indices and their corresponding times for animation keying.
            int currIndex = frameData.frameIndex;
            int nextIndex = frameData.nextFrameIndex;
            float currTime = frameData.time;
            float nextTime = frameData.nextTime;

            // Set the active state of the GameObject to on for the current frame only
            {
                // Get the binding path for the current layer and activate the point object at the current frame time
                string path = fileData.GetBindingPath(frameData.layerData.name);
                frameData.clip.SetIsActiveKey(path, currTime, true);

                // Add point object info for this frame to the spriteObjectInfo list
                spriteObjectInfo.AddInfo(frameData.layerData.name, SpriteObjectType.Point, frameData.animationData.name, frameData.animationData.toFrame - frameData.animationData.fromFrame + 1, frameData.clip.length, currIndex - frameData.animationData.fromFrame + 1, currTime);

                // If there is a next frame, and it doesn't already have a key, deactivate the point object at the next frame time
                if (nextIndex > currIndex && !frameData.clip.HasActiveKey(path, nextTime))
                {
                    frameData.clip.SetIsActiveKey(path, nextTime, false);
                }
            }

            // Set the position of the GameObject
            {
                Vector2 position = Vector2.Lerp(frameData.cellRect.min, frameData.cellRect.max, 0.5f);

                position.x -= frameData.fileDimensions.x * frameData.filePivot.x;
                position.x /= frameData.pixelsPerUnit;

                position.y = frameData.fileDimensions.y - position.y;
                position.y -= frameData.fileDimensions.y * frameData.filePivot.y;
                position.y /= frameData.pixelsPerUnit;

                string path = fileData.GetBindingPath(frameData.layerData.name);
                frameData.clip.SetPositionKey(path, frameData.time, position, true);
            }

            return true;
        }

        /// <summary>
        /// Generates root motion animation curves and events for a RootMotion-type layer.
        /// Calculates the root position and delta for the current frame, sets the root position
        /// in the animation clip, and adds an animation event to apply the root motion at the correct time.
        /// Returns true if the layer is of type RootMotion and root motion is generated; otherwise, false.
        /// </summary>
        /// <param name="fileData">The ImportFileData containing all imported Aseprite data.</param>
        /// <param name="frameData">The data describing the layer and frame.</param>
        /// <returns>True if root motion is generated; otherwise, false.</returns>
        internal static bool TryGenerateRootMotion(this ImportFileData fileData, ImportFrameData frameData)
        {
            // Only process if the layer is of type RootMotion
            if (frameData.layerData.type != LayerType.RootMotion)
            {
                return false;
            }

            // Try to get the sprite for the current frame
            if (!fileData.TryGetSprite(frameData.frameIndex, out Sprite sprite))
            {
                return false;
            }

            // Calculate the center of the cell rectangle in pixel coordinates (from the top-left corner)
            Vector2 rootInPixelsFromCorner = Vector2.Lerp(frameData.cellRect.min, frameData.cellRect.max, 0.5f);

            // Calculate the root point in pixels relative to the Aseprite file's pivot
            Vector2 rootInPixels = new()
            {
                x = rootInPixelsFromCorner.x - frameData.fileDimensions.x * frameData.filePivot.x,
                y = frameData.fileDimensions.y - rootInPixelsFromCorner.y - (frameData.fileDimensions.y * frameData.filePivot.y),
            };

            // Create a new RootData instance to store the root position and animation name for this frame
            RootData root = new()
            {
                position = rootInPixels / frameData.pixelsPerUnit,
                animation = frameData.clip.name,
            };

            // If the animation name has changed, reset the previous root data
            if (root.animation != s_previousRoot.animation)
            {
                s_previousRoot = new();
            }

            // Calculate the root motion delta between the current frame and the previous frame
            Vector2 rootDelta = root.position - s_previousRoot.position;

            // Update the previous root data to the current frame's root data
            s_previousRoot = root;

            // Set the root point in world space relative to the Aseprite file's pivot
            frameData.clip.SetPositionKey(AsepriteAnimationUtility.SpriteBinding.path, frameData.time, -root.position, true);

            // Get the object reference curve for the sprite
            ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(frameData.clip, AsepriteAnimationUtility.SpriteBinding);

            // Add an animation event to the clip that will call the OnAsepriteRootMotion method with the calculated root motion vector as a string parameter at the correct frame time.
            frameData.clip.AddAnimationEvent(new()
            {
                functionName = nameof(AsepriteAnimationBridge.OnAsepriteRootMotion),
                stringParameter = $"{frameData.frameIndex - frameData.animationData.fromFrame},{rootDelta.x},{rootDelta.y}",
                time = frames.GetNearestFrameTime(frameData.time),
            });

            // Return true if the root motion was generated
            return true;
        }
    }
}
