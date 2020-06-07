using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler2 : MonoBehaviour
{
  public void changeScene() {
    AnimateCharacter.abc = 2;
    UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
  }
}
