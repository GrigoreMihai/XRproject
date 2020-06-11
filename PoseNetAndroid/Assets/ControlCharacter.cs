using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlCharacter : MonoBehaviour
{
    public GameObject freezeButton;
    public GameObject startButton;

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
}
