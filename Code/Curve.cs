using System;
using System.Collections.Generic;
using UnityEngine;

namespace c1tr00z.Curves {
    [Serializable]
    public class Curve {

        #region Serialized Fields

        [SerializeField, HideInInspector] private List<Vector3> _points;

        #endregion

        #region Accessors

        public Vector3[] points => _points.ToArray();

        public int pointsCount => points.Length;

        public int segmentsCount => (points.Length - 1) / 3;
        
        #endregion

        #region Operators

        public Vector3 this[int i] {
            get {
                if (_points.Count > i) {
                    return _points[i];
                }
                throw new IndexOutOfRangeException("Point index can't be bigger that points count");
            }
            set {
                if (_points.Count > i) {
                    _points[i] = value;
                    return;
                }
                throw new IndexOutOfRangeException("Point index can't be bigger that points count");
            }
        }

        #endregion

        #region Constructors

        public Curve(Vector3 center) {
            _points = new List<Vector3> {
                center + Vector3.left,
                center + (Vector3.left + Vector3.up) * .5f,
                center + (Vector3.right + Vector3.down) * .5f,
                center + Vector3.right
            };
        }

        #endregion

        #region Class Implementation

        public void AddSegment(Vector3 newAnchorPoint) {
            _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
            _points.Add((_points[_points.Count - 1] + newAnchorPoint) * .5f);
            _points.Add(newAnchorPoint);
        }

        public Vector3[] GetPointsInSegment(int segmentIndex) {
            return new Vector3[] {
                _points[segmentIndex * 3], _points[segmentIndex * 3 + 1], 
                _points[segmentIndex * 3 + 2], _points[segmentIndex * 3 + 3]
            };
        }

        public void MovePoint(int pointIndex, Vector3 newPosition) {
            if (pointIndex >= _points.Count) {
                throw new IndexOutOfRangeException("Point index can't be bigger that points count");
            }

            _points[pointIndex] = newPosition;
        }

        #endregion
    }
}