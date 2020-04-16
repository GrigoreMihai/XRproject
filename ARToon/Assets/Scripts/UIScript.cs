using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        GameObject myUI = new GameObject();
        myUI.transform.SetParent(canvas.transform);

        Text myText = myUI.AddComponent<Text>();
        myText.text = "Go!";

        /*
        DefaultControls.Resources uiResources = new DefaultControls.Resources();
        //Set the Panel Background Image someBgSprite;
        uiResources.background = someBgSprite;
        GameObject uiPanel = DefaultControls.CreatePanel(uiResources);
        uiPanel.transform.SetParent(canvas.transform, false);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
