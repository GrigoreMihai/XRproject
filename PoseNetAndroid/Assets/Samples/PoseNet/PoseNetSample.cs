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
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseNet?.Dispose();
        glDrawer.OnDraw -= OnGLDraw;
    }

    void Update()
    {
        poseNet.Invoke(webcamTexture);
        results = poseNet.GetResults();

        Vector3 imageCenter = new Vector3(0.5f, 0.5f, 0);

        /* rotate the results */
        for(int i = 0; i < results.Length; i++)
        {
            results[i].position = imageRotation * (results[i].position - imageCenter) + imageCenter;
        }

        cameraView.material = poseNet.transformMat;
        /* arrange the camera view */
        cameraView.rectTransform.localEulerAngles = new Vector3(0,0,180);


        // set uv
        // cameraView.uvRect = TextureToTensor.GetUVRect(
        //     (float)webcamTexture.width / webcamTexture.height,
        //     1,
        //     TextureToTensor.AspectMode.Fill);
    }

    void OnGLDraw()
    {
        var rect = cameraView.GetComponent<RectTransform>();
        rect.GetWorldCorners(corners);
        Vector3 min = corners[0];
        Vector3 max = corners[2];

        GL.Begin(GL.LINES);

        GL.Color(Color.green);
        var connections = PoseNet.Connections;
        int len = connections.GetLength(0);
        for (int i = 0; i < len; i++)
        {
            var a = results[(int)connections[i, 0]];
            var b = results[(int)connections[i, 1]];
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
