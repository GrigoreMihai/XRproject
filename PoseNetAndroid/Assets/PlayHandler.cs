using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayHandler : MonoBehaviour
{
  public void changeScene() {
    UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
  }
  public void goToSettings() {
    UnityEngine.SceneManagement.SceneManager.LoadScene("ChooseCharacter");
  }
}
