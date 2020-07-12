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

        [SerializeField, HideInInspector] private bool _isClosed;

        [SerializeField, HideInInspector] private bool _isAutoSetControlPoints;

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

        public bool isAutoSetControlPoints {
            get => _isAutoSetControlPoints;
            set {
                if (_isAutoSetControlPoints == value) {
                    return;
                }

                _isAutoSetControlPoints = value;

                if (_isAutoSetControlPoints) {
                    AutoSetControlPoints();
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

            if (isAutoSetControlPoints) {
                AutoSetControlPoints();
            }
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

            if (isAutoSetControlPoints) {
                AutoSetControlPoints();
                return;
            }
            
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

        public void AutoSetControlPoints() {
            var anchorIndexes = new List<int>();
            for (var i = 0; i < _points.Count; i++) {
                if (i > 0 && i < _points.Count - 1 && i % 3 == 0) {
                    anchorIndexes.Add(i);
                }
            }
            for (var i = 0; i < anchorIndexes.Count; i++) {
                AutoSetControlPoints(anchorIndexes[i]);
            }

            AutoSetStartEndControlPoints();
        }

        private void AutoSetControlPoints(int anchorPointIndex) {
            if (anchorPointIndex >= _points.Count && !isClosed)
            {
                Debug.LogError("Point index should be less than points count or curve should be closed");
                return;
            }
            if (anchorPointIndex % 3 != 0) {
                Debug.LogWarning("Parameter should lead to anchor point");
                return;
            } 
            var anchorPoint = _points[GetPointLoopIndex(anchorPointIndex)];
            var direction = Vector3.zero;
            var neighbourDistances = new float[2];

            var firstNeighbourIndex = anchorPointIndex - 3;
            if (firstNeighbourIndex >= 0 || isClosed) {
                var offset = _points[GetPointLoopIndex(firstNeighbourIndex)] - anchorPoint;
                direction += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }
            
            var secondNeighbourIndex = anchorPointIndex + 3;
            if (secondNeighbourIndex >= 0 || isClosed) {
                var offset = _points[GetPointLoopIndex(secondNeighbourIndex)] - anchorPoint;
                direction -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }
            
            direction.Normalize();
            
            for (var i = 0; i < neighbourDistances.Length; i++) {
                var controlIndex = anchorPointIndex + i * 2 - 1;
                if (controlIndex >= 0 && controlIndex < _points.Count || isClosed) {
                    _points[GetPointLoopIndex(controlIndex)] = anchorPoint + direction * neighbourDistances[i] * .5f;
                }
            }
        }

        private void AutoSetStartEndControlPoints() {
            if (isClosed) {
                return;
            }

            _points[1] = (_points[0] + _points[2]) * .5f;
            _points[_points.Count - 2] = (_points[_points.Count - 1] + _points[_points.Count - 3]) * .5f;
        }

        private void AutoSetAffected(int updatedControlPoint) {
            for (var i = updatedControlPoint - 3; i <= updatedControlPoint + 3; i += 3) {
                if (i >= 0 && i < _points.Count || isClosed) {
                    AutoSetControlPoints(i);
                }
            }
            
            AutoSetStartEndControlPoints();
        }

        private float CalculateSegmentLenght(int segmentIndex) {
            var segmentPoints = GetPointsInSegment(segmentIndex);
            var controlNetSize = Vector3.Distance(segmentPoints[0], segmentPoints[1]) +
                                 Vector3.Distance(segmentPoints[1], segmentPoints[2]) +
                                 Vector3.Distance(segmentPoints[2], segmentPoints[3]);

            var curveLength = Vector3.Distance(segmentPoints[0], segmentPoints[3]) + controlNetSize / 2;
            return curveLength;
        }

        private float CalculateCurveLength() {
            var curveLength = 0f;
            for (int i = 0; i < segmentsCount; i++) {
                curveLength += CalculateSegmentLenght(i);
            }

            return curveLength;
        }

        public Vector3[] CalculatePointsOnCurve(int resolution) {
            var calculatedPoints = new List<Vector3>();
            calculatedPoints.Add(_points[0]);

            var prevPoint = points[0];
            var distanceFromLastPoint = 0f;

            resolution = resolution < segmentsCount ? segmentsCount : resolution;

            var spacing = CalculateCurveLength() / resolution;

            for (int segmentIndex = 0; segmentIndex < segmentsCount; segmentIndex++) {
                var segmentPoints = GetPointsInSegment(segmentIndex);
                var divisions = Mathf.CeilToInt(resolution);
                float t = 0;
                while (t <= 1f) {
                    t += 1f/divisions;

                    var pointOnCurve = BezierUtils.EvalCubic(segmentPoints[0], segmentPoints[1], 
                        segmentPoints[2], segmentPoints[3], t);

                    distanceFromLastPoint += Vector3.Distance(prevPoint, pointOnCurve);

                    while (distanceFromLastPoint >= spacing) {
                        float overshoot = distanceFromLastPoint - spacing;
                        var newPoint = pointOnCurve + (prevPoint - pointOnCurve).normalized * overshoot;
                        calculatedPoints.Add(newPoint);
                        distanceFromLastPoint = overshoot;
                        prevPoint = newPoint;
                    }

                    prevPoint = pointOnCurve;
                }
            }

            if ((_points.Last() - calculatedPoints.Last()).magnitude > .01f) {
                calculatedPoints.Add(_points.Last());
            } else {
                // Debug.LogError($"Prev: {prevPoint} / Last: {_points.Last()}");
            }
            
            return calculatedPoints.ToArray();
        }

        #endregion
    }
}