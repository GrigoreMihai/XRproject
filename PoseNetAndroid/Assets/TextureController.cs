using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class TextureController : MonoBehaviour
{
    public Camera firstPersonCamera;
    private Anchor anchor;
    float distance = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // The tracking state must be FrameTrackingState.Tracking
        // in order to access the Frame.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
    }

    public void CreateAnchor()
    {
        transform.position = firstPersonCamera.transform.position +
            firstPersonCamera.transform.forward * distance;

        transform.SetParent(firstPersonCamera.transform);
        transform.localPosition = Vector3.forward * distance;

        // Finally, enable the renderers.
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
    }
}
