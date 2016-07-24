using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.UI;

namespace TugasAkhir
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
            GaborWavelet test = new GaborWavelet();
            int cols = 275;
            int rows = 275;
            double[] coba = Utillity.createArray(rows);
            double[] coba2 = Utillity.createArray(cols);
            Matrix<double> newXrange = new Matrix<double>(coba2);
            newXrange = newXrange.Transpose();

            Matrix<double> newYrange = new Matrix<double>(coba);
            newYrange = newYrange.Transpose();

            Matrix<double> x = new Matrix<double>(rows, cols);
            Matrix<double> y = new Matrix<double>(rows, cols);
            CvInvoke.Repeat(newXrange.Reshape(1, 1), newYrange.Cols, 1, x);
            CvInvoke.Repeat(newYrange.Reshape(1, 1).Transpose(), 1, newXrange.Cols, y);
            Matrix<double> multX = Utillity.power(x, 2);
            Matrix<double> multY = Utillity.power(y, 2);
            Matrix<double> tes = multX.Add(multY);

            Matrix<double> radius = Utillity.square(multX.Add(multY));
            Matrix<double> theta = Utillity.atanMinY(y, x);
            //radius = Utillity.ifftshift(radius);
            //theta = Utillity.ifftshift(theta);
            //Matrix<double> lp = Utillity.lowpassfilter(rows, cols, 0.45f, 15);

            for (int i = 0; i < radius.Rows; i++) {
                Console.WriteLine(i + " " + radius.Data[i, 0]);
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            PelatihanForm pelatihan = new PelatihanForm();
            pelatihan.Tag = this;
            pelatihan.Show(this);
            //HiddenForms.Add(this);
            Hide();
        }
    }
}
