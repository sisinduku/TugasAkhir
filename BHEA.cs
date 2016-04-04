using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace TugasAkhir
{
    class BHEA
    {
        public void hCalc(int row, int start, int last, Image<Gray, Byte> orgM, ref Image<Gray, Byte> horM) {
            int[] array = new int[last];

            if (start != last) {
                int len = 0;
                for (int i = start; i < last; i++) {
                    array[len] = orgM.Data[row, i, 0];
                    len++;
                }
                for (int i = start; i < last; i++) {
                    int modus = array.GroupBy(item => item).OrderByDescending(g => g.Count()).Select(g => g.Key).First();
                    horM.Data[row, i, 0] = Convert.ToByte(modus);
                }
            }
        }
    }
}
