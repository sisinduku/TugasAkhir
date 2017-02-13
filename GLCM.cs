using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TugasAkhir
{
    class GLCM
    {
        private float epsilon = 0.0000001f;
        public Matrix<float> featureGLCM(Matrix<int> img) {
            Matrix<float> result = new Matrix<float>(1, 5);

            Matrix<float> sudut0 = calc_GLCM(img, 0);
            Matrix<float> sudut45 = calc_GLCM(img, 45);
            Matrix<float> sudut90 = calc_GLCM(img, 90);
            Matrix<float> sudut135 = calc_GLCM(img, 135);

            for (int i = 0; i < 5; i++) {
                result.Data[0, i] = (sudut0.Data[0, i] + sudut45.Data[0, i] + sudut90.Data[0, i] + sudut135.Data[0, i]) / 4;
            }
            return result;
        }

        public Matrix<float> calc_GLCM(Matrix<int> img, int sudut) {
            float variance = 0, mean = 0;
            float[] pxplusy = new float[512];
            float[] pxminy = new float[512];
            float[] px = new float[256];
            float[] py = new float[256];
            float asm = 0, contrast = 0, correlation = 0, homogenity = 0, IDM = 0, entropy = 0, sumAverage = 0, difAverage = 0;
            float sumEntropy = 0, difEntropy = 0, sumVariance = 0, difVariance = 0;
            int row = img.Rows, col = img.Cols;
            Matrix<float> gl = new Matrix<float>(256, 256);
            gl.SetZero();
            switch (sudut) {
                case 0:
                    //creating glcm matrix with 256 levels,radius=1 and in the 0 direction
                    for (int i = 0; i < row; i++) 
                        for (int j = 0; j < col - 1; j++)    
                            gl.Data[img.Data[i, j], img.Data[i, j + 1]] += 1;
                    break;
                case 45:
                    //creating glcm matrix with 256 levels,radius=1 and in the 45 direction
                    for (int i = 1; i < row; i++)
                        for (int j = 0; j < col - 1; j++)
                            gl.Data[img.Data[i, j], img.Data[i - 1, j + 1]] += 1;
                    break;
                case 90:
                    //creating glcm matrix with 256 levels,radius=1 and in the 90 direction
                    for (int i = 1; i < row; i++)
                        for (int j = 0; j < col; j++)
                            gl.Data[img.Data[i, j], img.Data[i - 1, j]] += 1;
                    break;
                case 135:
                    //creating glcm matrix with 256 levels,radius=1 and in the 135 direction
                    for (int i = 1; i < row; i++)
                        for (int j = 1; j < col; j++)
                            gl.Data[img.Data[i, j], img.Data[i - 1, j - 1]] += 1;
                    break;
            }
            
            // normalizing glcm matrix for parameter determination
            gl +=  gl.Transpose();
            gl = gl / CvInvoke.Sum(gl).V0;

            Matrix<float> result = new Matrix<float>(1, 5);
            
            // mean = calcMean(gl);
            //float correlation = calcCorrelation(gl, mean);
            //float[] sumdifVariance = calcSumnDifVariance(gl);
            //float[] sumdifEntropy = calcSumnDifEntropy(gl);
            //float sumAverage = calcSumnDifAverage(gl)[0];
            for (int i = 0; i < 256; i++) {

                for (int j = 0; j < 256; j++)
                {
                    asm += gl.Data[i, j] * gl.Data[i, j];            //finding parameters
                    contrast += ((i - j) * (i - j) * gl.Data[i, j]);
                    homogenity += (gl.Data[i, j] / (1 + Math.Abs(i - j)));
                    IDM += (gl.Data[i, j] / (1 + (i - j) * (i - j)));
                    if (gl.Data[i, j] != 0)
                        entropy += -(gl.Data[i, j] * (float)Math.Log10(Convert.ToDouble(gl.Data[i, j]) + epsilon));
                    //mean1 = mean1 + 0.5f * (i * gl.Data[i, j] + j * gl.Data[i, j]);
                    pxplusy[i + j] += gl.Data[i, j];
                    pxminy[Math.Abs(i - j)] += gl.Data[i, j];
                }
                mean += i * (float)CvInvoke.Sum(gl.GetRow(i)).V0;             
            }


            for (int i = 0; i < 256; i++)
            {
                float rowVal = 0;
                for (int j = 0; j < 256; j++)
                {
                    rowVal += gl.Data[i, j] * (float)Math.Pow((i - mean), 2);
                    px[i] += gl.Data[i, j];
                    py[i] += gl.Data[j, i];
                }
                variance += rowVal;
            }            

            for (int i = 0; i < gl.Rows * 2 - 2; i++)
            {
                sumAverage += i * pxplusy[i];
                difAverage += i * pxminy[i];
                sumEntropy += -(pxplusy[i] * (float)Math.Log10(pxplusy[i] + epsilon));
                difEntropy += -(pxminy[i] * (float)Math.Log10(pxminy[i] + epsilon));
            }

            for (int i = 0; i < gl.Rows * 2 - 2; i++)
            {
                sumVariance += (float)Math.Pow((1 - sumEntropy), 2) * pxplusy[i];
                difVariance += (float)Math.Pow((1 - difEntropy), 2) * pxplusy[i];
            }

            float hxy = entropy;
            float hxy1 = 0, hxy2 = 0, hx = 0, hy = 0;
            for (int i = 0; i < gl.Rows; i++)
            {
                float rowVal = 0;
                for (int j = 0; j < gl.Cols; j++)
                {
                    hxy1 += -(gl.Data[i, j] * (float)Math.Log10(px[i] * py[j] + epsilon));
                    hxy2 += -(px[i] * py[j] * (float)Math.Log10(px[i] * py[j] + epsilon));
                    rowVal += gl.Data[i, j] * ((i - mean) * (j - mean) / (float)Math.Sqrt(variance * variance));
                }
                hx += -(px[i] * (float)Math.Log10(px[i] + epsilon));
                hy += -(py[i] * (float)Math.Log10(py[i] + epsilon));
                correlation += rowVal;
            }
            float imc1 = (hxy - hxy1) / (Math.Max(hx, hy));
            float imc2 = (float)Math.Sqrt(1 - (float)Math.Exp(-2 * (hxy2 - hxy)));

            result.Data[0, 0] = asm;
            result.Data[0, 1] = contrast;
            result.Data[0, 2] = correlation;
            //result.Data[0, 3] = variance;
            result.Data[0, 3] = IDM;
            /*result.Data[0, 5] = sumAverage;
            result.Data[0, 6] = sumVariance;
            result.Data[0, 7] = sumEntropy;*/
            result.Data[0, 4] = entropy;
            /*result.Data[0, 9] = difVariance;
            result.Data[0, 10] = difEntropy;
            result.Data[0, 11] = imc1;
            result.Data[0, 12] = imc2;*/
            gl.Dispose();
            
            return result;
        }

        /*private float calcMean(Matrix<float> gl)
        {
            float mean = 0;
            for (int i = 0; i < gl.Rows; i++)
            {
                mean += i * (float)CvInvoke.Sum(gl.GetRow(i)).V0;
            }

            return mean;
        }*/
        /*private float calcVariance(Matrix<float> gl, float mean)
        {
            float variance = 0;
            for (int i = 0; i < gl.Rows; i++)
            {
                float rowVal = 0;
                for (int j = 0; j < gl.Cols; j++) {
                    rowVal = rowVal + gl.Data[i, j] * (float)Math.Pow((i - mean), 2);
                }
                variance += rowVal;
            }

            return variance;
        }*/

        /*private float calcCorrelation(Matrix<float> gl, float mean)
        {
            float variance = calcVariance(gl, mean);
            float correlation = 0;
            for (int i = 0; i < gl.Rows; i++)
            {
                float rowVal = 0;
                for (int j = 0; j < gl.Cols; j++)
                {
                    rowVal += gl.Data[i, j] * ((i - mean) * (j - mean) / (float)Math.Sqrt(variance * variance));
                }
                correlation += rowVal;
            }

            return correlation;
        }*/

        /*private float calcPxplusy(Matrix<float> gl, int k) {
            float result = 0;
            for (int i = 0; i < gl.Rows; i++) {
                for (int j = 0; j < gl.Cols; j++) {
                    if ((i + j) == k) {
                        result += gl.Data[i, j];
                    }
                }
            }

            return result;
        }

        private float calcPxminy(Matrix<float> gl, int k)
        {
            float result = 0;
            for (int i = 0; i < gl.Rows; i++)
            {
                for (int j = 0; j < gl.Cols; j++)
                {
                    if (Math.Abs((i - j)) == k)
                    {
                        result += gl.Data[i, j];
                    }
                }
            }

            return result;
        }*/

        /*private float[] calcSumnDifAverage(Matrix<float> gl)
        {
            float resultSum = 0;
            float resultDif = 0;
            for (int i = 0; i < gl.Rows * 2 - 2; i++)
            {
                resultSum += i * calcPxplusy(gl, i);
                resultDif += i * calcPxminy(gl, i);
            }
            float[] result = new float[2] { resultSum , resultDif };
            return result;
        }*/

        /*private float[] calcSumnDifVariance(Matrix<float> gl, float sumEntropy, float difEntropy)
        {
            float resultSum = 0;
            float resultDif = 0;
            for (int i = 0; i < gl.Rows*2 - 2; i++)
            {
                resultSum += (float)Math.Pow((1 - sumEntropy), 2) * calcPxplusy(gl, i);
                resultDif += (float)Math.Pow((1 - difEntropy), 2) * calcPxminy(gl, i);
            }
            float[] result = new float[2] { resultSum, resultDif };
            return result;
        }*/

        /*private float[] calcSumnDifEntropy(Matrix<float> gl) {
            float resultSum = 0;
            float resultDif = 0;
            for (int i = 0; i < gl.Rows * 2 - 2; i++) {
                resultSum += -(calcPxplusy(gl, i) * (float)Math.Log10(calcPxplusy(gl, i) + epsilon));
                resultDif += -(calcPxminy(gl, i) * (float)Math.Log10(calcPxminy(gl, i) + epsilon));
            }
            float[] result = new float[2] { resultSum, resultDif };
            return result;
        }*/

        /*private float[,] calcPxPy(Matrix<float> gl) {
            float[] px = new float[gl.Rows];
            float[] py = new float[gl.Cols];
            float[,] result = new float[gl.Rows, gl.Cols];
            for (int i = 0; i < gl.Rows; i++)
            {
                for (int j = 0; j < gl.Cols; j++)
                {
                    result[0, i] += gl.Data[i, j];
                    result[1, i] += gl.Data[j, i];
                }
            }
            return result;
        }*/

        /*private float[] calcIMC(Matrix<float> gl, float entropy) {
            float[,] pxpy = calcPxPy(gl);
            float hxy = entropy;
            float hxy1 = 0, hxy2 = 0, hx = 0, hy = 0;
            for (int i = 0; i < gl.Rows; i++) {
                for (int j = 0; j < gl.Cols; j++) {
                    hxy1 += -(gl.Data[i, j] * (float)Math.Log(pxpy[0, i] * pxpy[1, j], 2));
                    hxy2 += -(pxpy[0, i] * pxpy[1, j] * (float)Math.Log(pxpy[0, i] * pxpy[1, j], 2));
                }
                hx += -(pxpy[0, i] * (float)Math.Log(pxpy[0, i], 2));
                hy += -(pxpy[1, i] * (float)Math.Log(pxpy[1, i], 2));
            }
            float imc1 = (hxy - hxy1) / (Math.Max(hx, hy));
            float imc2 = (float)Math.Sqrt(1 - (float)Math.Exp(-2 * (hxy2 - hxy)));
            float[] result = new float[2] { imc1, imc2 };

            return result;
        }*/

        /*private float calcMCC(Matrix<float> gl) {
            float[,] pxpy = calcPxPy(gl);
            Matrix<float> q = new Matrix<float>(gl.Rows, gl.Cols);
            for (int i = 0; i < gl.Rows; i++) {
                for (int j = 0; j < gl.Cols; j++) {
                    for(int k = 0; k < gl.Cols; k++)
                    {
                        q.Data[i, j] += (gl.Data[i, k] * gl.Data[j, k]) / (pxpy[0, i] * pxpy[1, k]);
                    }
                }
            }
            Mat eigenValue = new Mat();
            CvInvoke.Eigen(q, eigenValue);
            Matrix<float> eigen = new Matrix<float>(1, eigenValue.Cols);
            eigenValue.CopyTo(eigen);
            return (float)Math.Sqrt(eigen.Data[0, 1]);
        }*/
    }
}
