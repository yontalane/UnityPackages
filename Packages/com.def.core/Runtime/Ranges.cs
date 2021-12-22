using UnityEngine;

namespace DEF
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

        public float Mid => Mathf.Lerp(min, max, 0.5f);

        public float Random => Mathf.Lerp(min, max, UnityEngine.Random.value);

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

        public int Mid => Mathf.RoundToInt(Mathf.Lerp((float)min, (float)max, 0.5f));

        public int Random => Mathf.RoundToInt(Mathf.Lerp((float)min, (float)max, UnityEngine.Random.value));

        public override string ToString() => $"{GetType()} {{ {min}, {max} }}";
    }
}