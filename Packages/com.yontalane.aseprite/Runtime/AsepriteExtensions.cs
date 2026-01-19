using System.Collections.Generic;

namespace Yontalane.Aseprite
{
    public static class AsepriteExtensions
    {
        /// <summary>
        /// Attempts to find a SpriteObjectInfo in the list by its name.
        /// </summary>
        /// <param name="spriteObjectInfo">The list of SpriteObjectInfo to search.</param>
        /// <param name="name">The name of the object to find.</param>
        /// <param name="result">The found SpriteObjectInfo, or null if not found.</param>
        /// <returns>True if found, otherwise false.</returns>
        public static bool TryGetInfo(this IReadOnlyList<SpriteObjectInfo> spriteObjectInfo, string name, out SpriteObjectInfo result)
        {
            // Iterate through each SpriteObjectInfo in the list
            foreach (SpriteObjectInfo info in spriteObjectInfo)
            {
                // Check if the name matches
                if (info.name == name)
                {
                    // If found, set result and return true
                    result = info;
                    return true;
                }
            }

            // If not found, set result to null and return false
            result = null;
            return false;
        }

        /// <summary>
        /// Gets a SpriteObjectInfo from the list by its name, or null if not found.
        /// </summary>
        /// <param name="spriteObjectInfo">The list of SpriteObjectInfo to search.</param>
        /// <param name="name">The name of the object to find.</param>
        /// <returns>The found SpriteObjectInfo, or null if not found.</returns>
        public static SpriteObjectInfo GetInfo(this IReadOnlyList<SpriteObjectInfo> spriteObjectInfo, string name)
        {
            // Use TryGetInfo to attempt to find the object by name
            _ = spriteObjectInfo.TryGetInfo(name, out SpriteObjectInfo result);
            return result;
        }

        /// <summary>
        /// Gets the length of the animation with the given name, assuming the animation exists.
        /// </summary>
        /// <param name="animationLengths">A list containing the lengths of all the animations.</param>
        /// <param name="name">The name of the animation whose length we want.</param>
        /// <param name="length">The length of the animation.</param>
        /// <returns>Whether the animation exists.</returns>
        public static bool TryGetLength(this IReadOnlyList<AnimationLengthInfo> animationLengths, string name, out AnimationLengthInfo length)
        {
            foreach (AnimationLengthInfo info in animationLengths)
            {
                if (info.name != name)
                {
                    continue;
                }

                length = info;
                return true;
            }
            
            length = default;
            return false;
        }

        /// <summary>
        /// Attempts to find a SpriteObjectAnimationInfo for a specific object and animation name.
        /// </summary>
        /// <param name="spriteObjectInfo">The list of SpriteObjectInfo to search.</param>
        /// <param name="objectName">The name of the object to find.</param>
        /// <param name="animationName">The name of the animation to find.</param>
        /// <param name="result">The found SpriteObjectAnimationInfo, or null if not found.</param>
        /// <returns>True if found, otherwise false.</returns>
        public static bool TryGetAnimationInfo(this IReadOnlyList<SpriteObjectInfo> spriteObjectInfo, string objectName, string animationName, out SpriteObjectAnimationInfo result)
        {
            // Try to get the SpriteObjectInfo for the given object name
            if (!spriteObjectInfo.TryGetInfo(objectName, out SpriteObjectInfo info))
            {
                // If not found, set result to null and return false
                result = null;
                return false;
            }

            // Try to get the animation info from the found object
            return info.TryGetAnimationInfo(animationName, out result);
        }

        /// <summary>
        /// Gets a SpriteObjectAnimationInfo for a specific object and animation name, or null if not found.
        /// </summary>
        /// <param name="spriteObjectInfo">The list of SpriteObjectInfo to search.</param>
        /// <param name="objectName">The name of the object to find.</param>
        /// <param name="animationName">The name of the animation to find.</param>
        /// <returns>The found SpriteObjectAnimationInfo, or null if not found.</returns>
        public static SpriteObjectAnimationInfo GetAnimationInfo(this IReadOnlyList<SpriteObjectInfo> spriteObjectInfo, string objectName, string animationName)
        {
            // Use TryGetAnimationInfo to attempt to find the animation info
            _ = spriteObjectInfo.TryGetAnimationInfo(objectName, animationName, out SpriteObjectAnimationInfo result);
            return result;
        }

        /// <summary>
        /// Attempts to find a SpriteObjectAnimationInfo in the object's animationInfo list by animation name.
        /// </summary>
        /// <param name="spriteObjectInfo">The SpriteObjectInfo to search.</param>
        /// <param name="name">The name of the animation to find.</param>
        /// <param name="result">The found SpriteObjectAnimationInfo, or null if not found.</param>
        /// <returns>True if found, otherwise false.</returns>
        public static bool TryGetAnimationInfo(this SpriteObjectInfo spriteObjectInfo, string name, out SpriteObjectAnimationInfo result)
        {
            // Iterate through each SpriteObjectAnimationInfo in the animationInfo list
            foreach (SpriteObjectAnimationInfo info in spriteObjectInfo.animationInfo)
            {
                // Check if the animation name matches
                if (info.animation == name)
                {
                    // If found, set result and return true
                    result = info;
                    return true;
                }
            }

            // If not found, set result to null and return false
            result = null;
            return false;
        }

        /// <summary>
        /// Gets a SpriteObjectAnimationInfo from the object's animationInfo list by animation name, or null if not found.
        /// </summary>
        /// <param name="spriteObjectInfo">The SpriteObjectInfo to search.</param>
        /// <param name="name">The name of the animation to find.</param>
        /// <returns>The found SpriteObjectAnimationInfo, or null if not found.</returns>
        public static SpriteObjectAnimationInfo GetAnimationInfo(this SpriteObjectInfo spriteObjectInfo, string name)
        {
            // Use TryGetAnimationInfo to attempt to find the animation info
            _ = spriteObjectInfo.TryGetAnimationInfo(name, out SpriteObjectAnimationInfo result);
            return result;
        }
    }
}
