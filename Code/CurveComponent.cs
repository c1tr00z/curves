using System.Collections.Generic;
using UnityEngine;

namespace c1tr00z.Curves {
    public class CurveComponent : MonoBehaviour{

        #region Private Fields

        [SerializeField, HideInInspector]
        private Curve _curve;

        #endregion

        #region Public Fields

        public CurveMode curveMode = CurveMode.Mode3D;

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

        public bool isClosed {
            get => curve.isClosed;
            set => curve.isClosed = value;
        }

        public bool isAutoSetControlPoints {
            get => curve.isAutoSetControlPoints;
            set => curve.isAutoSetControlPoints = value;
        }

        public int pointsCount => curve.pointsCount;

        public int segmentsCount => curve.segmentsCount;

        #endregion

        #region Operators

        public Vector3 this[int pointIndex] {
            get => curve[pointIndex] + transform.position;
            set =>  MovePoint(pointIndex, value);
        }

        #endregion

        #region Class Implementation

        public void MakeCurve() {
            if (_curve == null) {
                _curve = new Curve(transform.position);
            }
        }

        public void AddSegment(Vector3 center) {
            curve.AddSegment(center - transform.position);
        }

        public void RemoveSegment(int segmentIndex) {
            curve.RemoveSegment(segmentIndex);
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
        
        public void MoveSegmentPoint(int segmentIndex, int pointIndexInSegment, Vector3 newPosition) {
            newPosition -= transform.position;
            curve.MoveSegmentPoint(segmentIndex, pointIndexInSegment, newPosition);
        }

        public void AutoSetControlPoints() {
            curve.AutoSetControlPoints();
        }

        #endregion
    }
}