using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class CharacterController : MonoBehaviour
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

        // Make the character face the viewer.
        transform.LookAt(firstPersonCamera.transform);
    }

    public void CreateAnchor()
    {
        /*
        // Create the position of the anchor by raycasting a point towards
        // the top of the screen.
        Vector2 pos = new Vector2(Screen.width * .5f, Screen.height * .90f);
        Ray ray = firstPersonCamera.ScreenPointToRay(pos);
        Vector3 anchorPosition = ray.GetPoint(5f);

        // Create the anchor at that point.
        if (anchor != null)
        {
            DestroyObject(anchor);
        }
        */

        /*
        anchor = Session.CreateAnchor(
            Frame.getCamera().getPose()
            .compose(Pose.makeTranslation(0, 0, -1f))
            .extractTranslation());
        */

        /*
        anchor = Session.CreateAnchor(
            new Pose(anchorPosition, Quaternion.identity));
        */

        /*
        anchor = Session.CreateAnchor(
            anchorPosition
            .compose(Pose.makeTranslation(0, 0, -0.5))
            .extractTranslation());
        */

        transform.position = firstPersonCamera.transform.position +
            firstPersonCamera.transform.forward * distance;

        transform.SetParent(firstPersonCamera.transform);
        transform.localPosition = Vector3.forward * distance;

        // Attach the scoreboard to the anchor.
        //transform.position = anchorPosition;
        //transform.SetParent(anchor.transform);
        
        // Finally, enable the renderers.
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
    }
}
