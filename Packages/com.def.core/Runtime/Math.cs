using UnityEngine;

namespace DEF
{
    public static class Math
    {
        public static Vector2 RadianToVector2(float radian) => new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

        public static Vector2 DegreeToVector2(float degree) => RadianToVector2(degree * Mathf.Deg2Rad);

        public static Vector2 RotateByRadians(this Vector2 vector, float radians) => new Vector2(
                vector.x * Mathf.Cos(radians) - vector.y * Mathf.Sin(radians),
                vector.x * Mathf.Sin(radians) + vector.y * Mathf.Cos(radians)
            );

        public static Vector2 RotateByDegrees(this Vector2 vector, float degrees) => RotateByRadians(vector, degrees * Mathf.Deg2Rad);

        public static Vector3 SetX(this Vector3 vector, float value) => new Vector3()
        {
            x = value,
            y = vector.y,
            z = vector.z
        };

        public static Vector3 SetY(this Vector3 vector, float value) => new Vector3()
        {
            x = vector.x,
            y = value,
            z = vector.z
        };

        public static Vector3 SetZ(this Vector3 vector, float value) => new Vector3()
        {
            x = vector.x,
            y = vector.y,
            z = value
        };

        public static Vector2Int ToVector2Int(this Vector2 vector) => new Vector2Int()
        {
            x = Mathf.RoundToInt(vector.x),
            y = Mathf.RoundToInt(vector.y)
        };
    }
}