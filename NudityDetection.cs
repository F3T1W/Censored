using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Censored
{
    public class NudityDetection
    {
        private readonly InferenceSession _session;

        public NudityDetection(string modelPath)
        {
            _session = new InferenceSession(modelPath);
        }

        public float[] DetectNudity(Bitmap image)
        {
            // Convert Bitmap to byte array
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] imageBytes = ms.ToArray();

                // Create input tensor
                var inputTensor = new DenseTensor<float>(new[] { 1, 3, 320, 320 }); // Adjust dimensions as needed

                // Run inference
                var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_name", inputTensor) // Replace "input_name" with actual input name
            };

                using (var results = _session.Run(inputs))
                {
                    var outputTensor = results.First().AsTensor<float>();
                    return outputTensor.ToArray();
                }
            }
        }
    }
}