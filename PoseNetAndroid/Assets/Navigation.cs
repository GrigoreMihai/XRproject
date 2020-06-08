using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    public GameObject CharacterPanel;
    public GameObject SoundPanel;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OpenCharacterPanel() {
      if(CharacterPanel != null && SoundPanel != null) {
        CharacterPanel.SetActive(true);
        SoundPanel.SetActive(false);
      }
    }
    public void OpenSoundPanel() {
      if(CharacterPanel != null && SoundPanel != null) {
        SoundPanel.SetActive(true);
        CharacterPanel.SetActive(false);
      }
    }
}
