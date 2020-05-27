using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TensorFlowLite
{
    [System.Serializable]
    public struct NetworkResult
    {
        public Vector3 position;// = new Vector3();
        public float confidence;
    }

    public class NNRunner : BaseImagePredictor<float>
    {
        private float[,,] heatmap3D; // 24 * 14 * 14 * 14
        private float[,,] offset3D; // 24 * 14 * 14 * 14 * 3

        private NetworkResult[] results;

        public NNRunner(string modelFile) : base(modelFile)
        {
            /* Allocate the heatmap3d and offset tensors */
            var heatmapShape = interpreter.GetOutputTensorInfo(2).shape;
            var offsetShape = interpreter.GetOutputTensorInfo(3).shape;
            heatmap3D = new float[heatmapShape[1], heatmapShape[2], heatmapShape[3]];
            offset3D = new float[offsetShape[1], offsetShape[2], offsetShape[3]];

            /* Allocate the results vector */
            results = new NetworkResult[(int)JointIndex.COUNT];
            for (int i = 0; i < (int)JointIndex.COUNT; i++)
            {
                results[i] = new NetworkResult();
            }
        }

        public override void Invoke(Texture inputTexture)
        {
            //const float OFFSET = 128f;
            //const float SCALE = 1f / 128f;
            //const float OFFSET = 0f;
            //const float SCALE = 1f / 255f;
            //ToTensor(inputTexture, inputs, OFFSET, SCALE);
            ToTensor(inputTexture, inputs);

            interpreter.SetInputTensorData(0, inputs);
            interpreter.Invoke();
            interpreter.GetOutputTensorData(2, heatmap3D);
            interpreter.GetOutputTensorData(3, offset3D);
        }

        static void CalculateExtraJointPoints(NetworkResult[] results)
        {
            // neck location
            results[(int)JointIndex.NECK].position =
                (results[(int)JointIndex.LEFT_SHOULDER].position +
                 results[(int)JointIndex.RIGHT_SHOULDER].position) / 2f;
            results[(int)JointIndex.NECK].confidence =
                Mathf.Min(results[(int)JointIndex.LEFT_SHOULDER].confidence,
                          results[(int)JointIndex.RIGHT_SHOULDER].confidence);
            // head location
            var betweenEars = (results[(int)JointIndex.LEFT_EAR].position +
                               results[(int)JointIndex.RIGHT_EAR].position) / 2f;
            var headVect = (betweenEars - results[(int)JointIndex.NECK].position);
            headVect = Vector3.Normalize(headVect);
            var noseVect = (results[(int)JointIndex.NOSE].position
                            - results[(int)JointIndex.NECK].position);
            results[(int)JointIndex.HEAD].position = results[(int)JointIndex.NECK].position
                + headVect * (Vector3.Dot(headVect, noseVect));
            results[(int)JointIndex.HEAD].confidence =
                Mathf.Min(
                          Mathf.Min(results[(int)JointIndex.LEFT_EAR].confidence,
                                    results[(int)JointIndex.RIGHT_EAR].confidence),
                          Mathf.Min(results[(int)JointIndex.NOSE].confidence,
                                    results[(int)JointIndex.NECK].confidence)
                          );
            var lc = (results[(int)JointIndex.LEFT_HIP].position +
                      results[(int)JointIndex.RIGHT_HIP].position) / 2f;
            var lcConf = Mathf.Min(results[(int)JointIndex.LEFT_HIP].confidence,
                                   results[(int)JointIndex.RIGHT_HIP].confidence);
            // spine location; this is given by the network, but named ABDOMEN_UPPER
            // Calculate spine location
            results[(int)JointIndex.SPINE].position = results[(int)JointIndex.ABDOMEN_UPPER].position;
            results[(int)JointIndex.SPINE].confidence = results[(int)JointIndex.ABDOMEN_UPPER].confidence;

            // results[(int)JointIndex.SPINE].position
            //     = (results[(int)JointIndex.NECK].position + lc) / 2f;
            // results[(int)JointIndex.SPINE].confidence =
            //     Mathf.Min(results[(int)JointIndex.NECK].confidence, lcConf);

            // hip location
            results[(int)JointIndex.HIP].position
                = (results[(int)JointIndex.SPINE].position + lc) / 2f;
            results[(int)JointIndex.HIP].confidence =
                Mathf.Min(results[(int)JointIndex.SPINE].confidence, lcConf);
        }

        /* Uses the offset3D tensor to calculate the positions of the joints
           predicted by the network. X, Y, Z represent the position with the
           maximum score in heatmap3D associated with the given joint */
        void CalculatePredictedPosition(int joint, int X, int Y, int Z)
        {
            const int predictedJointsCount = 24;
            int spatialSize = heatmap3D.GetLength(0); // this should be 14
            var xpos = (float)X + offset3D[Y, X, joint * spatialSize + Z];
            var ypos = (float)Y + offset3D[Y, X, (joint + predictedJointsCount) * spatialSize + Z];
            var zpos = (float)Z + offset3D[Y, X, (joint + 2 * predictedJointsCount) * spatialSize + Z];
            var inputSize = inputs.GetLength(0);
            var halfInputSize = (float)inputSize / 2f;
            var imageScale = (float)inputSize / (float)spatialSize; // is 448 / 28 = 16f
            results[joint].position.x = (xpos + 0.5f) * imageScale - halfInputSize;
            results[joint].position.y = halfInputSize - (ypos + 0.5f) * imageScale;
            results[joint].position.z = (zpos + 0.5f) * imageScale - halfInputSize;
        }

        void GetNetworkResults()
        {
            const int predictedJointsCount = 24;
            int spatialSize = heatmap3D.GetLength(0); // this should be 28
            for (var j = 0; j < predictedJointsCount; j++)
            {
                /* the j iterates through the 3rd dimension (the one with 24) */
                var maxXIndex = 0;
                var maxYIndex = 0;
                var maxZIndex = 0;
                results[j].confidence = 0.0f;
                /* We do this multiplication because the C dimension actually packs
                   2 dimensions */
                var jointOffset = j * spatialSize;
                /* now iterate through the height and width dimensions (1st and 2nd) */
                for (var y = 0; y < spatialSize; y++)
                {
                    for (var x = 0; x < spatialSize; x++)
                    {
                        for (var z = 0; z < spatialSize; z++)
                        {
                            float score = heatmap3D[y, x, jointOffset + z];
                            if (score > results[j].confidence)
                            {
                                results[j].confidence = score;
                                maxXIndex = x;
                                maxYIndex = y;
                                maxZIndex = z;
                            }
                        }
                    }
                }

                CalculatePredictedPosition(j, maxXIndex, maxYIndex, maxZIndex);
            }
        }

        /* This function has the same role as GetResults() from PoseNet and
           PredictPose() from barracuda */
        public NetworkResult[] GetResults()
        {
            /* First we calculate the points predicted by the network directly */
            GetNetworkResults();

            // Calculate the extra points here
            CalculateExtraJointPoints(results);

            //for (int i = 0; i < (int)JointIndex.COUNT; i++)
            //{
            //    Debug.Log(results[i].position);
            //    Debug.Log(results[i].confidence);
            //}

            // And give the caller acces to the results
            return results;
        }
    }
}