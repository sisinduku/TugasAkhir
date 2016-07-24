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

        public static Matrix<double> atanMinY(Matrix<double> y, Matrix<double> x) {
            Matrix<double> result = y.Clone();
            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Cols; j++)
                    result.Data[i, j] = Math.Atan2(-y.Data[i,j], x.Data[i,j]);

            return result;
        }

        public static Matrix<double> ifftshift(Matrix<double> x) {
            int numDims = 2;
            int[][] idx = new int[numDims][];
            Matrix<double> y = x;
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
                    y.Data[i, j] = x.Data[idx[0][i], idx[1][j]];
                    //Console.WriteLine("Y[" + i + "," + j + "] : " + y.Data[i,j] + " -> X[" + idx[0][i] + "," + idx[1][j] + "] : " + x.Data[idx[0][i], idx[1][j]]);
                }
            }
            return y;
        }

        public static Matrix<double> sin(Matrix<double> input) {
            Matrix<double> result = input;
            for (int i = 0; i < input.Rows; i++) {
                for (int j = 0; j < input.Cols; j++) {
                    result.Data[i, j] = Math.Sin(input.Data[i, j]);
                }
            }
            return result;
        }

        public static Matrix<double> cos(Matrix<double> input) {
            Matrix<double> result = input;
            for (int i = 0; i < input.Rows; i++)
            {
                for (int j = 0; j < input.Cols; j++)
                {
                    result.Data[i, j] = Math.Cos(input.Data[i, j]);
                }
            }
            return result;
        }

        public static Matrix<double> lowpassfilter(int rows, int cols, float cutoff, int n) {
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
            Matrix<double> param = radius;

            for (int i = 0; i < radius.Rows; i++) {
                for (int j = 0; j < radius.Cols; j++) {
                    param.Data[i, j] = 1 / (Math.Pow(1 + (radius.Data[i, j] / cutoff), (2 * n)));
                }
            }
            Matrix<double> f = Utillity.ifftshift(param);
            return param;
        }
    }
}
