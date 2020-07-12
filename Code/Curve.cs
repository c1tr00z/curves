using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace c1tr00z.Curves {
    [Serializable]
    public class Curve {

        #region Serialized Fields

        [SerializeField, HideInInspector] private List<Vector3> _points;

        [FormerlySerializedAs("_closed")] [SerializeField, HideInInspector] private bool _isClosed;

        #endregion

        #region Accessors

        public Vector3[] points => _points.ToArray();

        public int pointsCount => points.Length;

        public int segmentsCount => points.Length / 3;

        public bool isClosed {
            get => _isClosed;
            set {
                if (_isClosed == value) {
                    return;
                }

                _isClosed = value;
                
                if (isClosed) {
                    _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
                    _points.Add(_points[0] * 2 - _points[1]);
                } else {
                    _points.RemoveRange(_points.Count - 2, 2);
                }
            }
        }
        
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
                center + Vector3.left + Vector3.forward,
                center + Vector3.forward + Vector3.up,
                center + Vector3.back + Vector3.down,
                center + Vector3.right + Vector3.back
            };
        }

        #endregion

        #region Class Implementation

        public void AddSegment(Vector3 newAnchorPoint) {
            _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
            _points.Add((_points[_points.Count - 1] + newAnchorPoint) * .5f);
            _points.Add(newAnchorPoint);
        }

        public void RemoveSegment(int segmentIndex) {
            var newPoints = new List<Vector3>();
            var allSegmentIndexes = IEnumerableUtils.MakeIndexesTo(segmentsCount).ToList();
            allSegmentIndexes.Remove(segmentIndex);
            for (var pointIndex = 0; pointIndex < _points.Count; pointIndex++) {
                var isLastPointInSegment = segmentIndex != 0 && pointIndex == segmentIndex * 3 + 3;
                var isPointFromOtherSegments = allSegmentIndexes.Any(s => IsPointInSegment(s, pointIndex));
                if (!isLastPointInSegment && isPointFromOtherSegments) {
                    newPoints.Add(_points[pointIndex]);
                }
            }

            _points = newPoints;
        }

        public Vector3[] GetPointsInSegment(int segmentIndex) {
            return new Vector3[] {
                _points[segmentIndex * 3], _points[segmentIndex * 3 + 1], 
                _points[segmentIndex * 3 + 2], _points[GetPointLoopIndex(segmentIndex * 3 + 3)]
            };
        }

        private bool IsPointInSegment(int segmentIndex, int pointIndex) {
            var segmentPointsIndexes = new int[] {
                segmentIndex * 3, segmentIndex * 3 + 1, segmentIndex * 3 + 2, segmentIndex * 3 + 3
            };

            return segmentPointsIndexes.Contains(pointIndex);
        }

        public void MovePoint(int pointIndex, Vector3 newPosition) {
            if (pointIndex >= _points.Count) {
                throw new IndexOutOfRangeException("Point index can't be bigger that points count");
            }

            var delta = newPosition - _points[pointIndex];
            _points[pointIndex] = newPosition;
            var mod = pointIndex % 3;
            if (mod == 0) {
                if (pointIndex > 0 || isClosed) {
                    _points[GetPointLoopIndex(pointIndex - 1)] += delta;
                }

                if (pointIndex < _points.Count - 1 || isClosed) {
                    _points[GetPointLoopIndex(pointIndex + 1)] += delta;
                }
            } else if (mod == 1) {
                if (pointIndex > 1 || isClosed) {
                    _points[GetPointLoopIndex(pointIndex - 2)] = _points[GetPointLoopIndex(pointIndex - 1)] - (_points[GetPointLoopIndex(pointIndex)] - _points[
                                                                                                  GetPointLoopIndex(pointIndex - 1)]);
                }
            } else if (mod == 2) {
                if (pointIndex < _points.Count - 2 || isClosed) {
                    _points[GetPointLoopIndex(pointIndex + 2)] = _points[GetPointLoopIndex(pointIndex + 1)] - (_points[GetPointLoopIndex(pointIndex)] - _points[
                                                                                                  GetPointLoopIndex(pointIndex + 1)]);
                }
            }
        }

        public void MoveSegmentPoint(int segmentIndex, int pointIndexInSegment, Vector3 newPosition) {
            var realPointIndex = segmentIndex * 3 + pointIndexInSegment;
            MovePoint(realPointIndex, newPosition);
        }

        private int GetPointLoopIndex(int pointIndex) {
            return (pointIndex + _points.Count) % _points.Count;
        }

        // public Vector3[] GetPointsOnCurve(int resolution) {
        //     
        // }

        #endregion
    }
}