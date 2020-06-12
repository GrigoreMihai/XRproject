using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TensorFlowLite;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class Main : MonoBehaviour
{
    //[SerializeField] string fileName = "converted_model.tflite";
    [SerializeField] string fileName = "MobileNet3D2.tflite";
    //[SerializeField] RawImage cameraView = null;
    //[SerializeField] GLDrawer glDrawer = null;
    [SerializeField, Range(0f, 1f)] float threshold = 0.5f;

    private WebCamTexture webcamTexture;
    NNRunner network;
    Vector3[] corners = new Vector3[4];

    private Texture networkInputs;

    public NetworkResult[] results;

    public AnimateCharacter animateCharacter;

    Quaternion imageRotation = new Quaternion();

    // For video play
    private RenderTexture videoTexture;

    private Texture2D texture;
    private int videoScreenWidth = 2560;
    private float videoWidth, videoHeight;
    private UnityEngine.Rect clipRect;
    public float clipScale;

    //public GameObject TextureObject;

    private const int inputImageSize = 224;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        network = new NNRunner(path);      

        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[0].name);

        GameObject videoScreen = GameObject.Find("VideoScreen");
        RawImage screen = videoScreen.GetComponent<RawImage>();
        var sd = screen.GetComponent<RectTransform>();
        screen.texture = webcamTexture;

        webcamTexture.Play();

        sd.sizeDelta = new Vector2(videoScreenWidth, (int)(videoScreenWidth * webcamTexture.height / webcamTexture.width));

        texture = new Texture2D(webcamTexture.width, webcamTexture.height);

        // Clip size
        videoWidth = texture.width;
        videoHeight = texture.height;
        float padWidth = (videoWidth < videoHeight) ? 0 : (videoHeight - videoWidth) / 2;
        float padHeight = (videoWidth < videoHeight) ? (videoWidth - videoHeight) / 2 : 0;
        if (clipScale == 0f) clipScale = 0.001f;
        var w = (videoWidth + padWidth * 2f) * clipScale;
        padWidth += w;
        padHeight += w;
        clipRect = new UnityEngine.Rect(-padWidth, -padHeight, videoWidth + padWidth * 2, videoHeight + padHeight * 2);

        /*
        networkInputs = new Texture[1];
        networkInputs[0] = webcamTexture;

        glDrawer.OnDraw += OnGLDraw;

        imageRotation.eulerAngles = new Vector3(0, 0, 180);

        Vector3 test = new Vector3(1, 0, 0);
        Debug.Log(imageRotation * test);
        */

        results = new NetworkResult[(int)JointIndex.COUNT];
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        network?.Dispose();
        //glDrawer.OnDraw -= OnGLDraw;
    }

    void RunNetwork(Texture2D texture)
    {
        ResizeTexture(texture);

        networkInputs = texture;

        network.Invoke(networkInputs);
        results = network.GetResults();

        animateCharacter.SetNetworkResults(results);
    }

    void Update()
    {
        Color32[] color32 = webcamTexture.GetPixels32();
        texture.SetPixels32(color32);
        texture.Apply();


        StartCoroutine("RunNetwork", texture);
        //results = network.GetResults();
        //results = networkResults;

        //cameraView.material = network.transformMat;
        /* arrange the camera view */
        //cameraView.rectTransform.localEulerAngles = new Vector3(0,0,180);

        //Vector3 imageCenter = new Vector3(0.5f, 0.5f, 0);

        /* rotate the results */
        /*
        for(int i = 0; i < results.Length; i++)
        {
            results[i].position = imageRotation * (results[i].position - imageCenter) + imageCenter;
        }
        */


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

    //void OnGLDraw()
    //{
    //    var rect = cameraView.GetComponent<RectTransform>();
    //    rect.GetWorldCorners(corners);
    //    Vector3 min = corners[0];
    //    Vector3 max = corners[2];

    //    GL.Begin(GL.LINES);

    //    GL.Color(Color.green);
    //    int len = Connections.GetLength(0);
    //    for (int i = 0; i < len; i++)
    //    {
    //        var a = results[(int)Connections[i, 0]];
    //        var b = results[(int)Connections[i, 1]];
    //        if (a.confidence >= threshold && b.confidence >= threshold)
    //        {
    //            // GL.Vertex(Leap3(min, max, new Vector3(a.x, 1f - a.y, 0)));
    //            // GL.Vertex(Leap3(min, max, new Vector3(b.x, 1f - b.y, 0)));
    //            GL.Vertex(Leap3(min, max, new Vector3(a.position.x, 1f - a.position.y, 0)));
    //            GL.Vertex(Leap3(min, max, new Vector3(b.position.x, 1f - b.position.y, 0)));
    //        }
    //    }

    //    GL.End();
    //}

    static Vector3 Leap3(in Vector3 a, in Vector3 b, in Vector3 t)
    {
        return new Vector3(
            Mathf.Lerp(a.x, b.x, t.x),
            Mathf.Lerp(a.y, b.y, t.y),
            Mathf.Lerp(a.z, b.z, t.z)
        );
    }

    private void ResizeTexture(Texture2D src)
    {
        float bbLeft = clipRect.xMin;
        float bbRight = clipRect.xMax;
        float bbTop = clipRect.yMin;
        float bbBottom = clipRect.yMax;
        float bbWidth = clipRect.width;
        float bbHeight = clipRect.height;

        float videoLongSide = (videoWidth > videoHeight) ? videoWidth : videoHeight;
        float videoShortSide = (videoWidth > videoHeight) ? videoHeight : videoWidth;
        float aspectWidth = videoWidth / videoShortSide;
        float aspectHeight = videoHeight / videoShortSide;

        float left = bbLeft;
        float right = bbRight;
        float top = bbTop;
        float bottom = bbBottom;

        left /= videoShortSide;
        right /= videoShortSide;
        top /= videoShortSide;
        bottom /= videoShortSide;

        src.filterMode = FilterMode.Trilinear;
        src.Apply(true);

        RenderTexture rt = new RenderTexture(224, 224, 32);
        Graphics.SetRenderTarget(rt);
        GL.LoadPixelMatrix(left, right, bottom, top);
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new UnityEngine.Rect(0, 0, aspectWidth, aspectHeight), src);

        //UnityEngine.Rect dstRect = new UnityEngine.Rect(0, 0, 224, 224);
        //Texture2D dst = (Texture2D)TextureObject.GetComponent<Renderer>().material.mainTexture;
        //dst.ReadPixels(dstRect, 0, 0, true);
        //Graphics.SetRenderTarget(null);
        Destroy(rt);

        //dst.Apply();

        //TextureObject.GetComponent<Renderer>().material.mainTexture = dst;       
    }
}
