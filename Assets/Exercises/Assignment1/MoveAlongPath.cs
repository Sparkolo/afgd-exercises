using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Assignment1
{
    public class MoveAlongPath : MonoBehaviour
    {
        public DebugCurve curve;
        private float t = 0;
        public float duration = 4;

        void Update(){
            if (!curve.Init())
                return;

            t += Time.deltaTime/duration;
            t %= 1;

            float easedT = EasingFunctions.Linear(t); // you can use any easing function here

            float arcLength;
            CurveSegment curveSegment = curve.CurveSegmentAtArcLength(easedT * curve.ArcLength(curve.CurveCount()), out arcLength);
            float curveT = curveSegment.ComputeTAtArcLength(arcLength);

            transform.position = curveSegment.Evaluate(curveT);
            transform.forward = curveSegment.EvaluateDv(curveT);
        }
    }
}