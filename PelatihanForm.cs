using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Util;
using System.Runtime.InteropServices;

namespace TugasAkhir
{
    public partial class PelatihanForm : MetroFramework.Forms.MetroForm
    {
        private Hashtable listImage;
        private ArrayList roiImage = new ArrayList();
        private List<Matrix<float>> featureList = new List<Matrix<float>>();
        private List<string> classes = new List<string>();
        SVM modelSVM = new SVM();

        public PelatihanForm()
        {
            InitializeComponent();
        }

        private void Pelatihan_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        // Thread Load image
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file list here
            string[] filePaths = Directory.GetFiles(@metroTextBox1.Text, "*.pgm");
            //string[] filePaths = Directory.GetFiles(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\data kanker", "*.pgm");

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadImage(filePaths, worker, e);

            // Cleanup here
        }

        // Fungsi untuk meload citra dan mengenhance citra
        Hashtable loadImage(String[] paths, BackgroundWorker worker, DoWorkEventArgs e) {
            int totalImageCount = paths.Length;
            Hashtable listImage = new Hashtable();
            int highestPercentageReached = 0;

            for (int i = 1; i <= totalImageCount; i++) {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else {
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
                foreach (DictionaryEntry elemen in listImage) {
                    Console.WriteLine(elemen.Key);
                }
                Console.WriteLine(listImage.Count);
            }

            // Enable the Start button.
            metroButton3.Enabled = true;

        }

        // Memilih folder citra
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

        // Menjalankan load citra
        private void metroButton3_Click_2(object sender, EventArgs e)
        {
            // Start loading images
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();

            // Disable the Start button until 
            // the asynchronous operation is done.
            this.metroButton3.Enabled = false;

            // Enable the Cancel button while 
            // the asynchronous operation runs.
        }


        // Cancel load citra
        private void metroButton4_Click(object sender, EventArgs e)
        {
            // Cancel the asynchronous operation.
            this.backgroundWorker1.CancelAsync();
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
        }

        // Thread get ROI from images
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file here
            string roiPaths = @metroTextBox2.Text;

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadROI(roiPaths, worker, e);
        }

        // Fungsi untuk load ROI
        ArrayList loadROI(String paths, BackgroundWorker worker, DoWorkEventArgs e) {
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
                if (worker.CancellationPending) {
                    e.Cancel = true;
                    break;
                }
                else {
                    elements = line.Split(' ');
                    if (!elements[2].Equals("NORM") && !elements[4].Equals("*NOTE3*")) {
                        Console.WriteLine(elements[0]);
                        Image<Gray, byte> My_Image = (Image<Gray, byte>)listImage[elements[0]];
                        int radius = 0;
                        // Mengambil radius di dibuat untuk habis dibagi 4 (Kebutuhan LESH)
                        if ((Int32.Parse(elements[6]) * 2) % 4 == 0) {
                            radius = Int32.Parse(elements[6]) * 2;
                        }
                        else {
                            radius = (Int32.Parse(elements[6]) * 2) + (4 - ((Int32.Parse(elements[6]) * 2) % 4));
                        }
                        float jejari = radius / 2;
                        int x = Convert.ToInt32(Int32.Parse(elements[4]) - jejari);
                        int y = Convert.ToInt32(1024 - Int32.Parse(elements[5]) - jejari);

                        Image<Gray, double> newImage = My_Image.Copy(new Rectangle(x, y, radius, radius)).Convert<Gray, double>();

                        container.Add(elements[0]);                 // File name
                        container.Add(newImage);                    // Image
                        container.Add(elements[2]);                 // Calsification
                        result.Add(container);

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

                foreach (ArrayList container in roiImage) {
                    classes.Add(container[2].ToString());
                }

                Console.WriteLine(roiImage.Count);
                metroButton6.Enabled = true;
            }
        }

        // Menjalankan ekstraksi fitur
        private void metroButton6_Click(object sender, EventArgs e)
        {
            // Extracting Features
            backgroundWorker3.WorkerSupportsCancellation = true;
            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.RunWorkerAsync();
        }

        // Fungsi ekstraksi fitur
        List<Matrix<float>> extractFeature(ArrayList roiImage, BackgroundWorker worker, DoWorkEventArgs e) {
            int totalImageCount = roiImage.Count;
            int i = 1;
            int highestPercentageReached = 0;
            List<Matrix<float>> leshFeatures = new List<Matrix<float>>();

            LESH leshExtractor = new LESH();
            foreach (ArrayList container in roiImage) {
                Image<Gray, double> im = (Image<Gray, double>)container[1];
                Matrix<float> leshFeature = leshExtractor.calc_LESH(im);
                leshFeatures.Add(leshFeature);

                int percentComplete = (int)((float)i / (float)totalImageCount * 100);

                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
                i++;
            }

            return leshFeatures;
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
        }

        // Fungsi untuk melatih SVM
        SVM trainSVM(List<Matrix<float>> samples, List<string> classes, BackgroundWorker worker, DoWorkEventArgs e) {
            int count = 0;

            // Initialize Sample
            Matrix<float> data = new Matrix<float>(samples.Count, samples[0].Cols);
            foreach (Matrix<float> sample in samples) {
                for (int cols = 0; cols < sample.Cols; cols++) {
                    data.Data[count, cols] = Convert.ToSingle(sample.Data[0, cols]);
                }
                count++;
            }

            // Initialize response
            Matrix<int> response = new Matrix<int>(classes.Count, 1);
            
            count = 0;
            foreach (string kelas in classes) {
                //Console.WriteLine(kelas);
                switch (kelas) {
                    case "CALC":
                        response.Data[count, 0] = 0;
                        break;
                    case "CIRC":
                        response.Data[count, 0] = 1;
                        break;
                    case "SPIC":
                        response.Data[count, 0] = 2;
                        break;
                    case "MISC":
                        response.Data[count, 0] = 3;
                        break;
                    case "ARCH":
                        response.Data[count, 0] = 4;
                        break;
                    case "ASYM":
                        response.Data[count, 0] = 5;
                        break;
                }
                count++;
            }
            /*for (int i = 0; i < response.Rows; i++)
            {
                Console.WriteLine("[" + (i + 1) + "] = " + response.Data[i, 0]);
            }*/

            // Initialize SVM
            SVM model = new SVM();
            model.Type = SVM.SvmType.CSvc;
            model.SetKernel(SVM.SvmKernelType.Poly);
            model.TermCriteria = new MCvTermCriteria(100, 0.00001);
            model.Degree = 1;
            model.C = 1;
            model.Coef0 = 1;
            model.Gamma = 1;

            // Initialize TrainData
            //Matrix<int> respon = responses[j];
            TrainData trainData = new TrainData(data, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, response);

            bool training = model.TrainAuto(trainData, 4);
            Console.WriteLine(training);
            float hasil = model.Predict(data.GetRow(35));
            Console.WriteLine(response.Data[35,0] + " " + hasil);
            worker.ReportProgress(100);

            return model;
        }

        // Train SVM Background
        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = trainSVM(featureList, classes, worker, e);
        }

        // Progress bar pelatihan SVM
        private void backgroundWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar4.Value = e.ProgressPercentage;
        }

        // Action setelah pelatihan selesai
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
                modelSVM = (SVM)e.Result;
                metroButton9.Enabled = true;
            }
        }

        // Menyimpan SVM ke dalam file
        private void metroButton8_Click(object sender, EventArgs e)
        {
            // Save SVM
            backgroundWorker5.WorkerSupportsCancellation = true;
            backgroundWorker5.WorkerReportsProgress = true;
            backgroundWorker5.RunWorkerAsync();
        }

        // Fungsi untuk menyimpan SVM
        void saveSVM(SVM modelSVM, string path, BackgroundWorker worker, DoWorkEventArgs e) {
            modelSVM.Save(path + "model.xml");
            worker.ReportProgress(100);
        }

        // Memilih lokasi penyimpanan SVM
        private void metroButton9_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string namaFolder = folderBrowserDialog1.SelectedPath;
                metroTextBox3.Text = namaFolder;
                metroButton8.Enabled = true;
            }
        }

        // Thread untuk menyimpan SVM
        private void backgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string path = metroTextBox3.Text + "\\";
            saveSVM(modelSVM, path, worker, e);
        }

        // Action ketika SVM berhasil disimpan
        private void backgroundWorker5_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar5.Value = e.ProgressPercentage;
        }

        private void PelatihanForm_Load(object sender, EventArgs e)
        {

        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            var main = (Form1)Tag;
            main.Show();
            Hide();
        }
    }
}
