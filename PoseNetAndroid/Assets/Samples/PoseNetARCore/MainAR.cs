using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TensorFlowLite;
using GoogleARCore;

public class MainAR : MonoBehaviour
{
    //[SerializeField] string fileName = "converted_model.tflite";
    [SerializeField] string fileName = "MobileNet3D2.tflite";
    //[SerializeField] RawImage cameraView = null;
    //[SerializeField] GLDrawer glDrawer = null;
    [SerializeField, Range(0f, 1f)] float threshold = 0.5f;

    //private WebCamTexture webcamTexture;
    public Camera webcamTexture;
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

    public GameObject TextureObject;

    private const int inputImageSize = 224;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        network = new NNRunner(path);

        //texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false, false);
        texture = null;

        // Clip size
        // videoWidth = texture.width;
        // videoHeight = texture.height;
        // float padWidth = (videoWidth < videoHeight) ? 0 : (videoHeight - videoWidth) / 2;
        // float padHeight = (videoWidth < videoHeight) ? (videoWidth - videoHeight) / 2 : 0;
        // if (clipScale == 0f) clipScale = 0.001f;
        // var w = (videoWidth + padWidth * 2f) * clipScale;
        // padWidth += w;
        // padHeight += w;
        // clipRect = new UnityEngine.Rect(-padWidth, -padHeight, videoWidth + padWidth * 2, videoHeight + padHeight * 2);

        results = new NetworkResult[(int)JointIndex.COUNT];
    }

    void OnDestroy()
    {
        //webcamTexture?.Stop();
        network?.Dispose();
        //glDrawer.OnDraw -= OnGLDraw;
    }

    void RunNetwork(Texture2D texture)
    {
        ResizeTexture(texture);

        networkInputs = texture;

        //network.Invoke(networkInputs);
        //results = network.GetResults();

        //animateCharacter.SetNetworkResults(results);
    }

    /*
    static int clamp(int val, int lower, int upper) {
        if(val < lower)
            return lower;
        if(val > upper)
            return upper;
        return val;
    }

    static (byte r, byte g, byte b) yuv2rgb(byte y, byte u, byte v)//, byte * buf)
    {
        var r = (int)(y + (1.37705f * (v-128)));
        var g = (int)(y + (0.698001f * (v-128)) - (0.337633f * (u-128)));
        var b = (int)(y + (1.732446f * (u-128)));
        return ((byte)clamp(r, 0, 255), (byte)clamp(g, 0, 255), (byte)clamp(b, 0, 255));
    }

    byte [] pixelConversionBuffer;

    bool getCameraTexture()
    {
        var image = Frame.CameraImage.AcquireCameraImageBytes();
        // if(!image.IsAvailable) {
        //     return false;
        // }
        if(texture == null) {
            texture = new Texture2D(image.Width, image.Height, TextureFormat.ARGB32, false, false);
            pixelConversionBuffer = new byte[image.Width*image.Height*4];

            videoWidth = texture.width;
            videoHeight = texture.height;
            float padWidth = (videoWidth < videoHeight) ? 0 : (videoHeight - videoWidth) / 2;
            float padHeight = (videoWidth < videoHeight) ? (videoWidth - videoHeight) / 2 : 0;
            if (clipScale == 0f) clipScale = 0.001f;
            var w = (videoWidth + padWidth * 2f) * clipScale;
            padWidth += w;
            padHeight += w;
            clipRect = new UnityEngine.Rect(-padWidth, -padHeight, videoWidth + padWidth * 2, videoHeight + padHeight * 2);
        }
        unsafe {
            byte * Y = (byte*)image.Y.ToPointer();
            byte * U = (byte*)image.U.ToPointer();
            byte * V = (byte*)image.V.ToPointer();
            for(uint y = 0; y < image.Height; y++) {
                uint halfy = y >> 1;
                long index = y * 4 * image.Width;
                for(uint x = 0; x < image.Width; x++) {
                    uint halfx = x >> 1;
                    byte yval = *(Y + y * image.YRowStride + x);
                    byte uval = *(U + halfy * image.UVRowStride + halfx * image.UVPixelStride);
                    byte vval = *(V + halfy * image.UVRowStride + halfx * image.UVPixelStride);

                    var color = yuv2rgb(yval, uval, vval);
                    pixelConversionBuffer[index+0] = 127;
                    pixelConversionBuffer[index+1] = color.r;
                    pixelConversionBuffer[index+2] = color.g;
                    pixelConversionBuffer[index+3] = color.b;

                    index += 4;
                }
            }
        }
        texture.LoadRawTextureData(pixelConversionBuffer);
        texture.Apply();
        return true;
    }
    */

    bool getCameraTexture()
    {
        var image = Frame.CameraImage.AcquireCameraImageBytes();
        if(!image.IsAvailable) {
            return false;
        }

        if (texture == null)
        {
            texture = new Texture2D(image.Width, image.Height, TextureFormat.R8, false, false);
        }

        int size = image.Width * image.Height;
        byte[] yBuff = new byte[size];
        System.Runtime.InteropServices.Marshal.Copy(image.Y, yBuff, 0, size);

        texture.LoadRawTextureData(yBuff);
        texture.Apply();

        return true;

        //return onImageAvailable(image.Width, image.Height, image.Y, image.Width * image.Height);
    }

    bool onImageAvailable(int width, int height, IntPtr pixelBuffer, int bufferSize)
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        byte[] bufferYUV = new byte[width * height * 3 / 2];
        bufferSize = width * height * 3 / 2;
        System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, bufferYUV, 0, bufferSize);
        Color color = new Color();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float Yvalue = bufferYUV[y * width + x];
                float Uvalue = bufferYUV[(y / 2) * (width / 2) + x / 2 + (width * height)];
                float Vvalue = bufferYUV[(y / 2) * (width / 2) + x / 2 + (width * height) + (width * height) / 4];
                color.r = Yvalue + (float)(1.37705 * (Vvalue - 128.0f));
                color.g = Yvalue - (float)(0.698001 * (Vvalue - 128.0f)) - (float)(0.337633 * (Uvalue - 128.0f));
                color.b = Yvalue + (float)(1.732446 * (Uvalue - 128.0f));

                color.r /= 255.0f;
                color.g /= 255.0f;
                color.b /= 255.0f;

                if (color.r < 0.0f)
                    color.r = 0.0f;
                if (color.g < 0.0f)
                    color.g = 0.0f;
                if (color.b < 0.0f)
                    color.b = 0.0f;

                if (color.r > 1.0f)
                    color.r = 1.0f;
                if (color.g > 1.0f)
                    color.g = 1.0f;
                if (color.b > 1.0f)
                    color.b = 1.0f;

                color.a = 1.0f;
                texture.SetPixel(width - 1 - x, y, color);
            } 
        }

        texture.Apply();
        return true;
    }

    void Update()
    {
        /*
        Color32[] color32 = webcamTexture.GetPixels32();
        texture.SetPixels32(color32);
        texture.Apply();
        */

        if(getCameraTexture()) {
            //StartCoroutine("RunNetwork", texture);
            RunNetwork(texture);
        }
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
        /*
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
        Graphics.DrawTexture(new UnityEngine.Rect(0, 0, 224, 224), src);

        UnityEngine.Rect dstRect = new UnityEngine.Rect(0, 0, 224, 224);
        Texture2D dst = (Texture2D)TextureObject.GetComponent<Renderer>().material.mainTexture;
        dst.ReadPixels(dstRect, 0, 0, true);
        Graphics.SetRenderTarget(null);
        Destroy(rt);

        dst.Apply();

        TextureObject.GetComponent<Renderer>().material.mainTexture = dst;
        */
        TextureObject.GetComponent<Renderer>().material.mainTexture = src;
    }
}
