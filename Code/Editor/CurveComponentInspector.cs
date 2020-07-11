using System;
using c1tr00z.AssistLib.Utils;
using UnityEditor;
using UnityEngine;

namespace c1tr00z.Curves.Editor {
    [CustomEditor(typeof(CurveComponent))]
    public class CurveComponentInspector : UnityEditor.Editor {

        #region Private Fields

        private CurveComponent _curveComponent;

        private bool _showSegments;

        #endregion
        
        #region Accessors

        public CurveComponent curveComponent =>
            CommonUtils.GetCachedObject(ref _curveComponent, () => target as CurveComponent);

        #endregion

        #region UnityEditor.Editor Implementation

        private void OnEnable() {
            curveComponent.MakeCurve();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Add segment")) {
                curveComponent.AddSegment(curveComponent.points.Last() + Vector3.right);
            }

            _showSegments = EditorGUILayout.Foldout(_showSegments, "Show segments");
            if (_showSegments) {
                var segmentToRemove = -1;
                for (var segmentIndex = 0; segmentIndex < curveComponent.segmentsCount; segmentIndex++) {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Segment #{segmentIndex}");
                    if (GUILayout.Button("X", GUILayout.Width(32))) {
                        segmentToRemove = segmentIndex;
                    }
                    EditorGUILayout.EndHorizontal();

                    var segmentPoints = curveComponent.GetPointsInSegment(segmentIndex);
                    for (var pointIndex = 0; pointIndex < segmentPoints.Length; pointIndex++) {
                        var newPointPosition =
                            EditorGUILayout.Vector3Field($"Point#{pointIndex}", segmentPoints[pointIndex]);

                        if (segmentPoints[pointIndex] != newPointPosition) {
                            Undo.RecordObject(curveComponent, "Move point");
                            curveComponent.MoveSegmentPoint(segmentIndex, pointIndex, newPointPosition);
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }

                if (segmentToRemove > -1) {
                    Undo.RecordObject(curveComponent, "Remove segment");
                    curveComponent.RemoveSegment(segmentToRemove);
                }
            }
        }

        private void OnSceneGUI() {
            var handlesColor = Handles.color;
            for (var i = 0; i < curveComponent.pointsCount; i++) {

                if (i % 3 == 0) {
                    Handles.color = Color.red;
                } else {
                    Handles.color = Color.blue;
                }
                
                var newPosition = Handles.FreeMoveHandle(curveComponent[i], Quaternion.identity, 
                    .1f, Vector3.zero, Handles.SphereHandleCap);

                if (curveComponent.curveMode == CurveMode.Mode2DHorizontal) {
                    newPosition.y = curveComponent.transform.position.y;
                } else if (curveComponent.curveMode == CurveMode.Mode2DVertical) {
                    newPosition.z = curveComponent.transform.position.z;
                }
                
                if (newPosition == curveComponent[i]) {
                    continue;
                }
                Undo.RecordObject(curveComponent, "Move point");
                curveComponent[i] = newPosition;

                Handles.color = handlesColor;
            }

            for (int i = 0; i < curveComponent.segmentsCount; i++) {
                var segment = curveComponent.GetPointsInSegment(i);
                Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2], Color.green, null, 2);
                Handles.DrawLine(segment[0], segment[1]);
                Handles.DrawLine(segment[2], segment[3]);
            }
        }

        #endregion
        
        
    }
}