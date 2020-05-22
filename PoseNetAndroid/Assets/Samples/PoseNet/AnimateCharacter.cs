using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlowLite;

public enum JointIndex
{
    NOSE,

    LEFT_EYE,
    RIGHT_EYE,

    LEFT_EAR,
    RIGHT_EAR,

    LEFT_SHOULDER,
    RIGHT_SHOULDER,

    LEFT_ELBOW,
    RIGHT_ELBOW,

    LEFT_WRIST,
    RIGHT_WRIST,

    LEFT_HIP,
    RIGHT_HIP,

    LEFT_KNEE,
    RIGHT_KNEE,

    LEFT_ANKLE,
    RIGHT_ANKLE,

    /* Calculated points */
    HIP,
    HEAD,
    NECK,
    SPINE,

    /* number of joints */
    COUNT
}


public class AnimateCharacter : MonoBehaviour
{

    [System.Serializable]
    public class JointPoint
    {
        public Vector3 Pos3D = new Vector3();

        // Bones
        public Transform Transform = null;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;

        public JointPoint Child = null;
        public JointPoint Parent = null;
    }


    // Joint position and bone
    private JointPoint[] jointPoints;

    private Vector3 initPosition; // Initial center position

    private Quaternion InitGazeRotation;
    private Quaternion gazeInverse;

    // UnityChan
    public GameObject ModelObject;
    public GameObject Nose;
    private Animator anim;

    /* Called by PoseNetSample.Start()  */
    public void SetNetworkResults(PoseNet.Result [] networkResults)
    {
        /* netResults[i] corresponds to jointpoints[i] */
        //var netResults = networkResults;
        for(int i = 0; i < networkResults.Length; i++)
        {
            jointPoints[i].Pos3D = networkResults[i].position;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        jointPoints = new JointPoint[(int)JointIndex.COUNT];
        for (var i = 0; i < (int)JointIndex.COUNT; i++)
            jointPoints[i] = new JointPoint();

        anim = ModelObject.GetComponent<Animator>();

        jointPoints[(int)JointIndex.NOSE].Transform = Nose.transform;
        jointPoints[(int)JointIndex.LEFT_EYE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[(int)JointIndex.RIGHT_EYE].Transform = anim.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[(int)JointIndex.LEFT_EAR].Transform = anim.GetBoneTransform(HumanBodyBones.Head);   
        jointPoints[(int)JointIndex.RIGHT_EAR].Transform = anim.GetBoneTransform(HumanBodyBones.Head);

        jointPoints[(int)JointIndex.LEFT_SHOULDER].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[(int)JointIndex.RIGHT_SHOULDER].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);

        jointPoints[(int)JointIndex.LEFT_ELBOW].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[(int)JointIndex.RIGHT_ELBOW].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);

        jointPoints[(int)JointIndex.LEFT_WRIST].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[(int)JointIndex.RIGHT_WRIST].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);

        jointPoints[(int)JointIndex.LEFT_HIP].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[(int)JointIndex.RIGHT_HIP].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

        jointPoints[(int)JointIndex.LEFT_KNEE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[(int)JointIndex.RIGHT_KNEE].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);

        jointPoints[(int)JointIndex.LEFT_ANKLE].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[(int)JointIndex.RIGHT_ANKLE].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);


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

        // Fase

        // Right Leg
        jointPoints[(int)JointIndex.RIGHT_HIP].Child = jointPoints[(int)JointIndex.RIGHT_KNEE];
        jointPoints[(int)JointIndex.RIGHT_KNEE].Child = jointPoints[(int)JointIndex.RIGHT_ANKLE];
        jointPoints[(int)JointIndex.RIGHT_ANKLE].Parent = jointPoints[(int)JointIndex.RIGHT_KNEE];

        // Left Leg
        jointPoints[(int)JointIndex.LEFT_HIP].Child = jointPoints[(int)JointIndex.LEFT_KNEE];
        jointPoints[(int)JointIndex.LEFT_KNEE].Child = jointPoints[(int)JointIndex.LEFT_ANKLE];
        jointPoints[(int)JointIndex.LEFT_ANKLE].Parent = jointPoints[(int)JointIndex.LEFT_KNEE];

        // etc
        jointPoints[(int)JointIndex.SPINE].Child = jointPoints[(int)JointIndex.NECK];
        jointPoints[(int)JointIndex.NECK].Child = jointPoints[(int)JointIndex.HEAD];

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
}
