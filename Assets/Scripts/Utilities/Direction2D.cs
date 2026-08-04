using UnityEngine;

namespace EMIT.Utilities
{
    public static class Direction2D
    {
        public static bool IsOpposite(Vector2 vector1, Vector2 vector2)
        {
            return vector1.x * vector2.x < 0f;
        }
        public static bool IsOpposite(int flag1, int flag2)
        {
            return flag1 * flag2 < 0f;
        }
        public static bool IsOpposite(float flag1, float flag2)
        {
            return flag1 * flag2 < 0f;
        }
        
        /// <summary>
        /// source가 바라보는 방향을 Vector2로 반환한다.
        /// </summary>
        public static Vector2 FacingDir(Transform source)
        {
            return source == null ? Vector2.zero : FacingDir(source.right);
        }

        /// <summary>
        /// source의 x축 방향을 Vector2로 반환한다.
        /// </summary>
        public static Vector2 FacingDir(Vector2 direction)
        {
            return FacingDir(direction.x);
        }

        /// <summary>
        /// horizontal이 음수이면 Vector2.left, 양수이면 Vector2.right을 반환한다.
        /// 0이라면 Vector2.zero를 반환한다. 
        /// </summary>
        public static Vector2 FacingDir(float horizontal)
        {
            if (horizontal > 0f) return Vector2.right;
            if (horizontal < 0f) return Vector2.left;

            return Vector2.zero;
        }

        /// <summary>
        /// horizontal이 음수이면 Vector2.left, 양수이면 Vector2.right을 반환한다.
        /// 0이라면 Vector2.zero를 반환한다. 
        /// </summary>
        public static Vector2 FacingDir(int horizontal)
        {
            if (horizontal > 0) return Vector2.right;
            if (horizontal < 0) return Vector2.left;

            return Vector2.zero;
        }
    }
}
