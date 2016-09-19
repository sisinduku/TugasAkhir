using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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
            Matrix<double> result = array.Clone();
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

        public static void HeapSort(ref Matrix<double> input)
        {
            //Build-Max-Heap
            int heapSize = input.Cols-1;
            for (int p = heapSize / 2; p >= 0; p--)
                MaxHeapify(ref input, heapSize, p);

            for (int i = input.Cols - 1; i > 0; i--)
            {
                //Swap
                double temp = input.Data[0, i];
                input.Data[0, i] = input.Data[0, 0];
                input.Data[0, 0] = temp;

                heapSize--;
                MaxHeapify(ref input, heapSize, 0);
            }
        }

        private static void MaxHeapify(ref Matrix<double> input, int heapSize, int index)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int largest = index;

            if (left <= heapSize && input.Data[0, left] > input.Data[0, index])
                largest = left;            

            if (right <= heapSize && input.Data[0, right] > input.Data[0, largest])
                largest = right;

            if (largest != index)
            {
                double temp = input.Data[0, index];
                input.Data[0, index] = input.Data[0, largest];
                input.Data[0, largest] = temp;

                MaxHeapify(ref input, heapSize, largest);
            }
        }

        public static double median(Matrix<double> array) {
            double med = -1.0;
            Matrix<double> test = array.Clone();
            
            Matrix<double> reshaped = test.Reshape(1, 1).Clone();
            
            HeapSort(ref reshaped);

            if (reshaped.Cols % 2 == 1) {
                med = reshaped.Data[0, Convert.ToInt32(Math.Floor(Convert.ToDouble(reshaped.Cols / 2)))];
            }
            else {
                double temp1 = reshaped.Data[0, reshaped.Cols / 2];
                double temp2 = reshaped.Data[0, (reshaped.Cols / 2) - 1];
                med = (temp1 + temp2) / 2;
            }

            return med;
        }

        public static double minVal(Matrix<double> array) {
            Matrix<double> sort = array.Clone();

            HeapSort(ref sort);

            return sort.Data[0, 0];
        }

        public static double maxVal(Matrix<double> array)
        {
            Matrix<double> sort = array.Clone();

            HeapSort(ref sort);

            return sort.Data[0, sort.Cols-1];
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
                int p;
                int[] temp = new int[m];
                if (m % 2 == 1)
                {
                    p = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(m) / 2)) - 1;
                    for (int i = 0; i < m; i++) {
                        if (i < p) {
                            temp[i + 1 + p] = i;
                        }
                        else {
                            temp[i - p] = i;
                        }
                    }
                }
                else {
                    p = m/2;
                    for (int i = 0; i < m; i++) {
                        if (i < p) {
                            temp[i + p] = i;
                        }
                        else {
                            temp[i - p] = i;
                        }
                    }
                }
                //Convert.ToInt32(Math.Ceiling(Convert.ToDouble(m) / 2)) - 1;                
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

        public static Matrix<double> bitwiseEqual(Matrix<double> input, int number)
        {
            Matrix<double> result = input.Clone();
            for (int i = 0; i < input.Rows; i++)
            {
                for (int j = 0; j < input.Cols; j++)
                {
                    result.Data[i, j] = Convert.ToInt32(input.Data[i, j] == number);
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

        public static Matrix<double> getLabel(List<Matrix<double>> PC) {
            double maxVal = new double();
            Matrix<double> result = new Matrix<double>(PC[0].Rows, PC[0].Cols);
            for (int i = 0; i < PC[0].Rows; i++) {
                for (int j = 0; j < PC[0].Cols; j++) {
                    maxVal = Math.Max(Math.Max(Math.Max(PC[0].Data[i, j], PC[1].Data[i, j]), Math.Max(PC[2].Data[i, j], PC[3].Data[i, j])), Math.Max(Math.Max(PC[4].Data[i, j], PC[5].Data[i, j]), Math.Max(PC[6].Data[i, j], PC[7].Data[i, j])));
                    if (maxVal == PC[0].Data[i, j])
                        result.Data[i, j] = 0;
                    else if(maxVal == PC[1].Data[i, j])
                        result.Data[i, j] = 1;
                    else if(maxVal == PC[2].Data[i, j])
                        result.Data[i, j] = 2;
                    else if (maxVal == PC[3].Data[i, j])
                        result.Data[i, j] = 3;
                    else if (maxVal == PC[4].Data[i, j])
                        result.Data[i, j] = 4;
                    else if (maxVal == PC[5].Data[i, j])
                        result.Data[i, j] = 5;
                    else if (maxVal == PC[6].Data[i, j])
                        result.Data[i, j] = 6;
                    else if (maxVal == PC[7].Data[i, j])
                        result.Data[i, j] = 7;
                }
            }
            return result;
        }
    }
}
