using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using TensorFlowLite;

namespace Tests
{
    public class NetworkTest
    {
        /*
          This test checks the integration between Unity and
          the TensorFlow Lite native plugin for Android by running
          a neural network with random inputs to see if any
          runtime error happens.
         */
        [Test]
        public void NetworkIntegrationTest()
        {
            NNRunner runner = new NNRunner("MobileNet3D2.tflite");
            Texture2D input = new Texture2D(224, 224, TextureFormat.RGBA32, false);
            runner.Invoke(input);
            runner.GetResults();
        }

    }
}
