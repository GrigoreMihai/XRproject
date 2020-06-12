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
            NNRunner runner = NNRunner.Instance;
            Texture2D input = new Texture2D(224, 224, TextureFormat.RGBA32, false);
            runner.Invoke(input);
            var results = runner.GetResults();
            Assert.That((int)results.Length, Is.EqualTo((int)JointIndex.COUNT));
        }

        [Test]
        public void NetworkInputSizeTest()
        {
            var converter = new TextureToTensor();
            var options = new TextureToTensor.ResizeOptions()
                {
                    aspectMode = TextureToTensor.AspectMode.Fill,
                    rotationDegree = 180,
                    flipX = false,
                    flipY = false,
                    width = 320,
                    height = 140,
                };
            var texture = new Texture2D(1080, 720, TextureFormat.RGBA32, false);
            var resized = converter.Resize(texture, options);
            Assert.That(resized.width, Is.EqualTo(320));
            Assert.That(resized.height, Is.EqualTo(140));
        }

    }
}
