using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class VolumeSlider : MonoBehaviour
{
    public Slider Volume;
    public Text VolumeValue;

    void Start() {
      GameObject AudSou = GameObject.FindGameObjectsWithTag("music")[0];
      Volume.value = AudSou.GetComponent<AudioSource>().volume;
    }
    void Update () {
      //myMusic.volume = Volume.value;
      foreach (GameObject AudSou in GameObject.FindGameObjectsWithTag("music"))
      {
          AudSou.GetComponent<AudioSource>().volume = Volume.value;
      }
      string sVolume = (Volume.value * 100).ToString("0.00");
      VolumeValue.text = sVolume + "%";
   }
}
