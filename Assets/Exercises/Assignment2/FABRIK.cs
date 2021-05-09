using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Assignment2
{
    public class FABRIK : MonoBehaviour
    {
        [Tooltip("the joints that we are controlling")]
        public Transform[] joints;

        [Tooltip("target that our end effector is trying to reach")]
        public Transform target;

        [Tooltip("error tolerance, will stop updating after distance between end effector and target is smaller than tolerance.")]
        [Range(.01f, .2f)]
        public float tolerance = 0.05f;

        [Tooltip("maximum number of iterations before we follow to the next frame")]
        [Range(1, 100)]
        public int maxIterations = 20;

        [Tooltip("rotation constraint. " +
        	"Instead of an elipse with 4 rotation limits, " +
        	"we use a circle with a single rotation limit. " +
        	"Implementation will be a lot simpler than in the paper.")]
        [Range(0f, 180f)]
        public float rotationLimit = 45f;

        // distances/lengths between joints.
        private float[] distances;
        // total length of the system
        private float chainLength;


        private void Solve()
        {
            float rootTargetDist = (joints[0].position - target.position).magnitude;
            if(rootTargetDist > chainLength)
            {
                Unreachable();
            }
            else
            {
                Vector3 rootConstraintPos = joints[0].position;
                float difToTarget = (joints[joints.Length - 1].position - target.position).magnitude;
                int curIteration = 0;
                while (difToTarget > tolerance && curIteration < maxIterations)
                {
                    joints[joints.Length - 1].position = target.position;
                    FwdReach();
                    joints[0].position = rootConstraintPos;
                    BwdReach();
                    difToTarget = (joints[joints.Length - 1].position - target.position).magnitude;
                    curIteration++;
                }
            }
            AdjustRotation();
        }

        private void Unreachable()
        {
            float distToTarget, lambdaDist;
            for (int i=0; i<joints.Length - 1; i++)
            {
                distToTarget = (joints[i].position - target.position).magnitude;
                lambdaDist = distances[i] / distToTarget;
                joints[i+1].position = (1 - lambdaDist) * joints[i].position + lambdaDist * target.position;
            }
        }

        private void FwdReach()
        {
            float distToTarget, lambdaDist;
            for (int i = joints.Length - 2; i > -1; i--)
            {
                distToTarget = (joints[i+1].position - joints[i].position).magnitude;
                lambdaDist = distances[i] / distToTarget;
                joints[i].position = (1 - lambdaDist) * joints[i+1].position + lambdaDist * joints[i].position;
                ConstrainRotation(i);
            }
        }

        private void BwdReach()
        {
            float distToTarget, lambdaDist;
            for (int i = 0; i < joints.Length - 1; i++)
            {
                distToTarget = (joints[i + 1].position - joints[i].position).magnitude;
                lambdaDist = distances[i] / distToTarget;
                joints[i+1].position = (1 - lambdaDist) * joints[i].position + lambdaDist * joints[i+1].position;
                ConstrainRotation(i + 1);
            }
        }
        private void ConstrainRotation(int index)
        {
            if (index < 1 || index > joints.Length - 2)
                return;

            Vector3 l = joints[index].position - joints[index - 1].position;
            Vector3 ln = joints[index + 1].position - joints[index].position;

            if (Vector3.Angle(l, ln) < rotationLimit)
                return;

            Vector3 o = Vector3.Project(ln, l);

            if(rotationLimit < 90f && Vector3.Dot(l, ln) < 0)
            {
                o = -o;
                ln = Vector3.Reflect(ln, l);
            }

            Vector3 po = joints[index].position + o;

            Vector3 d = (joints[index + 1].position - po).normalized;

            float r = o.magnitude * Mathf.Tan(Mathf.Deg2Rad * rotationLimit);

            joints[index + 1].position = po + r * d;
        }

        private void AdjustRotation()
        {
            for(int i=0; i<joints.Length-1; i++)
            {
                Vector3 direction = (joints[i + 1].position - joints[i].position).normalized;
                joints[i].right = direction;

                //Vector3 direction = (joints[i + 1].position - joints[i].position).normalized;
                //Vector3 rightVec = joints[i].right;

                //Vector3 axis = Vector3.Cross(rightVec, direction).normalized;
                //float angle = Mathf.Acos(Vector3.Dot(rightVec, direction) / (rightVec.magnitude * direction.magnitude));

                //joints[i].rotation = Quaternion.AngleAxis(angle, axis);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // pre-compute segment lenghts and total length of the chain
            // we assume that the segment/bone length is constant during execution
            distances = new float[joints.Length-1];
            chainLength = 0;
            // If we have N joints, then there are N-1 segment/bone lengths connecting these joints
            for (int i = 0; i < joints.Length - 1; i++)
            {
                distances[i] = (joints[i + 1].position - joints[i].position).magnitude;
                chainLength += distances[i];
            }
        }

        void Update()
        {
            Solve();
            for (int i = 1; i < joints.Length - 1; i++)
            {
                DebugJointLimit(joints[i], joints[i - 1], rotationLimit, 2);
            }
        }

        /// <summary>
        /// Helper function to draw the joint limit in the editor
        /// The drawing migh not make sense if you did not complete the 
        /// second task in the assignment (joint rotations)
        /// </summary>
        /// <param name="tr">current joint</param>
        /// <param name="trPrev">previous joint</param>
        /// <param name="angle">angle limit in degrees</param>
        /// <param name="scale"></param>
        void DebugJointLimit(Transform tr, Transform trPrev, float angle, float scale = 1)
        {
            float angleRad = Mathf.Deg2Rad * angle;
            float cosAngle = Mathf.Cos(angleRad);
            float sinAngle = Mathf.Sin(angleRad);
            int steps = 36;
            float stepSize = 360f / steps;
            // steps is the number of line segments used to draw the cone
            for (int i = 0; i < steps; i++)
            {
                float twistRad = Mathf.Deg2Rad * i * stepSize;
                Vector3 vec = new Vector3(cosAngle, 0, 0);
                vec.y = Mathf.Cos(twistRad) * sinAngle;
                vec.z = Mathf.Sin(twistRad) * sinAngle;
                vec = trPrev.rotation * vec;
                
                twistRad = Mathf.Deg2Rad * (i+1) * stepSize;
                Vector3 vec2 = new Vector3(cosAngle, 0, 0);
                vec2.y = Mathf.Cos(twistRad) * sinAngle;
                vec2.z = Mathf.Sin(twistRad) * sinAngle;
                vec2 = trPrev.rotation * vec2;

                Debug.DrawLine(tr.position, tr.position + vec * scale, Color.white);
                Debug.DrawLine(tr.position + vec * scale, tr.position + vec2 * scale, Color.white);
            }
        }
    }

}