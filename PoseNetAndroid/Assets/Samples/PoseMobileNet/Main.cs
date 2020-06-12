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
    private WebCamTexture webcamTexture;
    NNRunner network;
    Vector3[] corners = new Vector3[4];

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

    private const int inputImageSize = 224;

    void Start()
    {
        network = NNRunner.Instance;

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

        results = new NetworkResult[(int)JointIndex.COUNT];
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        network?.Dispose();
    }

    void RunNetwork(Texture2D texture)
    {
        texture.filterMode = FilterMode.Trilinear;
        texture.Apply(true);

        network.Invoke(texture);
        results = network.GetResults();

        animateCharacter.SetNetworkResults(results);
    }

    void Update()
    {
        Color32[] color32 = webcamTexture.GetPixels32();
        texture.SetPixels32(color32);
        texture.Apply();


        StartCoroutine("RunNetwork", texture);
    }

}
