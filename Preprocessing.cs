using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class Preprocessing
    {
        public static Image<Gray, byte> enhanceImage(Image<Gray, byte> src) {
            Image<Gray, byte> result = src.Clone();

            // Median blur (3x3)
            CvInvoke.MedianBlur(src, result, 3);

            // Unsharp mask filter
            Image<Gray, byte> temp1 = result.Clone();
            Image<Gray, byte> smoothed = result.Clone();
            Image<Gray, byte> input = result.Clone();
            CvInvoke.GaussianBlur(result.Clone(), temp1, new Size(0, 0), 2);
            CvInvoke.GaussianBlur(temp1.Clone(), smoothed, new Size(0, 0), 2);
            CvInvoke.AddWeighted(input, 1.5, smoothed, -0.5, 0, result);

            // CLAHE
            CvInvoke.CLAHE(result.Clone(), 3.56, new Size(8, 8), result);

            // Average Filtering
            CvInvoke.Blur(result.Clone(), result, new Size(2,2), new Point(-1, -1));

            /*CvInvoke.FastNlMeansDenoising(result.Clone(), result, 3, 7, 21);
            */

            MCvScalar bor = new MCvScalar();
            CvInvoke.MorphologyEx(result.Clone(), result, Emgu.CV.CvEnum.MorphOp.Close, new Mat(), new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect101, bor);
            return result;
        }
    }
}
