using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    //public AnimateCharacter a;
    public GameObject ModelObject1;
    public GameObject ModelObject2;

    public void changeScene() {
      AnimateCharacter.abc = 1;
      //UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
      ModelObject1.SetActive (true);
      ModelObject2.SetActive (false);
    }
    public void goToMainMenu() {
      UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
