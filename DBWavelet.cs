using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class DBWavelet
    {
        // Scaling function coefficient
        private double h0 = (1+Math.Sqrt(3)) / (4*Math.Sqrt(2));
        private double h1 = (3 + Math.Sqrt(3)) / (4 * Math.Sqrt(2));
        private double h2 = (3 - Math.Sqrt(3)) / (4 * Math.Sqrt(2));
        private double h3 = (1 - Math.Sqrt(3)) / (4 * Math.Sqrt(2));

        // wavelet function coefficient
        private double g0 = (1 - Math.Sqrt(3)) / (4 * Math.Sqrt(2));
        private double g1 = -(3 - Math.Sqrt(3)) / (4 * Math.Sqrt(2));
        private double g2 = (3 + Math.Sqrt(3)) / (4 * Math.Sqrt(2));
        private double g3 = -(1 + Math.Sqrt(3)) / (4 * Math.Sqrt(2));

        public void FWT(ref Matrix<double> data)
        {
            double[] temp = new double[data.Cols];

            int half = data.Cols >> 1;
            int i = 0;
            int n = data.Cols;
            for (int j = 0; j < n - 3; j = j +2)
            {
                temp[i] = data.Data[0, j] * h0 + data.Data[0, j + 1] * h1 + data.Data[0, j + 2] * h2 + data.Data[0, j + 3] * h3;
                temp[i + half] = data.Data[0, j] * g0 + data.Data[0, j + 1] * g1 + data.Data[0, j + 2] * g2 + data.Data[0, j + 3] * g3;
                i++;
            }

            temp[i] = data.Data[0, n - 2] * h0 + data.Data[0, n - 1] * h1 + data.Data[0, 0] * h2 + data.Data[0, 1] * h3;
            temp[i + half] = data.Data[0, n - 2] * g0 + data.Data[0, n - 1] * g1 + data.Data[0, 0] * g2 + data.Data[0, 1] * g3;

            for (i = 0; i < n; i++)
                data.Data[0, i] = temp[i];
        }
    }
}
