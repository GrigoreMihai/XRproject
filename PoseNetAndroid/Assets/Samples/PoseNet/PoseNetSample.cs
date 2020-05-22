using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;

public class PoseNetSample : MonoBehaviour
{
    [SerializeField] string fileName = "posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite";
    [SerializeField] RawImage cameraView = null;
    [SerializeField] GLDrawer glDrawer = null;
    [SerializeField, Range(0f, 1f)] float threshold = 0.5f;


    WebCamTexture webcamTexture;
    PoseNet poseNet;
    Vector3[] corners = new Vector3[4];

    public PoseNet.Result[] results;

    public AnimateCharacter animateCharacter;

    Quaternion imageRotation = new Quaternion();


    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        poseNet = new PoseNet(path);

        // Init camera
        string cameraName = WebCamUtil.FindName();
        const int imageWidth = 640;
        const int imageHeight = 480;
        webcamTexture = new WebCamTexture(cameraName, imageWidth, imageHeight, 30);
        webcamTexture.Play();
        cameraView.texture = webcamTexture;

        glDrawer.OnDraw += OnGLDraw;

        imageRotation.eulerAngles = new Vector3(0, 0, 180);

        Vector3 test = new Vector3(1, 0, 0);
        Debug.Log(imageRotation * test);

        results = new PoseNet.Result[(int)JointIndex.COUNT];
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseNet?.Dispose();
        glDrawer.OnDraw -= OnGLDraw;
    }

    /* Copies the data from networkResults to results and calculates the extra positions. */
    void CalculateJointPoints(PoseNet.Result [] networkResults)
    {
        for(int i = 0; i < networkResults.Length; i++)
        {
            results[i].position = networkResults[i].position;
            results[i].confidence = networkResults[i].confidence;
        }

        /* now calculate the 4 extra points */
        // neck location
        results[(int)JointIndex.NECK].position =
            (results[(int)JointIndex.LEFT_SHOULDER].position +
             results[(int)JointIndex.RIGHT_SHOULDER].position) / 2f;
        results[(int)JointIndex.NECK].confidence =
            Mathf.Min(results[(int)JointIndex.LEFT_SHOULDER].confidence,
                      results[(int)JointIndex.RIGHT_SHOULDER].confidence);
        // head location
        var betweenEars = (results[(int)JointIndex.LEFT_EAR].position +
                           results[(int)JointIndex.RIGHT_EAR].position) / 2f;
        var headVect = (betweenEars - results[(int)JointIndex.NECK].position);
        headVect = Vector3.Normalize(headVect);
        var noseVect = (results[(int)JointIndex.NOSE].position
                        - results[(int)JointIndex.NECK].position);
        results[(int)JointIndex.HEAD].position = results[(int)JointIndex.NECK].position
            + headVect * (Vector3.Dot(headVect, noseVect));
        results[(int)JointIndex.HEAD].confidence = Mathf.Min(
                  Mathf.Min(results[(int)JointIndex.LEFT_EAR].confidence,
                            results[(int)JointIndex.RIGHT_EAR].confidence),
                  Mathf.Min(results[(int)JointIndex.NOSE].confidence,
                            results[(int)JointIndex.NECK].confidence)
                  );

        var lc = (results[(int)JointIndex.LEFT_HIP].position +
                  results[(int)JointIndex.RIGHT_HIP].position) / 2f;
        var lcConf = Mathf.Min(results[(int)JointIndex.LEFT_HIP].confidence,
                             results[(int)JointIndex.RIGHT_HIP].confidence);
        // spine location
        results[(int)JointIndex.SPINE].position
            = (results[(int)JointIndex.NECK].position + lc) / 2f;
        results[(int)JointIndex.SPINE].confidence =
            Mathf.Min(results[(int)JointIndex.NECK].confidence, lcConf);
        // hip location
        results[(int)JointIndex.HIP].position
            = (results[(int)JointIndex.SPINE].position + lc) / 2f;
        results[(int)JointIndex.HIP].confidence =
            Mathf.Min(results[(int)JointIndex.SPINE].confidence, lcConf);
    }

    void Update()
    {
        poseNet.Invoke(webcamTexture);
        var networkResults = poseNet.GetResults();
        //results = poseNet.GetResults();
        //results = networkResults;

        cameraView.material = poseNet.transformMat;
        /* arrange the camera view */
        cameraView.rectTransform.localEulerAngles = new Vector3(0,0,180);


        CalculateJointPoints(networkResults);

        Vector3 imageCenter = new Vector3(0.5f, 0.5f, 0);

        /* rotate the results */
        for(int i = 0; i < results.Length; i++)
        {
            results[i].position = imageRotation * (results[i].position - imageCenter) + imageCenter;
        }


        animateCharacter.SetNetworkResults(results);


        // set uv
        // cameraView.uvRect = TextureToTensor.GetUVRect(
        //     (float)webcamTexture.width / webcamTexture.height,
        //     1,
        //     TextureToTensor.AspectMode.Fill);
    }

    public static readonly JointIndex[,] Connections = new JointIndex[,]
    {
        // HEAD
        { JointIndex.LEFT_EAR, JointIndex.LEFT_EYE },
        { JointIndex.LEFT_EYE, JointIndex.NOSE },
        { JointIndex.NOSE, JointIndex.RIGHT_EYE },
        { JointIndex.RIGHT_EYE, JointIndex.RIGHT_EAR },
        // BODY
        { JointIndex.LEFT_HIP, JointIndex.LEFT_SHOULDER },
        { JointIndex.LEFT_ELBOW, JointIndex.LEFT_SHOULDER },
        { JointIndex.LEFT_ELBOW, JointIndex.LEFT_WRIST },
        { JointIndex.LEFT_HIP, JointIndex.LEFT_KNEE },
        { JointIndex.LEFT_KNEE, JointIndex.LEFT_ANKLE },
        { JointIndex.RIGHT_HIP, JointIndex.RIGHT_SHOULDER },
        { JointIndex.RIGHT_ELBOW, JointIndex.RIGHT_SHOULDER },
        { JointIndex.RIGHT_ELBOW, JointIndex.RIGHT_WRIST },
        { JointIndex.RIGHT_HIP, JointIndex.RIGHT_KNEE },
        { JointIndex.RIGHT_KNEE, JointIndex.RIGHT_ANKLE },
        { JointIndex.LEFT_SHOULDER, JointIndex.RIGHT_SHOULDER },
        { JointIndex.LEFT_HIP, JointIndex.RIGHT_HIP },
        //
        { JointIndex.HIP, JointIndex.LEFT_HIP },
        { JointIndex.HIP, JointIndex.RIGHT_HIP },
        { JointIndex.HIP, JointIndex.SPINE },
        { JointIndex.SPINE, JointIndex.LEFT_SHOULDER },
        { JointIndex.SPINE, JointIndex.RIGHT_SHOULDER },
        { JointIndex.SPINE, JointIndex.NECK },
        { JointIndex.NECK, JointIndex.HEAD }
    };

    void OnGLDraw()
    {
        var rect = cameraView.GetComponent<RectTransform>();
        rect.GetWorldCorners(corners);
        Vector3 min = corners[0];
        Vector3 max = corners[2];

        GL.Begin(GL.LINES);

        GL.Color(Color.green);
        int len = Connections.GetLength(0);
        for (int i = 0; i < len; i++)
        {
            var a = results[(int)Connections[i, 0]];
            var b = results[(int)Connections[i, 1]];
            if (a.confidence >= threshold && b.confidence >= threshold)
            {
                // GL.Vertex(Leap3(min, max, new Vector3(a.x, 1f - a.y, 0)));
                // GL.Vertex(Leap3(min, max, new Vector3(b.x, 1f - b.y, 0)));
                GL.Vertex(Leap3(min, max, new Vector3(a.position.x, 1f - a.position.y, 0)));
                GL.Vertex(Leap3(min, max, new Vector3(b.position.x, 1f - b.position.y, 0)));
            }
        }

        GL.End();
    }

    static Vector3 Leap3(in Vector3 a, in Vector3 b, in Vector3 t)
    {
        return new Vector3(
            Mathf.Lerp(a.x, b.x, t.x),
            Mathf.Lerp(a.y, b.y, t.y),
            Mathf.Lerp(a.z, b.z, t.z)
        );
    }

}
