using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    //public AnimateCharacter a;
    public GameObject ModelObject1;
    public GameObject ModelObject2;
    public GameObject ModelObject3;
    public GameObject ModelObject4;

    public void activateUnity() {
      AnimateCharacter.activeModel = 1;

      ModelObject1.SetActive (true);
      ModelObject2.SetActive (false);
      ModelObject3.SetActive (false);
      ModelObject4.SetActive (false);
    }
    public void activateTait() {
      AnimateCharacter.activeModel = 2;

      ModelObject2.SetActive (true);
      ModelObject3.SetActive (false);
      ModelObject1.SetActive (false);
      ModelObject4.SetActive (false);
    }
    public void activateYuna() {
      AnimateCharacter.activeModel = 3;

      ModelObject3.SetActive (true);
      ModelObject2.SetActive (false);
      ModelObject1.SetActive (false);
      ModelObject4.SetActive (false);
    }
    public void activateAsobi() {
      AnimateCharacter.activeModel = 4;

      ModelObject4.SetActive (true);
      ModelObject2.SetActive (false);
      ModelObject1.SetActive (false);
      ModelObject3.SetActive (false);
    }
    public void goToMainMenu() {
      UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
