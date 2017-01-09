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
        public Matrix<double> calc_LESH(Image<Gray, double> im) {
            // Load parameter
            int w = 4;          // image will be partitioned in w x w partitions
            int n_orient = 8;

            List<Matrix<double>> PC = new List<Matrix<double>>();
            Matrix<double> or = new Matrix<double>(im.Rows, im.Cols);
            PhaseCong2 phaseCongruency = new PhaseCong2();
            phaseCongruency.calcPhaseCong2(im, PC, or);
            Matrix<double> L = Utillity.getLabel(PC);

            List<Matrix<double>> pc_im = new List<Matrix<double>>();
            List<List<List<Matrix<double>>>> g = new List<List<List<Matrix<double>>>>();
            
            for (int i = 0; i < w; i++)
            {
                List<List<Matrix<double>>> kol = new List<List<Matrix<double>>>();
                for (int j = 0; j < w; j++)
                {
                    kol.Add(new List<Matrix<double>>());
                }
                g.Add(kol);
            }

            for (int i = 0; i < n_orient; i++) {
                Matrix<double> temp = new Matrix<double>(PC[i].Rows, PC[i].Cols);
                CvInvoke.Multiply(PC[i], Utillity.bitwiseEqual(L, i), temp);
                pc_im.Insert(i, temp.Clone());
                temp.Dispose();
            }

            int blksize = 0;
            if (PC[0].Rows % w == 0)
                blksize = PC[0].Rows / w;
            else
                blksize = Convert.ToInt32(Math.Floor(Convert.ToDouble(PC[0].Rows) / w));
            
            for (int ort = 0; ort < n_orient; ort++) {
                for (int i = 0; i < w; i++) {
                    for (int j = 0; j < w; j++) {
                        Matrix<double> temp = new Matrix<double>(blksize, blksize);
                        for (int baris = i * blksize; baris < i * blksize + blksize; baris++) {
                            for (int kolom = j * blksize; kolom < j * blksize + blksize; kolom++) {
                                temp.Data[baris % blksize, kolom % blksize] = pc_im[ort].Data[baris, kolom];
                            }
                        }
                        g[j][i].Insert(ort, temp.Clone());
                        temp.Dispose();
                    }
                }
            }

            Matrix<double> Shape_vect = new Matrix<double>(1, w * w * n_orient);
            
            int index = 0;
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < w; j++) {
                    for (int orientasi = 0; orientasi < n_orient; orientasi++) {
                        Matrix<double> temp = new Matrix<double>(1, blksize);
                        for (int baris = 0; baris < blksize; baris++) {
                            temp += g[j][i][orientasi].GetRow(baris);
                        }
                        double sumRows = CvInvoke.Sum(temp).V0;
                        Shape_vect.Data[0, index++] = sumRows;
                    }
                }
            }

            using (Matrix<double> tempShapeVect = Shape_vect.Clone()) {
                Shape_vect = (tempShapeVect - Utillity.minVal(tempShapeVect)) / (Utillity.maxVal(tempShapeVect) - Utillity.minVal(tempShapeVect));
            }

            for (int i = 0; i < Shape_vect.Cols; i++) {
                if (Double.IsNaN(Shape_vect.Data[0, i]))
                    Shape_vect.Data[0, i] = 0;
            }

            /*DBWavelet db = new DBWavelet();
            db.FWT(ref Shape_vect);
            Matrix<double> selectedLESH = Shape_vect.GetCols(0, Shape_vect.Cols/2);*/

            return Shape_vect;
        }
    }
}
