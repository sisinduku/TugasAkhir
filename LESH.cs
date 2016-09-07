using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class LESH
    {
        public Matrix<Double> calc_LESH(Image<Gray, double> im) {
            int w = 4;          // image will be partitioned in w x w partitions

            List<Matrix<double>> PC = new List<Matrix<double>>();
            Matrix<double> or = new Matrix<double>(im.Rows, im.Cols);
            PhaseCong2 phaseCongruency = new PhaseCong2();
            phaseCongruency.calcPhaseCong2(im, PC, or);
            for (int i = 0; i < PC[1].Rows; i++)
            {
                for (int j = 0; j < PC[1].Cols; j++)
                {
                    Console.WriteLine("[" + (i + 1) + ", " + (j + 1) + "] : " + PC[1].Data[i, j]);
                }
            }
            return or;
        }
    }
}
