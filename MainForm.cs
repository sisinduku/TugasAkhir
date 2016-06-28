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

            Image<Gray, Byte> My_Image = new Image<Gray, byte>(@"E:\Data\Project\TA\App\TugasAkhir\TugasAkhir\mdb006.pgm");
            //imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            My_Image = My_Image.SmoothGaussian(7);

            Image<Gray, Byte> My_Imagebw = new Image<Gray, byte>(1024, 1024);
            CvInvoke.Threshold(My_Image, My_Imagebw, 255 * 0.0706, 255, ThresholdType.Binary);
            //imageBox1.Image = My_Imagebw;

            Image<Gray, Byte> My_ImageCountour = new Image<Gray, byte>(1024, 1024);
            int largest_contour_index = 0;
            double largest_area = 0;
            VectorOfPoint largestContour;

            using (Mat hierachy = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint()) {

                CvInvoke.FindContours(My_Imagebw, contours, hierachy, RetrType.Ccomp, ChainApproxMethod.ChainApproxNone);

                for (int i = 0; i < contours.Size; i++)
                {
                    double a = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                    if (a > largest_area)
                    {
                        largest_area = a;
                        largest_contour_index = i;                //Store the index of largest contour
                    }
                }

                CvInvoke.DrawContours(My_ImageCountour, contours, largest_contour_index, new MCvScalar(255, 255, 255), -1, LineType.EightConnected, hierachy);
                largestContour = new VectorOfPoint(contours[largest_contour_index].ToArray());
                My_Imagebw.Dispose();
            }

            Image<Gray, Int32> oriImageInt = My_Image.Convert<Gray, Int32>();
            My_Image.Dispose();

            float[,] k = new float[3,3] { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
            ConvolutionKernelF kernel = new ConvolutionKernelF(k);
            CvInvoke.Filter2D(My_ImageCountour, My_ImageCountour, kernel, new Point(-1, -1));

            ConvolutionKernelF matrix = new ConvolutionKernelF(9, 9);

            matrix.SetValue(1);
            matrix.Data[0, 0] = 0; matrix.Data[0, 1] = 0; matrix.Data[1, 0] = 0; matrix.Data[0, 7] = 0; matrix.Data[0, 8] = 0; matrix.Data[1, 8] = 0;
            matrix.Data[7, 0] = 0; matrix.Data[8, 0] = 0; matrix.Data[8, 1] = 0; matrix.Data[7, 8] = 0; matrix.Data[8, 8] = 0; matrix.Data[8, 7] = 0;
            Mat strel = new Mat();
            strel.SetTo(matrix);
            matrix.Dispose();

            CvInvoke.MorphologyEx(My_ImageCountour, My_ImageCountour, MorphOp.Open, strel, new Point(-1, -1), 5, BorderType.Replicate, new MCvScalar());
            strel.Dispose();

            Image<Gray, Int32> productTemp = new Image<Gray, Int32>(1024, 1024);
            Image<Gray, Byte> product = new Image<Gray, Byte>(1024, 1024);

            Image<Gray, Int32> countourImageInt = My_ImageCountour.Convert<Gray, Int32>();
            //imageBox1.Image = My_ImageCountour;
            int jarakKiri = 0, jarakKanan = 0, indeks = 0;

            // From left
            while (My_ImageCountour.Data[10, indeks, 0] == 0 && My_ImageCountour.Data[1013, indeks, 0] == 0)
            {
                jarakKiri++;
                indeks++;
            }

            indeks = 1023;
            // From right
            while (My_ImageCountour.Data[10, indeks, 0] == 0 && My_ImageCountour.Data[1013, indeks, 0] == 0)
            {
                jarakKanan++;
                indeks--;
            }
            
            int orientasi = (jarakKiri < jarakKanan) ? 0 : 1;

            My_ImageCountour.Dispose();

            productTemp = oriImageInt.Mul(countourImageInt);
            oriImageInt.Dispose();
            countourImageInt.Dispose();

            product = productTemp.ConvertScale<Byte>(1.0f / 65025.0f * 255, 0);
            productTemp.Dispose();

            //imageBox1.Image = product;


        
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
