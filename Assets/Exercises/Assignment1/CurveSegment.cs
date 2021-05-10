using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Assignment1
{
    public static class CurveCoefficients
    {
        /// <summary>
        /// coefficient matrices
        /// </summary>
        private static Matrix4x4 hermiteMatrix = new Matrix4x4(
                    new Vector4(2, 1, 1, -2),  // column 0
                    new Vector4(-3, -2, -1, 3),  // column 1
                    new Vector4(0, 1, 0, 0),  // column 2
                    new Vector4(1, 0, 0, 0)  // column 3
            ); 

        /// <summary>
        /// Return 4x4 coefficient matrix of type Hermite
        /// </summary>
        /// <returns></returns>
        public static Matrix4x4 GetCoefficients()
        {
            return hermiteMatrix;
        }
    }



    public class CurveSegment
    {
        /// <summary>
        /// B contains the control parameters (points/vectors) of the curve
        /// </summary>
        public Matrix4x4 B;

        /// <summary>
        /// the M matrix contains the coefficients of the cubic polynomials used in the curve formulation
        /// </summary>
        private Matrix4x4 M;


        private int resolution = 30;
        public int Resolution() { return resolution; }

        private List<float> arcLengths = new List<float>();
        public float ArcLength(int i) { return arcLengths[i]; }

        public float ComputeTAtArcLength(float desiredLength){
            for (int i = 1; i <= resolution; i++){
                if(ArcLength(i) >= desiredLength)
                {
                    float deltaArcLength = (desiredLength - ArcLength(i - 1)) / (ArcLength(i) - ArcLength(i - 1));
                    return Mathf.Lerp((i - 1) / resolution, i / resolution, deltaArcLength);
                }
            }
            return 0;
        }

        /// <summary>
        /// Curve segment contructor
        /// </summary>
        /// <param name="cv1">control value 1</param>
        /// <param name="cv2">control value 2</param>
        /// <param name="cv3">control value 3</param>
        /// <param name="cv4">control value 4</param>
        public CurveSegment(Vector4 cv1, Vector4 cv2, Vector4 cv3, Vector4 cv4)
        {

            B = new Matrix4x4(
                    new Vector4(cv1.x, cv1.y, cv1.z, 0),  // column 0
                    new Vector4(cv2.x, cv2.y, cv2.z, 0),  // column 1
                    new Vector4(cv3.x, cv3.y, cv3.z, 0),  // column 2
                    new Vector4(cv4.x, cv4.y, cv4.z, 0)); // column 3

            M = CurveCoefficients.GetCoefficients();

            // Evaluate Arc Length
            arcLengths.Clear();
            arcLengths.Add(0);
            for (int i = 1; i < resolution + 1; i++){
                float evaluatedDistance = (Evaluate(i / resolution) - Evaluate((i - 1) / resolution)).magnitude;
                arcLengths.Add(arcLengths[i - 1] + evaluatedDistance);
            }
        }

        /// <summary>
        /// evaluate curve segment at u, for u in the normalized range [0,1]
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Vector4 Evaluate(float u)
        {
            // DONE 2.1 exercise
            // compute parameter matrix U and evaluate p at u
            Vector4 U = new Vector4(u*u*u, u*u, u, 1); // replace Vector4.zero
            Vector4 p = B*M*U; // replace Vector4.zero
            return p;
        }

        /// <summary>
        /// evaluate tangent of curve segment at u, for u in the normalized range [0,1]
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Vector4 EvaluateDv(float u)
        {
            // DONE 2.1 exercise
            // compute parameter matrix U and evaluate p at u
            // you should compute the first derivative of U
            Vector4 U = new Vector4(3*u*u, 2*u, 1, 0); // replace Vector4.zero
            Vector4 p = B*M*U; // replace Vector4.zero
            return p;
        }

        /// <summary>
        /// evaluate curvature of curve segment at u, for u in the normalized range [0,1]
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Vector4 EvaluateDv2(float u)
        {
            // DONE 2.1 exercise
            // compute parameter matrix U and evaluate p at u
            // you should compute the second derivative of U
            Vector4 U = new Vector4(6*u, 2, 0, 0); // replace Vector4.zero
            Vector4 p = B*M*U; // replace Vector4.zero
            return p;
        }

    }
}