namespace GoogleARCore.Examples.HelloAR
      {
      using System.Collections;
      using System.Collections.Generic;
      using UnityEngine;
      using UnityEngine.EventSystems;
      using GoogleARCore;
      using GoogleARCore.Examples.HelloAR;
      using UnityEngine.UI;
      using GoogleARCore.Examples.Common;

      #if UNITY_EDITOR
          // Set up touch input propagation while using Instant Preview in the editor.
          using Input = InstantPreviewInput;
      #endif

      public class movement : MonoBehaviour
      {
          private Animator Anim;
          public HelloARController helloARController;
          // Start is called before the first frame update
          void Start()
          {
              Anim = gameObject.GetComponent<Animator> ();
          }

          // Update is called once per frame
          void Update()
          {
            //   Touch touch;
            //   if(helloARController.maxOne == 1) {
            //     if (Input.touchCount >= 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
            //       if(Anim.GetBool("isWaving") == false)
            //         Anim.SetBool("isWaving",true);
            //       else
            //         Anim.SetBool("isWaving",false);
            //     }
            // }
          }
          public void changeState() {
            if(Anim.GetBool("isWaving") == false)
              Anim.SetBool("isWaving",true);
            else
              Anim.SetBool("isWaving",false);
          }
      }
}
