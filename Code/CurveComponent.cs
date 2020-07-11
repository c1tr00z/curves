using UnityEngine;

namespace c1tr00z.Curves {
    public class CurveComponent : MonoBehaviour{

        #region Private Fields

        [SerializeField, HideInInspector]
        private Curve _curve;

        #endregion

        #region Accessors

        public Curve curve {
            get {
                MakeCurve();
                
                return _curve;
            }
        }

        public Vector3[] points {
            get {
                var curvePoints = curve.points;
                for (var i = 0; i < curvePoints.Length; i++) {
                    curvePoints[i] += transform.position;
                }

                return curvePoints;
            }
        }

        public int pointsCount => curve.pointsCount;

        public int segmentsCount => curve.segmentsCount;

        #endregion

        #region Operators

        public Vector3 this[int pointIndex] {
            get => curve[pointIndex] + transform.position;
            set => curve[pointIndex] = value - transform.position;
        }

        #endregion

        #region Class Implementation

        public void MakeCurve() {
            if (_curve == null) {
                _curve = new Curve(transform.position);
            }
        }

        public void AddSegment(Vector3 center) {
            curve.AddSegment(center);
        }

        public Vector3[] GetPointsInSegment(int segmentIndex) {
            var points = curve.GetPointsInSegment(segmentIndex);
            for (var i = 0; i < points.Length; i++) {
                points[i] += transform.position;
            }

            return points;
        }

        public void MovePoint(int pointIndex, Vector3 newPosition) {
            newPosition -= transform.position;
            curve.MovePoint(pointIndex, newPosition);
        }

        #endregion
    }
}