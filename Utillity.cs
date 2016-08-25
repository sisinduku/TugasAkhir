using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class Utillity
    {
        public static double[] createArray(int value)
        {
            double[] result = new double[value];
            if (value % 2 == 1)
            {
                double batasBawah = -(value - 1) / 2;
                for (int i = 0; i < value; i++)
                {
                    result[i] = batasBawah;
                    result[i] = result[i] / (value - 1);
                    batasBawah++;
                }
            }
            else
            {
                double batasBawah = -(value) / 2;
                for (int i = 0; i < value; i++)
                {
                    result[i] = batasBawah;
                    result[i] = batasBawah / value;
                    batasBawah++;
                }
            }
            return result;
        }

        public static Matrix<double> square(Matrix<double> array) {
            Matrix<double> result = array;
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i,j] = Math.Sqrt(array.Data[i,j]);

            return result;
        }

        public static Matrix<double> power(Matrix<double> array, int pow) {
            Matrix<double> result = array;
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = Math.Pow(array.Data[i, j], pow);

            return result;
        }

        public static Matrix<double> div(Matrix<double> array, double div)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = array.Data[i, j]/div;

            return result;
        }

        public static Matrix<double> multArray(Matrix<double> array, Matrix<double> mult)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = array.Data[i, j] * mult.Data[i, j];

            return result;
        }

        public static Matrix<double> log(Matrix<double> array)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = Math.Log(array.Data[i, j]);

            return result;
        }

        public static Matrix<double> exp(Matrix<double> array)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = Math.Exp(array.Data[i, j]);

            return result;
        }

        public static Matrix<double> min(Matrix<double> array, double value)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = value - array.Data[i, j];

            return result;
        }

        public static Matrix<double> abs(Matrix<double> array)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = Math.Abs(array.Data[i, j]);

            return result;
        }

        public static Matrix<double> round(Matrix<double> array)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = Math.Round(array.Data[i, j]);

            return result;
        }

        public static Matrix<double> neg(Matrix<double> array)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = (array.Data[i, j] < 0)?1:0;

            return result;
        }

        public static Matrix<double> negasi(Matrix<double> array)
        {
            Matrix<double> result = array.Clone();
            for (int i = 0; i < array.Rows; i++)
                for (int j = 0; j < array.Cols; j++)
                    result.Data[i, j] = (array.Data[i, j] == 0) ? 1 : 0;

            return result;
        }

        public static double median(Matrix<double> array) {
            double med = -1.0;
            double m = (array.Rows * array.Cols) / 2;
            int bin = 0;

            int histSize = 256;
            int[] histSizeArr = new int[] { histSize };
            float[] range = new float[]{ 0, 256 };
            bool uniform = true;
            bool accumulate = false;
            Matrix<double> hist = new Matrix<double>(0, 255);
            CvInvoke.CalcHist(array, new int[] { 0 }, new Mat(), hist, histSizeArr, range, accumulate);
            for (int i = 0; i < histSize && med < 0.0; ++i) {
                bin += Convert.ToInt32(Math.Round(hist.Data[0, i]));
                if (bin > m && med < 0.0)
                    med = i;
            }

            return med;
        }

        public static Matrix<double> atanMinY(Matrix<double> y, Matrix<double> x) {
            Matrix<double> result = y.Clone();
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    
                    double temp = Math.Atan2(-y.Data[i, j], x.Data[i, j]);
                    result.Data[i, j] = temp;
                }
            }

            return result;
        }

        public static Matrix<double> atan(Matrix<double> y, Matrix<double> x)
        {
            Matrix<double> result = y.Clone();
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    double temp = Math.Atan2(y.Data[i, j], x.Data[i, j]);
                    result.Data[i, j] = temp;
                }
            }

            return result;
        }

        public static Matrix<double> ifftshift(Matrix<double> x) {
            int numDims = 2;
            int[][] idx = new int[numDims][];
            Matrix<double> y = x.Clone();

            for (int k = 0; k < numDims; k++) {
                int m;
                if (k == 0) {
                    m = x.Size.Height;
                }
                else {
                    m = x.Size.Width;
                }
                int p = (int)Math.Floor((decimal)m / 2);
                int[] temp = new int[m];
                for (int i = 0; i < m; i++) {
                    if (i < p) {
                        temp[i + 1 + p] = i;
                    } else {
                        temp[i - p] = i;
                    }
                }
                idx[k] = temp;
            }

            for (int i = 0; i < idx[0].Length; i++) {
                for (int j = 0; j < idx[1].Length; j++) {
                    double temp = x.Data[idx[0][i], idx[1][j]];
                    y.Data[i,j] = temp;
                }
            }

            return y;
        }

        public static Matrix<double> sin(Matrix<double> input) {
            Matrix<double> result = input.Clone();
            for (int i = 0; i < input.Rows; i++) {
                for (int j = 0; j < input.Cols; j++) {
                    double temp = Math.Sin(input.Data[i, j]);
                    result.Data[i, j] = temp;
                }
            }
            return result;
        }

        public static Matrix<double> cos(Matrix<double> input) {
            Matrix<double> result = input.Clone();
            for (int i = 0; i < input.Rows; i++)
            {
                for (int j = 0; j < input.Cols; j++)
                {
                    double temp = Math.Cos(input.Data[i, j]);
                    result.Data[i, j] = temp;
                }
            }
            return result;
        }

        public static Matrix<double> lowpassfilter(int rows, int cols, double cutoff, int n) {
            if (cutoff < 0 && cutoff > 0.5) {
                Console.WriteLine("cutoff frequency must be between 0 and 0.5");
                return null;
            }
            if (n % 1 != 0 || n < 1) {
                Console.WriteLine("n must be an integer >= 1");
                return null;
            }

            // Set up X and Y matrices with ranges normalised to +/ -0.5
            // The following code adjusts things appropriately for odd and even values
            // of rows and columns.
            double[] xrange = Utillity.createArray(cols);
            double[] yrange = Utillity.createArray(rows);

            Matrix<double> newXrange = new Matrix<double>(xrange);
            newXrange = newXrange.Transpose();
            Matrix<double> x = new Matrix<double>(rows, cols);

            Matrix<double> newYrange = new Matrix<double>(yrange);
            newYrange = newYrange.Transpose();
            Matrix<double> y = new Matrix<double>(rows, cols);

            CvInvoke.Repeat(newXrange.Reshape(1, 1), newXrange.Cols, 1, x);
            CvInvoke.Repeat(newYrange.Reshape(1, 1).Transpose(), 1, newXrange.Cols, y);

            Matrix<double> multX = Utillity.power(x, 2);
            Matrix<double> multY = Utillity.power(y, 2);
            Matrix<double> radius = Utillity.square(multX.Add(multY));  // A matrix with every pixel = radius relative to centre.
            Matrix<double> param = radius.Clone();

            for (int i = 0; i < radius.Rows; i++) {
                for (int j = 0; j < radius.Cols; j++) {
                    param.Data[i, j] = 1 / (1.0d + Math.Pow( radius.Data[i, j] / cutoff, (2 * n) ));
                    //param.Data[i, j] = 1.0f + Math.Pow(radius.Data[i, j] / cutoff, (2 * n));
                }
            }

            Matrix<double> f = Utillity.ifftshift(param);
            return f;
        }
    }
}
