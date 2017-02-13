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
using Emgu.CV.Structure;
using System.Collections;
using System.IO;
using Emgu.CV.ML;

namespace TugasAkhir
{
    public partial class Debug : MetroFramework.Forms.MetroForm
    {
        private Hashtable listImage;
        private ArrayList roiImage = new ArrayList();
        private List<Matrix<float>> featureList = new List<Matrix<float>>();
        private List<string> classes = new List<string>();
        SVM modelSVM = new SVM();

        public Debug()
        {
            InitializeComponent();
        }

        private void Debug_Load(object sender, EventArgs e)
        {
            /*Image<Gray, byte> image = new Image<Gray, byte>(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\all-mias\mdb256.pgm");
            Image<Gray, byte> CLAHEImage = image.Copy();
            int radius = 37 * 2;
            float jejari = radius / 2;
            int x = Convert.ToInt32(400 - jejari);
            int y = Convert.ToInt32(1024 - 484 - jejari);
            CvInvoke.MedianBlur(image.Copy(), image, 3);
            Image<Gray, byte> before = image.Clone();
            Image<Gray, byte> temp1 = image.Clone();
            Image<Gray, byte> smoothed = image.Clone();
            CvInvoke.GaussianBlur(image.Clone(), temp1, new Size(0, 0), 2);
            CvInvoke.GaussianBlur(temp1.Clone(), smoothed, new Size(0, 0), 2);
            CvInvoke.AddWeighted(image.Clone(), 1.5, smoothed, -0.5, 0, image);
            CvInvoke.CLAHE(image.Clone(), 3.56, new Size(8, 8), image);
            imageBox1.Image = CLAHEImage.Copy(new Rectangle(x, y, radius, radius));
            CLAHEImage = Preprocessing.enhanceImage(CLAHEImage);
            imageBox2.Image = CLAHEImage.Copy(new Rectangle(x, y, radius, radius));
            */
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            var main = (Form1)Tag;
            main.Show();
            Hide();
        }

        private void Debug_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            var main = (Form1)Tag;
            main.Show();
            Hide();
        }

        private void metroButton1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string namaFolder = folderBrowserDialog1.SelectedPath;
                metroTextBox1.Text = namaFolder;
                metroButton3.Enabled = true;
            }
        }

        // Load citra
        private void metroButton3_Click(object sender, EventArgs e)
        {
            // Start loading images
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();

            // Disable the Start button until 
            // the asynchronous operation is done.
            this.metroButton3.Enabled = false;
        }

        // Fungsi untuk meload citra dan mengenhance citra
        Hashtable loadImage(String[] paths, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int totalImageCount = paths.Length;
            Hashtable listImage = new Hashtable();
            int highestPercentageReached = 0;

            for (int i = 1; i <= totalImageCount; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    Image<Gray, byte> My_Image = new Image<Gray, byte>(@paths[i - 1]);
                    Image<Gray, byte> CLAHEImage = My_Image;
                    
                    CLAHEImage = Preprocessing.enhanceImage(My_Image);
                    String fullFileName = paths[i - 1].Split('\\', '/').Last();
                    String fileName = fullFileName.Split('.').First();
                    listImage.Add(fileName, CLAHEImage.Clone());
                    CLAHEImage.Dispose();

                    int percentComplete = (int)((float)i / (float)totalImageCount * 100);
                    if (percentComplete > highestPercentageReached)
                    {
                        highestPercentageReached = percentComplete;
                        worker.ReportProgress(percentComplete);
                    }
                }
            }
            return listImage;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file list here
            string[] filePaths = Directory.GetFiles(@metroTextBox1.Text, "*.pgm");
            //string[] filePaths = Directory.GetFiles(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\data kanker", "*.pgm");

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadImage(filePaths, worker, e);
        }

        // Progress bar load image
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar1.Value = e.ProgressPercentage;
        }

        // Action setelah load image sukses
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                Console.WriteLine("Canceled");
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                listImage = (Hashtable)e.Result;
                foreach (DictionaryEntry elemen in listImage)
                {
                    Console.WriteLine(elemen.Key);
                }
                Console.WriteLine(listImage.Count);
                metroButton2.Enabled = true;
            }

            // Enable the Start button.
            metroButton3.Enabled = true;
        }

        // Memilih ROI citra
        private void metroButton2_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string namaFile = openFileDialog1.InitialDirectory + openFileDialog1.FileName;
                try
                {
                    metroTextBox2.Text = namaFile;
                    metroButton5.Enabled = true;
                }
                catch (IOException io)
                {
                    Console.WriteLine(io.Message);
                }
            }
        }

        // Menjalankan load ROI
        private void metroButton5_Click(object sender, EventArgs e)
        {
            // Start loading images
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.RunWorkerAsync();

            this.metroButton5.Enabled = false;
        }

        // Fungsi untuk load ROI
        ArrayList loadROI(String paths, BackgroundWorker worker, DoWorkEventArgs e)
        {
            string line;
            string[] elements;
            ArrayList result = new ArrayList();
            //int totalImageCount = 119;
            int totalImageCount = File.ReadLines(paths).Count();
            int i = 1;
            int highestPercentageReached = 0;

            System.IO.StreamReader file =
                new System.IO.StreamReader(paths);

            while ((line = file.ReadLine()) != null)
            {
                Hashtable image = new Hashtable();
                ArrayList container = new ArrayList();
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    elements = line.Split(' ');
                    if (!elements[2].Equals("NORM") && !elements[4].Equals("*NOTE3*"))
                    {
                        Console.WriteLine(elements[0]);
                        Image<Gray, byte> My_Image = (Image<Gray, byte>)listImage[elements[0]];
                        int radius = 0;
                        if ((Int32.Parse(elements[6]) * 2) % 4 == 0)
                        {
                            radius = Int32.Parse(elements[6]) * 2;
                        }
                        else
                        {
                            radius = (Int32.Parse(elements[6]) * 2) + (4 - ((Int32.Parse(elements[6]) * 2) % 4));
                        }
                        float jejari = radius / 2;
                        int x = Convert.ToInt32(Int32.Parse(elements[4]) - jejari);
                        int y = Convert.ToInt32(1024 - Int32.Parse(elements[5]) - jejari);
                        Matrix<double> localEnergy = new Matrix<double>(radius, radius);
                        Image<Gray, int> im = My_Image.Copy(new Rectangle(x, y, radius, radius)).Convert<Gray, int>();
                        Matrix<int> newImage = new Matrix<int>(radius, radius);
                        im.CopyTo(newImage);
                        container.Add(elements[0]);                                // File name
                        container.Add(newImage);                                    // Image
                        container.Add(elements[3]);                                // Calsification
                        result.Add(container);
                        localEnergy.Dispose();
                        int percentComplete = (int)((float)i / (float)totalImageCount * 100);

                        if (percentComplete > highestPercentageReached)
                        {
                            highestPercentageReached = percentComplete;
                            worker.ReportProgress(percentComplete);
                        }
                        i++;
                    }
                }
            }
            file.Close();
            return result;
        }

        // Thread load ROI
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file here
            string roiPaths = @metroTextBox2.Text;

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadROI(roiPaths, worker, e);
        }

        // Progress bar load ROI
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar2.Value = e.ProgressPercentage;
        }

        // Action setelah load ROI berhasil
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                Console.WriteLine("Canceled");
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                roiImage = (ArrayList)e.Result;

                foreach (ArrayList container in roiImage)
                {
                    classes.Add(container[2].ToString());
                }

                Console.WriteLine(roiImage.Count);
                metroButton5.Enabled = true;
                metroButton6.Enabled = true;
            }
        }

        // Ekstraksi Fitur
        private void metroButton6_Click(object sender, EventArgs e)
        {
            // Extracting Features
            backgroundWorker3.WorkerSupportsCancellation = true;
            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.RunWorkerAsync();

            this.metroButton6.Enabled = false;
        }

        // Fungsi ekstraksi fitur
        List<Matrix<float>> extractFeature(ArrayList roiImage, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int totalImageCount = roiImage.Count;
            int i = 1;
            int highestPercentageReached = 0;
            List<Matrix<float>> GLCMFeatures = new List<Matrix<float>>();

            GLCM GLCMExtractor = new GLCM();
            foreach (ArrayList container in roiImage)
            {
                Matrix<int> im = (Matrix<int>)container[1];
                Matrix<float> leshFeature = GLCMExtractor.featureGLCM(im);
                Console.WriteLine(i);
                for (int j = 0; j < leshFeature.Cols; j++)
                {
                    Console.WriteLine("[" + 0 + ", " + j + "] = " + leshFeature.Data[0, j]);
                }
                GLCMFeatures.Add(leshFeature);

                int percentComplete = (int)((float)i / (float)totalImageCount * 100);

                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
                i++;
            }

            return GLCMFeatures;
        }

        // Thread ekstraksi fitur
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = extractFeature(roiImage, worker, e);
        }

        // Action setelah ekstraksi fitur selesai
        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                Console.WriteLine("Canceled");
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                featureList = (List<Matrix<float>>)e.Result;

                Console.WriteLine(featureList.Count);
                metroButton6.Enabled = true;
                metroButton7.Enabled = true;
            }
        }

        // Progress bar ekstraksi fitur
        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar3.Value = e.ProgressPercentage;
        }

        // Melatih SVM
        private void metroButton7_Click(object sender, EventArgs e)
        {
            // Train SVM
            backgroundWorker4.WorkerSupportsCancellation = true;
            backgroundWorker4.WorkerReportsProgress = true;
            backgroundWorker4.RunWorkerAsync();

            this.metroButton7.Enabled = false;
        }

        // Fungsi untuk melatih SVM
        void evaluateSVM(List<Matrix<float>> samples, List<string> classes, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int highestPercentageReached = 0;
            // Initialize Sample
            Matrix<float> data = new Matrix<float>(samples.Count, samples[0].Cols);
            int count = 0;
            foreach (Matrix<float> sample in samples)
            {
                for (int cols = 0; cols < sample.Cols; cols++)
                {
                    data.Data[count, cols] = sample.Data[0, cols];
                }
                count++;
            }

            for (int i = 0; i < data.Cols; i++) {
                Matrix<float> dimention = data.GetCol(i);
                MCvScalar mean = new MCvScalar();
                MCvScalar std = new MCvScalar();
                CvInvoke.MeanStdDev(dimention, ref mean, ref std);
                dimention = (dimention - mean.V0) / std.V0;
                for (int j = 0; j < data.Rows; j++) {
                    data.Data[j, i] = dimention.Data[j, 0];
                }
            }

            // Initialize response
            Matrix<int> response = new Matrix<int>(classes.Count, 1);

            count = 0;
            foreach (string kelas in classes)
            {
                //Console.WriteLine(kelas);
                switch (kelas)
                {
                    case "M":
                        response.Data[count, 0] = 1;
                        break;
                    case "B":
                        response.Data[count, 0] = 0;
                        break;
                }
                count++;
            }
            float akurasi = 0;
            for (int fold = 0; fold < 10; fold++)
            {
                // Data latih dan testing
                Matrix<float> dataFold = new Matrix<float>(1, data.Cols);
                Matrix<float> testingFold = new Matrix<float>(1, data.Cols);

                // response latih dan testing
                Matrix<int> targetFold = new Matrix<int>(1, 1);
                Matrix<int> responseFold = new Matrix<int>(1, 1);
                for (int kelas = 0; kelas < 2; kelas++)
                {
                    if (fold != 0)
                    {
                        Matrix<float> dataKelas = data.GetRows((kelas * 50), (kelas * 50) + (fold * 5), 1).Clone();
                        dataFold = dataFold.ConcateVertical(dataKelas).Clone();
                        Matrix<int> targetKelas = response.GetRows((kelas * 50), (kelas * 50) + (fold * 5), 1).Clone();
                        targetFold = targetFold.ConcateVertical(targetKelas).Clone();
                    }
                    Matrix<float> testingKelas = data.GetRows((kelas * 50) + (fold * 5), 5 + (kelas * 50) + (fold * 5), 1).Clone();
                    testingFold = testingFold.ConcateVertical(testingKelas).Clone();
                    Matrix<int> responseKelas = response.GetRows((kelas * 50) + (fold * 5), 5 + (kelas * 50) + (fold * 5), 1).Clone();
                    responseFold = responseFold.ConcateVertical(responseKelas).Clone();
                    if (fold != 9)
                    {
                        Matrix<float> dataKelas = data.GetRows(5 + (kelas * 50) + (fold * 5), ((kelas + 1) * 50), 1).Clone();
                        dataFold = dataFold.ConcateVertical(dataKelas).Clone();
                        Matrix<int> targetKelas = response.GetRows(5 + (kelas * 50) + (fold * 5), ((kelas + 1) * 50), 1).Clone();
                        targetFold = targetFold.ConcateVertical(targetKelas).Clone();
                    }
                }

                dataFold = dataFold.RemoveRows(0, 1).Clone();
                testingFold = testingFold.RemoveRows(0, 1).Clone();
                targetFold = targetFold.RemoveRows(0, 1).Clone();
                responseFold = responseFold.RemoveRows(0, 1).Clone();

                Console.WriteLine("Fold-" + (fold+1));
                Console.WriteLine("train " + dataFold.Rows + " " + dataFold.Cols);
                Console.WriteLine("testing " + testingFold.Rows + " " + testingFold.Cols);
                Console.WriteLine("target train " + targetFold.Rows + " " + targetFold.Cols);
                Console.WriteLine("response test " + responseFold.Rows + " " + responseFold.Cols);

                /*if (fold == 1) {
                    for (int i = 0; i < dataFold.Rows; i++) {
                        Console.WriteLine(targetFold.Data[i, 0]);
                        for (int j = 0; j < dataFold.Cols; j++) {
                            Console.Write(dataFold.Data[i, j] + " ");
                        }
                        Console.WriteLine();
                    }
                }*/
                // Initialize SVM
                SVM model = new SVM();
                model.Type = SVM.SvmType.CSvc;
                model.SetKernel(SVM.SvmKernelType.Rbf);
                model.TermCriteria = new MCvTermCriteria(10000000, 0.0000001);
                model.Degree = 0.5;
                model.C = 2;
                //model.Coef0 = 1;
                //model.Gamma = 1;

                // Initialize TrainData
                //Matrix<int> respon = responses[j];
                TrainData trainData = new TrainData(dataFold, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, targetFold);

                bool training = model.Train(trainData);
                model.Save(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\" + "model" + fold + ".xml");
                Console.WriteLine(training);
                int acc = 0;
                for (int i = 0; i < testingFold.Rows; i++)
                {
                    float hasil = model.Predict(testingFold.GetRow(i));
                    Console.WriteLine(hasil + " " + responseFold.Data[i, 0]);
                    if (hasil == responseFold.Data[i, 0])
                        acc++;
                }
                akurasi += ((float)acc / (float)testingFold.Rows) * 100;
                model.Dispose();
                //double c = model.C;
                //Console.WriteLine(c);
                Console.WriteLine(((float)acc / (float)testingFold.Rows) * 100);

                int percentComplete = (int)(((float)(fold + 1) / (float)10) * 100);

                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }

            }
            akurasi /= 10;
            Console.WriteLine(akurasi);
        }

        // Thread evaluate model
        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            evaluateSVM(featureList, classes, worker, e);
        }

        // Progress bar evaluasi SVM
        private void backgroundWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar4.Value = e.ProgressPercentage;
        }

        // Action setelah evaluasi selesai
        private void backgroundWorker4_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                Console.WriteLine("Canceled");
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                this.metroButton7.Enabled = true;
            }
        }
    }
}
