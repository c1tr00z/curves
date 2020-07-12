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

            var newClosedValue = EditorGUILayout.Toggle("Closed curve", curveComponent.isClosed);
            if (newClosedValue != curveComponent.isClosed) {
                ToggleClosed(newClosedValue);
            }
            
            if (GUILayout.Button("Add segment")) {
                AddSegment();
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
                            MoveSegmentPoint(segmentIndex, pointIndex, newPointPosition);
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }

                if (segmentToRemove > -1) {
                    RemoveSegment(segmentToRemove);
                }
            }
        }

        private void OnSceneGUI() {
            Input();
            DrawHandles();
        }

        private void DrawHandles() {
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
                MovePoint(i, newPosition);

                Handles.color = handlesColor;
            }

            for (int i = 0; i < curveComponent.segmentsCount; i++) {
                var segment = curveComponent.GetPointsInSegment(i);
                Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2], Color.green, null, 2);
                Handles.DrawLine(segment[0], segment[1]);
                Handles.DrawLine(segment[2], segment[3]);
            }
        }

        private void Input() {
            var guiEvent = Event.current;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
                var mousePosition = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(guiEvent.mousePosition);
                if (curveComponent.curveMode == CurveMode.Mode2DVertical) {
                    AddSegment(mousePosition);
                } else {
                    AddSegment();
                }
            }
        }

        private void AddSegment() {
            AddSegment(curveComponent.points.Last() + Vector3.right);
        }

        private void AddSegment(Vector3 anchorPoint) {
            Undo.RecordObject(curveComponent, "Add segment");
            curveComponent.AddSegment(anchorPoint);
        }

        private void RemoveSegment(int segmentIndex) {
            Undo.RecordObject(curveComponent, "Remove segment");
            curveComponent.RemoveSegment(segmentIndex);
        }

        private void MovePoint(int pointIndex, Vector3 newPosition) {
            Undo.RecordObject(curveComponent, "Move point");
            curveComponent[pointIndex] = newPosition;
        }
        
        private void MoveSegmentPoint(int segmentIndex, int pointIndexInSegment, Vector3 newPosition) {
            Undo.RecordObject(curveComponent, "Move point");
            curveComponent.MoveSegmentPoint(segmentIndex, pointIndexInSegment, newPosition);
        }

        private void ToggleClosed(bool newClosedValue) {
            Undo.RecordObject(curveComponent, "Toggle closed");
            curveComponent.isClosed = newClosedValue;
        }

        #endregion
        
        
    }
}