using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler2 : MonoBehaviour
{
  public GameObject ModelObject1;
  public GameObject ModelObject2;
  public void changeScene() {
    AnimateCharacter.abc = 2;
    //UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    ModelObject1.SetActive (false);
    ModelObject2.SetActive (true);
  }
}
