using System;
using UnityEngine;

namespace Yontalane
{
    [System.Serializable]
    public class FloatRange
    {
        public float min = 0f;
        public float max = 0f;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public FloatRange(float minAndMax) : this(minAndMax, minAndMax) { }

        public FloatRange() : this(0f) { }

        /// <summary>
        /// The midpoint between the range's min and max values.
        /// </summary>
        public float Mid => Lerp(0.5f);

        /// <summary>
        /// Linearly interpolates between the range's min and max values by t.
        /// </summary>
        /// <returns>The interpolated float result between the two float values.</returns>
        public float Lerp(float t) => Mathf.Lerp(min, max, t);

        /// <summary>
        /// A random float between the range's min and max values.
        /// </summary>
        public float Random => Lerp(UnityEngine.Random.value);

        public override string ToString() => $"{GetType()} {{ {min}, {max} }}";
    }

    [System.Serializable]
    public class IntRange
    {
        public int min = 0;
        public int max = 0;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public IntRange(int minAndMax) : this(minAndMax, minAndMax) { }

        public IntRange() : this(0) { }

        /// <summary>
        /// The midpoint (rounded to the nearest integer) between the range's min and max values.
        /// </summary>
        public int Mid => Lerp(0.5f);

        /// <summary>
        /// Linearly interpolates between the range's min and max values by t.
        /// </summary>
        /// <returns>The interpolated result between the two values, rounded to the nearest int.</returns>
        public int Lerp(float t) => Mathf.RoundToInt(Mathf.Lerp(min, max, t));

        /// <summary>
        /// A random int between the range's min and max values.
        /// </summary>
        public int Random => Lerp(UnityEngine.Random.value);

        public override string ToString() => $"{GetType()} {{ {min}, {max} }}";
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ClampAttribute : PropertyAttribute
    {
        public readonly float min = 0f;
        public readonly float max = 1f;

        public ClampAttribute(float min = 0f, float max = 1f)
        {
            this.min = min;
            this.max = max;
        }
    }
}