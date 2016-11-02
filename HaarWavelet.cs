using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class HaarWavelet
    {
        private const double w0 = 0.5;
        private const double w1 = -0.5;
        private const double s0 = 0.5;
        private const double s1 = 0.5;
        public void FWT(ref Matrix<double> data)
        {
            double[] temp = new double[data.Cols];

            int h = data.Cols >> 1;
            for (int i = 0; i < h; i++)
            {
                int k = (i << 1);
                temp[i] = data.Data[0, k] * s0 + data.Data[0, k + 1] * s1;
                temp[i + h] = data.Data[0, k] * w0 + data.Data[0, k + 1] * w1;
            }

            for (int i = 0; i < data.Cols; i++)
                data.Data[0, i] = temp[i];
        }
    }
}
