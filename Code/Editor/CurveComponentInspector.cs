using System;
using c1tr00z.AssistLib.Utils;
using UnityEditor;
using UnityEngine;

namespace c1tr00z.Curves.Editor {
    [CustomEditor(typeof(CurveComponent))]
    public class CurveComponentInspector : UnityEditor.Editor {

        #region Private Fields

        private CurveComponent _curveComponent;

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
        }

        private void OnSceneGUI() {
            for (var i = 0; i < curveComponent.pointsCount; i++) {
                var newPosition = Handles.FreeMoveHandle(curveComponent[i], Quaternion.identity, 
                    .1f, Vector3.zero, Handles.SphereHandleCap);
                if (newPosition == curveComponent[i]) {
                    continue;
                }
                Undo.RecordObject(curveComponent, "Move point");
                curveComponent[i] = newPosition;
            }

            for (int i = 0; i < curveComponent.segmentsCount; i++) {
                var segment = curveComponent.GetPointsInSegment(i);
                Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2], Color.blue, null, 2);
                Handles.DrawLine(segment[0], segment[1]);
                Handles.DrawLine(segment[2], segment[3]);
            }
        }

        #endregion
        
        
    }
}