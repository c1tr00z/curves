using UnityEngine;

namespace c1tr00z.Curves {
    public static class BezierUtils {
        public static Vector3 EvalQuadratic(Vector3 a, Vector3 b, Vector3 c, float t) {
            var p0 = Vector3.Lerp(a, b, t);
            var p1 = Vector3.Lerp(b, c, t);
            return Vector3.Lerp(p0, p1, t);
        }

        public static Vector3 EvalCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
            var p0 = EvalQuadratic(a, b, c, t);
            var p1 = EvalQuadratic(b, c, d, t);
            return Vector3.Lerp(p0, p1, t);
        }
    }
}