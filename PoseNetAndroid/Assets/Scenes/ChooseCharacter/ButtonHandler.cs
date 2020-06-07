using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    //public AnimateCharacter a;
    public void changeScene() {
      AnimateCharacter.abc = 1;
      UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
