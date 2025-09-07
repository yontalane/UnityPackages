using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.Aseprite
{
    public static class Extensions
    {
        #region Aseprite Animation Extras

        /// <summary>
        /// Checks if any of the provided extras contains a motion tree with the specified identifier.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="id">The identifier to check for.</param>
        /// <returns>True if any extra contains the motion tree; otherwise, false.</returns>
        public static bool HasMotionTree(this IReadOnlyList<AsepriteAnimationExtras> extras, string id)
        {
            if (extras == null)
            {
                return false;
            }

            foreach (AsepriteAnimationExtras extra in extras)
            {
                if (extra.HasMotionTree(id))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to get a motion tree by its identifier from a list of extras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <param name="motionTree">The found motion tree, if any.</param>
        /// <returns>True if a motion tree was found; otherwise, false.</returns>
        public static bool TryGetMotionTree(this IReadOnlyList<AsepriteAnimationExtras> extras, string id, out MotionTree motionTree)
        {
            if (extras == null)
            {
                motionTree = default;
                return false;
            }

            foreach (AsepriteAnimationExtras extra in extras)
            {
                if (extra.TryGetMotionTree(id, out motionTree))
                {
                    return true;
                }
            }

            motionTree = default;
            return false;
        }

        /// <summary>
        /// Retrieves a motion tree with the specified identifier from a list of AsepriteAnimationExtras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="id">The identifier of the motion tree to retrieve.</param>
        /// <returns>The found MotionTree if present; otherwise, the default MotionTree value.</returns>
        public static MotionTree GetMotionTree(this IReadOnlyList<AsepriteAnimationExtras> extras, string id)
        {
            if (extras == null)
            {
                return default;
            }

            _ = extras.TryGetMotionTree(id, out MotionTree motionTree);
            return motionTree;
        }

        /// <summary>
        /// Gets the value associated with a motion tree identifier from a list of extras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <returns>The float value associated with the motion tree, or default if not found.</returns>
        public static float GetMotionTreeValue(this IReadOnlyList<AsepriteAnimationExtras> extras, string id)
        {
            if (extras == null)
            {
                return default;
            }

            foreach (AsepriteAnimationExtras extra in extras)
            {
                if (extra.TryGetMotionTree(id, out MotionTree motionTree))
                {
                    return extra.GetMotionTreeValue(motionTree);
                }
            }

            return default;
        }

        /// <summary>
        /// Gets the value associated with a given motion tree from a list of extras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="motionTree">The motion tree to get the value for.</param>
        /// <returns>The float value associated with the motion tree.</returns>
        public static float GetMotionTreeValue(this IReadOnlyList<AsepriteAnimationExtras> extras, MotionTree motionTree)
        {
            if (extras == null)
            {
                return default;
            }

            return extras.GetMotionTreeValue(motionTree.id);
        }

        /// <summary>
        /// Attempts to get the animation name for the given motion tree from a list of AsepriteAnimationExtras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="motionTree">The motion tree to evaluate.</param>
        /// <param name="animation">The resulting animation name, if found.</param>
        /// <returns>True if a valid animation was found; otherwise, false.</returns>
        public static bool TryGetAnimation(this IReadOnlyList<AsepriteAnimationExtras> extras, MotionTree motionTree, out string animation, out float time)
        {
            if (extras == null)
            {
                animation = default;
                time = default;
                return false;
            }

            foreach(AsepriteAnimationExtras extra in extras)
            {
                if (extra.TryGetAnimation(motionTree, out animation, out time))
                {
                    return true;
                }
            }

            animation = default;
            time = default;
            return false;
        }

        /// <summary>
        /// Attempts to get the animation name for the motion tree with the specified identifier from a list of AsepriteAnimationExtras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <param name="animation">The resulting animation name, if found.</param>
        /// <returns>True if a valid animation was found; otherwise, false.</returns>
        public static bool TryGetAnimation(this IReadOnlyList<AsepriteAnimationExtras> extras, string id, out string animation, out float time)
        {
            if (extras == null)
            {
                animation = default;
                time = default;
                return false;
            }

            foreach (AsepriteAnimationExtras extra in extras)
            {
                if (extra.TryGetAnimation(id, out animation, out time))
                {
                    return true;
                }
            }

            animation = default;
            time = default;
            return false;
        }

        /// <summary>
        /// Gets the animation name for the given motion tree from a list of AsepriteAnimationExtras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="motionTree">The motion tree to evaluate.</param>
        /// <returns>The animation name, or default if not found.</returns>
        public static string GetAnimation(this IReadOnlyList<AsepriteAnimationExtras> extras, MotionTree motionTree, out float time)
        {
            if (extras == null)
            {
                time = default;
                return default;
            }

            _ = extras.TryGetAnimation(motionTree, out string animation, out time);
            return animation;
        }

        public static string GetAnimation(this IReadOnlyList<AsepriteAnimationExtras> extras, MotionTree motionTree)
        {
            return extras.GetAnimation(motionTree, out _);
        }

        /// <summary>
        /// Gets the animation name for the motion tree with the specified identifier from a list of AsepriteAnimationExtras.
        /// </summary>
        /// <param name="extras">A list of AsepriteAnimationExtras to search.</param>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <returns>The animation name, or default if not found.</returns>
        public static string GetAnimation(this IReadOnlyList<AsepriteAnimationExtras> extras, string id, out float time)
        {
            if (extras == null)
            {
                time = default;
                return default;
            }

            _ = extras.TryGetAnimation(id, out string animation, out time);
            return animation;
        }

        public static string GetAnimation(this IReadOnlyList<AsepriteAnimationExtras> extras, string id)
        {
            return extras.GetAnimation(id, out _);
        }

        #endregion

        /// <summary>
        /// Gets the minimum time value among all keys in the AnimationCurve.
        /// </summary>
        /// <param name="curve">The AnimationCurve to evaluate.</param>
        /// <returns>The minimum time value, or 0 if the curve is null or empty.</returns>
        public static float GetMinTime(this AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return 0f;
            }

            float val = float.MaxValue;

            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (curve.keys[i].time >= val)
                {
                    continue;
                }

                val = curve.keys[i].time;
            }

            return val;
        }

        /// <summary>
        /// Gets the maximum time value among all keys in the AnimationCurve.
        /// </summary>
        /// <param name="curve">The AnimationCurve to evaluate.</param>
        /// <returns>The maximum time value, or 0 if the curve is null or empty.</returns>
        public static float GetMaxTime(this AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return 0f;
            }

            float val = float.MinValue;

            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (curve.keys[i].time <= val)
                {
                    continue;
                }

                val = curve.keys[i].time;
            }

            return val;
        }

        /// <summary>
        /// Normalizes a given time value to a 0-1 range based on the minimum and maximum time values of the AnimationCurve.
        /// </summary>
        /// <param name="curve">The AnimationCurve to evaluate.</param>
        /// <param name="time">The time value to normalize.</param>
        /// <returns>The normalized time value in the range [0, 1], or 0 if the curve is null or empty.</returns>
        public static float GetNormalizedTime(this AnimationCurve curve, float time)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return 0f;
            }

            float min = curve.GetMinTime();
            float max = curve.GetMaxTime();

            return Mathf.Clamp01((time - min) / (max - min));
        }

        /// <summary>
        /// Gets the minimum value among all keys in the AnimationCurve.
        /// </summary>
        /// <param name="curve">The AnimationCurve to evaluate.</param>
        /// <returns>The minimum value, or 0 if the curve is null or empty.</returns>
        public static float GetMinValue(this AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return 0f;
            }

            float val = float.MaxValue;

            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (curve.keys[i].value >= val)
                {
                    continue;
                }

                val = curve.keys[i].value;
            }

            return val;
        }

        /// <summary>
        /// Gets the maximum value among all keys in the AnimationCurve.
        /// </summary>
        /// <param name="curve">The AnimationCurve to evaluate.</param>
        /// <returns>The maximum value, or 0 if the curve is null or empty.</returns>
        public static float GetMaxValue(this AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return 0f;
            }

            float val = float.MinValue;

            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (curve.keys[i].value <= val)
                {
                    continue;
                }

                val = curve.keys[i].value;
            }

            return val;
        }

        /// <summary>
        /// Normalizes a given value to a 0-1 range based on the minimum and maximum values of the AnimationCurve.
        /// </summary>
        /// <param name="curve">The AnimationCurve to evaluate.</param>
        /// <param name="value">The value to normalize.</param>
        /// <returns>The normalized value in the range [0, 1], or 0 if the curve is null or empty.</returns>
        public static float GetNormalizedValue(this AnimationCurve curve, float value)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return 0f;
            }

            float min = curve.GetMinValue();
            float max = curve.GetMaxValue();

            return Mathf.Clamp01((value - min) / (max - min));
        }
    }
}
