using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlowLite;

public class AnimateCharacter : MonoBehaviour
{

    [System.Serializable]
    public class JointPoint
    {
        //public Vector3 Pos3D = new Vector3();
        public Vector3 Pos3D = new Vector3();
        public Vector3 Now3D = new Vector3();
        public Vector3 PrevPos3D = new Vector3();

        // Bones
        public Transform Transform = null;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;

        public JointPoint Child = null;
        public JointPoint Parent = null;
    }

    public class Skeleton
    {
        public GameObject LineObject;
        public LineRenderer Line;

        public JointPoint start = null;
        public JointPoint end = null;
    }

    private List<Skeleton> Skeletons = new List<Skeleton>();
    public Material SkeletonMaterial;

    public bool ShowSkeleton;
    private bool useSkeleton;
    public float SkeletonX;
    public float SkeletonY;
    public float SkeletonZ;
    public float SkeletonScale;

    // Joint position and bone
    private JointPoint[] jointPoints;

    private Vector3 initPosition; // Initial center position

    private Quaternion InitGazeRotation;
    private Quaternion gazeInverse;

    // UnityChan
    public GameObject ModelObject1;
    public GameObject ModelObject2;
    public GameObject ModelObject3;
    public GameObject ModelObject4;

    static public int activeModel = 1;
    public GameObject Nose;
    private Animator anim;

    /* Called by PoseNetSample.Start()  */
    public void SetNetworkResults(NetworkResult [] networkResults)
    {
        /* netResults[i] corresponds to jointpoints[i] */
        for(int i = 0; i < networkResults.Length; i++)
        {
            jointPoints[i].Now3D = networkResults[i].position;
        }

        foreach (var jp in jointPoints)
        {
            jp.Pos3D = jp.PrevPos3D * 0.1f + jp.Now3D * 0.9f;
            jp.PrevPos3D = jp.Pos3D;
        }

        for (int i = 0; i < (int)JointIndex.COUNT; i++)
        {
            Debug.Log(jointPoints[i].Now3D);
            //Debug.Log(jointPoints[i].confidence);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        jointPoints = new JointPoint[(int)JointIndex.COUNT];
        for (var i = 0; i < (int)JointIndex.COUNT; i++)
            jointPoints[i] = new JointPoint();
            
        //choose active model
        GameObject ModelObject;
        Renderer[] renderers1, renderers2, renderers3;
        switch(activeModel) {
          case 1:
            ModelObject = ModelObject1;
            renderers1 = ModelObject2.GetComponentsInChildren<Renderer>();
            renderers2 = ModelObject3.GetComponentsInChildren<Renderer>();
            renderers3 = ModelObject4.GetComponentsInChildren<Renderer>();
            break;
          case 2:
            ModelObject = ModelObject2;
            renderers1 = ModelObject1.GetComponentsInChildren<Renderer>();
            renderers2 = ModelObject3.GetComponentsInChildren<Renderer>();
            renderers3 = ModelObject4.GetComponentsInChildren<Renderer>();
            break;
          case 3:
            ModelObject = ModelObject3;
            renderers1 = ModelObject2.GetComponentsInChildren<Renderer>();
            renderers2 = ModelObject1.GetComponentsInChildren<Renderer>();
            renderers3 = ModelObject4.GetComponentsInChildren<Renderer>();
            break;
          case 4:
            ModelObject = ModelObject4;
            renderers1 = ModelObject2.GetComponentsInChildren<Renderer>();
            renderers2 = ModelObject3.GetComponentsInChildren<Renderer>();
            renderers3 = ModelObject1.GetComponentsInChildren<Renderer>();
            break;
          default:
            ModelObject = ModelObject1;
            renderers1 = ModelObject2.GetComponentsInChildren<Renderer>();
            renderers2 = ModelObject3.GetComponentsInChildren<Renderer>();
            renderers3 = ModelObject4.GetComponentsInChildren<Renderer>();
            break;
        }
          foreach (Renderer r in renderers1)
          {
            r.enabled = false;
          }
          foreach (Renderer r in renderers2)
          {
            r.enabled = false;
          }
          foreach (Renderer r in renderers3)
          {
            r.enabled = false;
          }

          //

        anim = ModelObject.GetComponent<Animator>();

        // Right Arm
        jointPoints[(int)JointIndex.RIGHT_SHOULDER].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[(int)JointIndex.RIGHT_ELBOW].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[(int)JointIndex.RIGHT_WRIST].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[(int)JointIndex.RIGHT_THUMB].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[(int)JointIndex.RIGHT_MID].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

        // Left Arm
        jointPoints[(int)JointIndex.LEFT_SHOULDER].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[(int)JointIndex.LEFT_ELBOW].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[(int)JointIndex.LEFT_WRIST].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[(int)JointIndex.LEFT_THUMB].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[(int)JointIndex.LEFT_MID].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

        // Face
        jointPoints[(int)JointIndex.LEFT_EAR].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[(int)JointIndex.LEFT_EYE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[(int)JointIndex.RIGHT_EAR].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[(int)JointIndex.RIGHT_EYE].Transform = anim.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[(int)JointIndex.NOSE].Transform = Nose.transform;

        // Right Leg
        jointPoints[(int)JointIndex.RIGHT_HIP].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[(int)JointIndex.RIGHT_KNEE].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[(int)JointIndex.RIGHT_ANKLE].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[(int)JointIndex.RIGHT_TOE].Transform = anim.GetBoneTransform(HumanBodyBones.RightToes);

        // Left Leg
        jointPoints[(int)JointIndex.LEFT_HIP].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[(int)JointIndex.LEFT_KNEE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[(int)JointIndex.LEFT_ANKLE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[(int)JointIndex.LEFT_TOE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftToes);

        // etc
        jointPoints[(int)JointIndex.ABDOMEN_UPPER].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[(int)JointIndex.HIP].Transform = anim.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[(int)JointIndex.HEAD].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[(int)JointIndex.NECK].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[(int)JointIndex.SPINE].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);

        // Child Settings
        // Right Arm
        jointPoints[(int)JointIndex.RIGHT_SHOULDER].Child = jointPoints[(int)JointIndex.RIGHT_ELBOW];
        jointPoints[(int)JointIndex.RIGHT_ELBOW].Child = jointPoints[(int)JointIndex.RIGHT_WRIST];
        jointPoints[(int)JointIndex.RIGHT_ELBOW].Parent = jointPoints[(int)JointIndex.RIGHT_SHOULDER];

        // Left Arm
        jointPoints[(int)JointIndex.LEFT_SHOULDER].Child = jointPoints[(int)JointIndex.LEFT_ELBOW];
        jointPoints[(int)JointIndex.LEFT_ELBOW].Child = jointPoints[(int)JointIndex.LEFT_WRIST];
        jointPoints[(int)JointIndex.LEFT_ELBOW].Parent = jointPoints[(int)JointIndex.LEFT_SHOULDER];

        // Right Leg
        jointPoints[(int)JointIndex.RIGHT_HIP].Child = jointPoints[(int)JointIndex.RIGHT_KNEE];
        jointPoints[(int)JointIndex.RIGHT_KNEE].Child = jointPoints[(int)JointIndex.RIGHT_ANKLE];
        jointPoints[(int)JointIndex.RIGHT_ANKLE].Child = jointPoints[(int)JointIndex.RIGHT_TOE];
        jointPoints[(int)JointIndex.RIGHT_ANKLE].Parent = jointPoints[(int)JointIndex.RIGHT_KNEE];

        // Left Leg
        jointPoints[(int)JointIndex.LEFT_HIP].Child = jointPoints[(int)JointIndex.LEFT_KNEE];
        jointPoints[(int)JointIndex.LEFT_KNEE].Child = jointPoints[(int)JointIndex.LEFT_ANKLE];
        jointPoints[(int)JointIndex.LEFT_ANKLE].Child = jointPoints[(int)JointIndex.LEFT_TOE];
        //jointPoints[(int)JointIndex.LEFT_ANKLE].Parent = jointPoints[(int)JointIndex.LEFT_KNEE];

        // etc
        jointPoints[(int)JointIndex.SPINE].Child = jointPoints[(int)JointIndex.NECK];
        jointPoints[(int)JointIndex.NECK].Child = jointPoints[(int)JointIndex.HEAD];

        useSkeleton = ShowSkeleton;
        if (useSkeleton)
        {
            // Line Child Settings
            // Right Arm
            AddSkeleton(JointIndex.RIGHT_SHOULDER, JointIndex.RIGHT_ELBOW);
            AddSkeleton(JointIndex.RIGHT_ELBOW, JointIndex.RIGHT_WRIST);
            AddSkeleton(JointIndex.RIGHT_WRIST, JointIndex.RIGHT_THUMB);
            AddSkeleton(JointIndex.RIGHT_WRIST, JointIndex.RIGHT_MID);

            // Left Arm
            AddSkeleton(JointIndex.LEFT_SHOULDER, JointIndex.LEFT_ELBOW);
            AddSkeleton(JointIndex.LEFT_ELBOW, JointIndex.LEFT_WRIST);
            AddSkeleton(JointIndex.LEFT_WRIST, JointIndex.LEFT_THUMB);
            AddSkeleton(JointIndex.LEFT_WRIST, JointIndex.LEFT_MID);

            // Fase
            AddSkeleton(JointIndex.LEFT_EAR, JointIndex.NOSE);
            AddSkeleton(JointIndex.RIGHT_EAR, JointIndex.NOSE);

            // Right Leg
            AddSkeleton(JointIndex.RIGHT_HIP, JointIndex.RIGHT_KNEE);
            AddSkeleton(JointIndex.RIGHT_KNEE, JointIndex.RIGHT_ANKLE);
            AddSkeleton(JointIndex.RIGHT_ANKLE, JointIndex.RIGHT_TOE);

            // Left Leg
            AddSkeleton(JointIndex.LEFT_HIP, JointIndex.LEFT_KNEE);
            AddSkeleton(JointIndex.LEFT_KNEE, JointIndex.LEFT_ANKLE);
            AddSkeleton(JointIndex.LEFT_ANKLE, JointIndex.LEFT_TOE);

            // etc
            AddSkeleton(JointIndex.SPINE, JointIndex.NECK);
            AddSkeleton(JointIndex.NECK, JointIndex.HEAD);
            AddSkeleton(JointIndex.HEAD, JointIndex.NOSE);
            AddSkeleton(JointIndex.NECK, JointIndex.RIGHT_SHOULDER);
            AddSkeleton(JointIndex.NECK, JointIndex.LEFT_SHOULDER);
            AddSkeleton(JointIndex.RIGHT_HIP, JointIndex.RIGHT_SHOULDER);
            AddSkeleton(JointIndex.LEFT_HIP, JointIndex.LEFT_SHOULDER);
            AddSkeleton(JointIndex.RIGHT_SHOULDER, JointIndex.ABDOMEN_UPPER);
            AddSkeleton(JointIndex.LEFT_SHOULDER, JointIndex.ABDOMEN_UPPER);
            AddSkeleton(JointIndex.RIGHT_HIP, JointIndex.ABDOMEN_UPPER);
            AddSkeleton(JointIndex.LEFT_HIP, JointIndex.ABDOMEN_UPPER);
            AddSkeleton(JointIndex.LEFT_HIP, JointIndex.RIGHT_HIP);
        }

        // Set Inverse
        var forward = TriangleNormal(jointPoints[(int)JointIndex.HIP].Transform.position,
                                     jointPoints[(int)JointIndex.LEFT_HIP].Transform.position,
                                     jointPoints[(int)JointIndex.RIGHT_HIP].Transform.position);
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }

            if (jointPoint.Child != null)
            {
                jointPoint.Inverse = GetInverse(jointPoint, jointPoint.Child, forward);
                jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
            }
        }

        var hip = jointPoints[(int)JointIndex.HIP];
        initPosition = jointPoints[(int)JointIndex.HIP].Transform.position;
        hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        hip.InverseRotation = hip.Inverse * hip.InitRotation;

        // For Head Rotation
        var head = jointPoints[(int)JointIndex.HEAD];
        head.InitRotation = jointPoints[(int)JointIndex.HEAD].Transform.rotation;
        var gaze = jointPoints[(int)JointIndex.NOSE].Transform.position - jointPoints[(int)JointIndex.HEAD].Transform.position;
        head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
        head.InverseRotation = head.Inverse * head.InitRotation;

        var lHand = jointPoints[(int)JointIndex.LEFT_WRIST];
        var lf = TriangleNormal(lHand.Pos3D,
                                jointPoints[(int)JointIndex.LEFT_MID].Pos3D,
                                jointPoints[(int)JointIndex.LEFT_THUMB].Pos3D);
        lHand.InitRotation = lHand.Transform.rotation;
        lHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[(int)JointIndex.LEFT_THUMB].Transform.position -
                        jointPoints[(int)JointIndex.LEFT_MID].Transform.position, lf));
        lHand.InverseRotation = lHand.Inverse * lHand.InitRotation;

        var rHand = jointPoints[(int)JointIndex.RIGHT_WRIST];
        var rf = TriangleNormal(rHand.Pos3D,
                                jointPoints[(int)JointIndex.RIGHT_THUMB].Pos3D,
                                jointPoints[(int)JointIndex.RIGHT_MID].Pos3D);
        rHand.InitRotation = rHand.Transform.rotation;
        rHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[(int)JointIndex.RIGHT_THUMB].Transform.position -
                        jointPoints[(int)JointIndex.RIGHT_MID].Transform.position, rf));
        rHand.InverseRotation = rHand.Inverse * rHand.InitRotation;
    }

    // Move in z direction
    private float centerTall = 224 * 0.75f;
    private float tall = 224 * 0.75f;
    private float prevTall = 224 * 0.75f;
    public float ZScale = 0.8f;

    void Update()
    {
        if(jointPoints == null)
        {
            return;
        }

        // calculate movement range of z-coordinate from height
        var t1 = Vector3.Distance(jointPoints[(int)JointIndex.HEAD].Pos3D,
                                  jointPoints[(int)JointIndex.NECK].Pos3D);
        var t2 = Vector3.Distance(jointPoints[(int)JointIndex.NECK].Pos3D,
                                  jointPoints[(int)JointIndex.SPINE].Pos3D);
        var pm = (jointPoints[(int)JointIndex.RIGHT_HIP].Pos3D
                  + jointPoints[(int)JointIndex.LEFT_HIP].Pos3D) / 2f;
        var t3 = Vector3.Distance(jointPoints[(int)JointIndex.SPINE].Pos3D, pm);
        var t4r = Vector3.Distance(jointPoints[(int)JointIndex.RIGHT_HIP].Pos3D,
                                   jointPoints[(int)JointIndex.RIGHT_KNEE].Pos3D);
        var t4l = Vector3.Distance(jointPoints[(int)JointIndex.LEFT_HIP].Pos3D,
                                   jointPoints[(int)JointIndex.LEFT_KNEE].Pos3D);
        var t4 = (t4r + t4l) / 2f;
        var t5r = Vector3.Distance(jointPoints[(int)JointIndex.RIGHT_KNEE].Pos3D,
                                   jointPoints[(int)JointIndex.RIGHT_ANKLE].Pos3D);
        var t5l = Vector3.Distance(jointPoints[(int)JointIndex.LEFT_KNEE].Pos3D,
                                   jointPoints[(int)JointIndex.LEFT_ANKLE].Pos3D);
        var t5 = (t5r + t5l) / 2f;
        var t = t1 + t2 + t3 + t4 + t5;


        // Low pass filter in z direction
        tall = t * 0.7f + prevTall * 0.3f;
        prevTall = tall;

        if (tall == 0)
        {
            tall = centerTall;
        }
        var dz = (centerTall - tall) / centerTall * ZScale;

        // movement and rotation of center
        var forward = TriangleNormal(jointPoints[(int)JointIndex.HIP].Pos3D,
                                     jointPoints[(int)JointIndex.LEFT_HIP].Pos3D,
                                     jointPoints[(int)JointIndex.RIGHT_HIP].Pos3D);
        jointPoints[(int)JointIndex.HIP].Transform.position =
            jointPoints[(int)JointIndex.HIP].Pos3D * 0.005f +
            new Vector3(initPosition.x, initPosition.y, initPosition.z + dz);
        jointPoints[(int)JointIndex.HIP].Transform.rotation = Quaternion.LookRotation(forward)
            * jointPoints[(int)JointIndex.HIP].InverseRotation;

        // rotate each of bones
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
            }
        }

        // Head Rotation
        var gaze = jointPoints[(int)JointIndex.NOSE].Pos3D
            - jointPoints[(int)JointIndex.HEAD].Pos3D;
        var f = TriangleNormal(jointPoints[(int)JointIndex.NOSE].Pos3D,
                               jointPoints[(int)JointIndex.RIGHT_EAR].Pos3D,
                               jointPoints[(int)JointIndex.LEFT_EAR].Pos3D);
        var head = jointPoints[(int)JointIndex.HEAD];
        head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;

        // Wrist rotation
        var lHand = jointPoints[(int)JointIndex.LEFT_WRIST];
        var lf = TriangleNormal(lHand.Pos3D,
                                jointPoints[(int)JointIndex.LEFT_MID].Pos3D,
                                jointPoints[(int)JointIndex.LEFT_THUMB].Pos3D);
        lHand.Transform.rotation = Quaternion.LookRotation(jointPoints[(int)JointIndex.LEFT_THUMB].Pos3D -
                                   jointPoints[(int)JointIndex.LEFT_MID].Pos3D, lf) * lHand.InverseRotation;

        var rHand = jointPoints[(int)JointIndex.RIGHT_WRIST];
        var rf = TriangleNormal(rHand.Pos3D,
                                jointPoints[(int)JointIndex.RIGHT_THUMB].Pos3D,
                                jointPoints[(int)JointIndex.RIGHT_MID].Pos3D);
        rHand.Transform.rotation = Quaternion.LookRotation(jointPoints[(int)JointIndex.RIGHT_THUMB].Pos3D -
                                   jointPoints[(int)JointIndex.RIGHT_MID].Pos3D, rf) * rHand.InverseRotation;

        foreach (var sk in Skeletons)
        {
            var s = sk.start;
            var e = sk.end;

            sk.Line.SetPosition(0, new Vector3(s.Pos3D.x * SkeletonScale + SkeletonX, s.Pos3D.y * SkeletonScale + SkeletonY, s.Pos3D.z * SkeletonScale + SkeletonZ));
            sk.Line.SetPosition(1, new Vector3(e.Pos3D.x * SkeletonScale + SkeletonX, e.Pos3D.y * SkeletonScale + SkeletonY, e.Pos3D.z * SkeletonScale + SkeletonZ));
        }
    }

    /* Calculates the normal to the triangle defined by the 3 points */
    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    private Quaternion GetInverse(JointPoint p1, JointPoint p2, Vector3 forward)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.Transform.position - p2.Transform.position, forward));
    }

    private void AddSkeleton(JointIndex s, JointIndex e)
    {
        var sk = new Skeleton()
        {
            LineObject = new GameObject("Line"),
            start = jointPoints[(int)s],
            end = jointPoints[(int)e],
        };

        sk.Line = sk.LineObject.AddComponent<LineRenderer>();
        sk.Line.startWidth = 0.04f;
        sk.Line.endWidth = 0.01f;

        // define the number of vertex
        sk.Line.positionCount = 2;
        sk.Line.material = SkeletonMaterial;

        Skeletons.Add(sk);
    }
}
