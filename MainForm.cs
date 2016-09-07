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
using System.Collections;

namespace TugasAkhir
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
            PhaseCong2 test = new PhaseCong2();

            Image<Gray, double> image = new Image<Gray, double>(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\all-mias\mdb005.pgm");

            List<Matrix<double>> PC = new List<Matrix<double>>();
            Matrix<double> or = new Matrix<double>(image.Rows, image.Cols);
            PhaseCong2 phaseCongruency = new PhaseCong2();
            phaseCongruency.calcPhaseCong2(image.Copy(new Rectangle(477 - 1, (1024 - 133) - 1, 30, 30)), PC, or);
            //LESH lesh = new LESH();
            
            //lesh.calc_LESH(image.Copy(new Rectangle(477 - 1, (1024 - 133) - 1, 30, 30)));

            Matrix<double> img = new Matrix<double>(image.Rows, image.Cols);
            image.CopyTo(img);
            //double testing = Utillity.median(img);
            //Console.WriteLine(testing);
            Matrix<double> matBDftBlank = img.CopyBlank();
            Matrix<double> dftIn = new Matrix<double>(img.Rows, img.Cols, 2);
            using (VectorOfMat mv = new VectorOfMat(new Mat[] { img.Mat, matBDftBlank.Mat }))
                CvInvoke.Merge(mv, dftIn);

            Matrix<double> dftOut = new Matrix<double>(img.Rows, img.Cols, 2);

            CvInvoke.Dft(dftIn, dftOut, Emgu.CV.CvEnum.DxtType.Forward, 0);

            //The real part of EO
            Matrix<double> EORealPart = new Matrix<double>(img.Rows, img.Cols);
            //The imaginary part of EO
            Matrix<double> EOImPart = new Matrix<double>(img.Rows, img.Cols);

            using (VectorOfMat vm = new VectorOfMat())
            {
                vm.Push(EORealPart.Mat);
                vm.Push(EOImPart.Mat);
                CvInvoke.Split(dftOut, vm);
            }

            

            /*int cols = 275;
            int rows = 275;
            int nScale = 5;
            int nOrient = 8;
            int minWaveLength = 3;
            float mult = 2.1F;
            float dThetaOnSigma = 1.2F;
            float sigmaOnf = 0.55F;
            double thetaSigma = Math.PI / nOrient / dThetaOnSigma;
            double[,] zeros = new double[rows, cols];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    zeros[i, j] = 0;
                }
            }
            List <Matrix<double>> ifftFilterArray = new List<Matrix<double>>();

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
            Matrix<double> multX = Utillity.power(x.Clone(), 2);
            Matrix<double> multY = Utillity.power(y.Clone(), 2);

            int baris = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(200) / 2)) - 1;
            int kolom = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(200) / 2)) - 1;
            Matrix<double> radius = Utillity.square(multX.Add(multY));
            radius.Data[baris, kolom] = 1;
            Matrix<double> theta = Utillity.atanMinY(y, x);

            radius = Utillity.ifftshift(radius);
            theta = Utillity.ifftshift(theta);

            Matrix<double> sintheta = Utillity.sin(theta);

            Matrix<double> costheta = Utillity.cos(theta);

            x.Dispose(); y.Dispose(); theta.Dispose();

            Matrix<double> lp = Utillity.lowpassfilter(rows, cols, 0.45D, 15);

            List<Matrix<double>> logGabor = new List<Matrix<double>>();
            for (int i = 0; i < nScale; i++)
            {
                double waveLength = minWaveLength * Math.Pow(mult, ((i + 1) - 1));
                double fo = 1.0D / waveLength;  // Centre frequency of filter.
                //Console.WriteLine(i + " " + waveLength + " " + fo);
                Matrix<double> temp1 = Utillity.min(Utillity.power(Utillity.log(Utillity.div(radius, fo)), 2), 0);
                //Matrix<double> temp1 = Utillity.log(Utillity.div(radius, fo));
                double temp2 = (2 * Math.Pow(Math.Log(sigmaOnf), 2));

                //logGabor.Add(temp1);
                //Console.WriteLine(temp2);

                logGabor.Add(Utillity.exp(Utillity.div(temp1, temp2)));
                logGabor[i] = Utillity.multArray(logGabor[i], lp);    // Apply low-pass filter
                logGabor[i].Data[0, 0] = 0;    // Set the value at the 0 frequency point of the filter
                                               // back to zero (undo the radius fudge).
            }

            // Then construct the angular filter components...
            List<Matrix<double>> spread = new List<Matrix<double>>();

            for (int o = 0; o < nOrient; o++)
            {
                double angl = (o + 1 - 1) * Math.PI / nOrient;  // Filter angle.

                //Console.WriteLine(angl);
                //  For each point in the filter matrix calculate the angular distance from
                //  the specified filter orientation.To overcome the angular wrap - around
                //  problem sine difference and cosine difference values are first computed
                //  and then the atan2 function is used to determine angular distance.

                Matrix<double> ds = sintheta * Math.Cos(angl) - costheta * Math.Sin(angl);  // Difference in sine.
                Matrix<double> dc = costheta * Math.Cos(angl) + sintheta * Math.Sin(angl);  // Difference in cosine.
                Matrix<double> dtheta = Utillity.abs(Utillity.atan(ds, dc));    // Absolute angular distance.
                spread.Add(Utillity.exp(Utillity.div(Utillity.min(Utillity.power(dtheta, 2), 0), 2.0D * Math.Pow(thetaSigma, 2)))); // Calculate the angular filter component.

                //spread.Add(ds);
            }

            // The main loop...
            for (int o = 1; o <= nOrient; o++)
            { // For each orientation.
                double angl = (o - 1) * Math.PI / nOrient;  // Filter angle.
                double[,] sumE_ThisOrient = zeros;          // Initialize accumulator matrices.
                double[,] sumO_ThisOrient = zeros;
                double[,] sumAn_ThisOrient = zeros;
                double[,] Energy = zeros;

                for (int s = 1; s <= nScale; s++)
                { // For each scale.
                    Matrix<double> filter = Utillity.multArray(logGabor[s - 1], spread[o - 1]); // Multiply radial and angular
                                                                                                // components to get the filter. 
                    Matrix<double> matBDftBlank = filter.CopyBlank();
                    Matrix<double> dftIn = new Matrix<double>(filter.Rows, filter.Cols, 2);
                    using (VectorOfMat mv = new VectorOfMat(new Mat[] { filter.Mat, matBDftBlank.Mat }))
                        CvInvoke.Merge(mv, dftIn);

                    Matrix<double> dftOut = new Matrix<double>(rows, cols, 2);

                    CvInvoke.Dft(dftIn, dftOut, Emgu.CV.CvEnum.DxtType.InvScale, 0);
                    //The real part of the Fourior Transform
                    Matrix<double> outReal = new Matrix<double>(filter.Size);
                    //The imaginary part of the Fourior Transform
                    Matrix<double> outIm = new Matrix<double>(filter.Size);

                    using (VectorOfMat vm = new VectorOfMat())
                    {
                        vm.Push(outReal.Mat);
                        vm.Push(outIm.Mat);
                        CvInvoke.Split(dftOut, vm);
                    }

                    Matrix<double> ifftFilt = outReal * Math.Sqrt(rows * cols); //Note rescaling to match power
                    ifftFilterArray.Add(ifftFilt);
                }
            }*/


            /*for (int i = 0; i < ifftFilt.Rows; i++)
            {
                for (int j = 0; j < ifftFilt.Cols; j++)
                {
                    Console.WriteLine("[ " + i + ", " + j + " ] " + ifftFilt.Data[i, j]);
                }
            }*/
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
