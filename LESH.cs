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
    class LESH
    {
        public Matrix<float> calc_LESH(Image<Gray, double> im) {
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

            double min = 0, max = 0;
            Point minLoc = new Point();
            Point maxLoc = new Point();
            CvInvoke.MinMaxLoc(Shape_vect, ref min, ref max, ref minLoc, ref maxLoc);
            using (Matrix<double> tempShapeVect = Shape_vect.Clone()) {
                Shape_vect = (tempShapeVect - min) / (max - min);
            }

            for (int i = 0; i < Shape_vect.Cols; i++) {
                if (Double.IsNaN(Shape_vect.Data[0, i]))
                    Shape_vect.Data[0, i] = 0;
            }

            /*using (Matrix<double> tempShapeVect = Shape_vect.Clone())
            {
                Shape_vect = ((tempShapeVect - Utillity.minVal(tempShapeVect)) * (1 - (-1)) / (Utillity.maxVal(tempShapeVect) - Utillity.minVal(tempShapeVect))) + (-1);
            }*/
            DBWavelet db = new DBWavelet();
            db.FWT(ref Shape_vect);
            //Matrix<float> selectedLESH = Shape_vect.GetCols(0, Shape_vect.Cols/2).Convert<float>();
            Matrix<float> selectedLESH = new Matrix<float>(1, 70);
            /*using (Matrix<double> tempShapeVect = selectedLESH.Clone())
            {
                selectedLESH = ((tempShapeVect - Utillity.minVal(tempShapeVect)) / (Utillity.maxVal(tempShapeVect) - Utillity.minVal(tempShapeVect)));
            }*/

            List<float> dataList = new List<float>();
            using (Matrix<float> temp = Shape_vect.Convert<float>())
                for (int i = 0; i < temp.Cols; i++)
                    dataList.Add(temp.Data[0, i]);

            var sorted = dataList
                .Select((x, i) => new KeyValuePair<float, int>(x, i))
                .OrderBy(x => x.Key)
                .ToList();

            List<int> idxNew = sorted.Select(x => x.Value).ToList().GetRange(57, 70);
            
            var sortedIdx = idxNew
                .Select((x, i) => new KeyValuePair<int, int>(x, i))
                .OrderBy(x => x.Key)
                .ToList();

            List<int> indeks = sortedIdx.Select(x => x.Key).ToList();
            using (Matrix<float> temp = Shape_vect.Convert<float>()) {
                for (int i = 0; i < indeks.Count; i++)
                {
                    selectedLESH.Data[0, i] = temp.Data[0, indeks[i]];
                }
            }
            return selectedLESH;
        }
    }
}
