using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Assignment1
{
    [ExecuteInEditMode]
    public class DebugCurve : MonoBehaviour
    {

        // DONE exercise 2.3
        // you will want to have more than one CurveSegment when creating a cyclic path
        // you can consider a List<CurveSegment>.
        // You may also want to add more control points, and "lock" the CurveType, since
        // different curve types make curves in different ranges
        // (e.g. Catmull-rom and B-spline make a curve from cp2 to cp3, Hermite and Bezier from cp1 to cp4)
        List<CurveSegment> curves = new List<CurveSegment>();
        public int CurveCount() { return curves.Count; }

        [Tooltip("curve control points/vectors")]
        public List<Vector3> controlPoints = new List<Vector3>();

        List<float> arcLengths = new List<float>();
        public float ArcLength(int i) { return arcLengths[i]; }


        // these variables are only used for visualization
        [Header("Debug variables")]
        [Range(2, 100)]
        public int debugSegments = 15;
        public float tangentMagnitude = 1.0f;
        public bool drawPath = true;
        public Color pathColor = Color.magenta;
        public bool drawTangents = true;
        public Color tangentColor = Color.green;


        public bool Init()
        {
            if(controlPoints.Count<2){
                return false;
            }
            // initialize curve if all control points are valid
            foreach (Vector3 cp in controlPoints)
            {
                if (cp == null)
                    return false;
            }

            curves.Clear();
            arcLengths.Clear();
            arcLengths.Add(0);
            for (int i = 0; i < controlPoints.Count-1; i++)
            {
                //float halfDist = (controlPoints[FixIndexBounds(i + 1)] - controlPoints[i]).magnitude / 2.0f;

                // Calculate the tangent to current control point
                Vector3 curToPrevDir0 = (controlPoints[FixIndexBounds(i - 1)] - controlPoints[i]).normalized;
                Vector3 curToNextDir0 = (controlPoints[FixIndexBounds(i + 1)] - controlPoints[i]).normalized;
                Vector3 tanCp0 = (curToNextDir0 - curToPrevDir0).normalized * tangentMagnitude;

                // Calculate the tangent to current control point
                Vector3 curToPrevDir1 = (controlPoints[i] - controlPoints[FixIndexBounds(i + 1)]).normalized;
                Vector3 curToNextDir1 = (controlPoints[FixIndexBounds(i + 2)] - controlPoints[FixIndexBounds(i + 1)]).normalized;
                Vector3 tanCp1 = (curToNextDir1 - curToPrevDir1).normalized * tangentMagnitude;

                // Add the curve segment
                curves.Add(new CurveSegment(controlPoints[i], tanCp0, tanCp1, controlPoints[i + 1]));
                arcLengths.Add(arcLengths[i] + curves[i].ArcLength(curves[i].Resolution()));
            }
            return true;
        }

        private int FixIndexBounds(int index)
        {
            if (index < 0)
                return 0;
            if (index > controlPoints.Count - 1)
                return controlPoints.Count - 1;
            return index;
        }

        public static void DrawCurveSegments(CurveSegment curve,
            Color color, int segments = 50)
        {
            // DONE exercise 2.2
            // evaluate the curve from start to end (range [0, 1])
            // and you draw a number of line segments between
            // consecutive points
            for (float i = 0; i < segments; i++)
            {
                Debug.DrawLine(curve.Evaluate(i/segments), curve.Evaluate((i+1)/segments), color);
            }

        }

        public static void DrawTangents(CurveSegment curve,
            Color color, int segments = 50, float scale = 0.1f)
        {
            // DONE exercise 2.2
            // evaluate the curve and tangent from start to end (range [0, 1])
            // and draw the tangent as a line from the current curve point
            // to the current point + the tangent vector
            for (float i = 0; i < segments; i++)
            {
                Vector4 p = curve.Evaluate(i / segments);
                Debug.DrawLine(p, p+curve.EvaluateDv(i/segments)*scale, color);
            }
        }

        public CurveSegment CurveSegment(int i) => curves[i];
        public CurveSegment CurveSegmentAtArcLength(float arcLength, out float residualArcLength)
        {
            for (int i = 1; i <= curves.Count; i++){
                if(ArcLength(i) >= arcLength){
                    residualArcLength = arcLength - ArcLength(i - 1);
                    return CurveSegment(i - 1);
                }
            }
            residualArcLength = 0;
            return CurveSegment(0);
        }

        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (Application.isEditor)
            {
                // reinitialize if we change somethign while not playing
                // this is here so we can update the debug draw of the curve
                // while in edit mode
                if (!Init())
                    return;
            }

            // draw the debug shapes
            for (int i = 0; i < curves.Count; i++)
            {
                if (drawPath)
                    DrawCurveSegments(curves[i], pathColor, debugSegments);
                if (drawTangents)
                    DrawTangents(curves[i], tangentColor, debugSegments);
            }

        }
    }
}