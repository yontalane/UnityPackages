using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A static class containing math utility methods.
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// Create a vector representation of an angle.
        /// </summary>
        public static Vector2 RadianToVector2(float radian) => new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

        /// <summary>
        /// Create a vector representation of an angle.
        /// </summary>
        public static Vector2 DegreeToVector2(float degree) => RadianToVector2(degree * Mathf.Deg2Rad);

        /// <summary>
        /// Rotate the vector.
        /// </summary>
        public static Vector2 RotateByRadians(this Vector2 vector, float radians) => new()
        {
            x = vector.x * Mathf.Cos(radians) - vector.y * Mathf.Sin(radians),
            y = vector.x * Mathf.Sin(radians) + vector.y * Mathf.Cos(radians)
        };

        /// <summary>
        /// Rotate the vector.
        /// </summary>
        public static Vector2 RotateByDegrees(this Vector2 vector, float degrees) => RotateByRadians(vector, degrees * Mathf.Deg2Rad);

        /// <summary>
        /// Set a single value of the vector.
        /// </summary>
        public static Vector3 SetX(this Vector3 vector, float value) => new()
        {
            x = value,
            y = vector.y,
            z = vector.z
        };

        /// <summary>
        /// Set a single value of the vector.
        /// </summary>
        public static Vector3 SetY(this Vector3 vector, float value) => new()
        {
            x = vector.x,
            y = value,
            z = vector.z
        };

        /// <summary>
        /// Set a single value of the vector.
        /// </summary>
        public static Vector3 SetZ(this Vector3 vector, float value) => new()
        {
            x = vector.x,
            y = vector.y,
            z = value
        };

        /// <summary>
        /// Convert a float-based vector to an integer-based vector.
        /// </summary>
        public static Vector2Int ToVector2Int(this Vector2 vector) => new()
        {
            x = Mathf.RoundToInt(vector.x),
            y = Mathf.RoundToInt(vector.y)
        };

        /// <summary>
        /// Convert a float-based vector to an integer-based vector.
        /// </summary>
        public static Vector3Int ToVector3Int(this Vector3 vector) => new()
        {
            x = Mathf.RoundToInt(vector.x),
            y = Mathf.RoundToInt(vector.y),
            z = Mathf.RoundToInt(vector.z)
        };
    }
}