using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Recorder;

public class ControlCharacter : MonoBehaviour
{
    public GameObject freezeButton;
    public GameObject startButton;
    public GameObject startVidButton;
    public GameObject stopVidButton;
    public RecordManager recordManager;

    public void PressFreeze() {
      AnimateCharacter.bFrozen = false;
      freezeButton.SetActive(false);
      startButton.SetActive(true);
    }
    public void PressStart() {
      AnimateCharacter.bFrozen = true;
      freezeButton.SetActive(true);
      startButton.SetActive(false);
    }
    public void StartVid() {
      recordManager.StartRecord();
      startVidButton.SetActive(false);
      stopVidButton.SetActive(true);
    }
    public void StopVid() {
      recordManager.StopRecord();
      startVidButton.SetActive(true);
      stopVidButton.SetActive(false);
    }
}
