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
        private List<Matrix<double>> featureList = new List<Matrix<double>>();
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
            //string[] filePaths = Directory.GetFiles(@metroTextBox1.Text, "*.pgm");
            string[] filePaths = Directory.GetFiles(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\data kanker", "*.pgm");

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadImage(filePaths, worker, e);

            // Cleanup here
        }

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
                    CvInvoke.CLAHE(My_Image, 10, new Size(8, 8), CLAHEImage);
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

        // This event handler updates the progress bar.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar1.Value = e.ProgressPercentage;
        }

        // This event handler deals with the results of the
        // background operation.
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

        private void metroButton4_Click(object sender, EventArgs e)
        {
            // Cancel the asynchronous operation.
            this.backgroundWorker1.CancelAsync();
        }

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
                        Image<Gray, byte> My_Image = (Image<Gray, byte>)listImage[elements[0]];
                        int radius = Int32.Parse(elements[6]) + 14;
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

        // This event handler updates the progress bar.
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar2.Value = e.ProgressPercentage;
        }

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
                //ArrayList test = (ArrayList)roiImage[10];
                //imageBox1.SizeMode = PictureBoxSizeMode.Zoom;
                //imageBox1.Image = (Image<Gray, Byte>)test[2];
                //Console.WriteLine(test[0]);
            }
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            // Extracting Features
            backgroundWorker3.WorkerSupportsCancellation = true;
            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.RunWorkerAsync();
        }

        List<Matrix<double>> extractFeature(ArrayList roiImage, BackgroundWorker worker, DoWorkEventArgs e) {
            int totalImageCount = roiImage.Count;
            int i = 1;
            int highestPercentageReached = 0;
            List<Matrix<double>> leshFeatures = new List<Matrix<double>>();

            LESH leshExtractor = new LESH();
            foreach (ArrayList container in roiImage) {
                Image<Gray, double> im = (Image<Gray, double>)container[1];
                Matrix<double> leshFeature = leshExtractor.calc_LESH(im);
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

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = extractFeature(roiImage, worker, e);
        }

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
                featureList = (List<Matrix<double>>)e.Result;

                Console.WriteLine(featureList.Count);
                metroButton7.Enabled = true;
            }
        }

        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar3.Value = e.ProgressPercentage;
        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
            // Train SVM
            backgroundWorker4.WorkerSupportsCancellation = true;
            backgroundWorker4.WorkerReportsProgress = true;
            backgroundWorker4.RunWorkerAsync();
        }

        SVM trainSVM(List<Matrix<double>> samples, List<string> classes, BackgroundWorker worker, DoWorkEventArgs e) {
            int count = 0;

            // Initialize Sample
            Matrix<float> data = new Matrix<float>(samples.Count, samples[0].Cols);
            foreach (Matrix<double> sample in samples) {
                for (int cols = 0; cols < sample.Cols; cols++) {
                    data.Data[count, cols] = Convert.ToSingle(sample.Data[0, cols]);
                }
                count++;
            }

            // Initialize response
            Matrix<int> response = new Matrix<int>(classes.Count, 1);
            /*List<Matrix<int>> responses = new List<Matrix<int>>();
            Matrix<int> responseCALC = new Matrix<int>(classes.Count, 1);
            Matrix<int> responseCIRC = new Matrix<int>(classes.Count, 1);
            Matrix<int> responseSPIC = new Matrix<int>(classes.Count, 1);
            Matrix<int> responseMISC = new Matrix<int>(classes.Count, 1);
            Matrix<int> responseARCH = new Matrix<int>(classes.Count, 1);
            Matrix<int> responseASYM = new Matrix<int>(classes.Count, 1);*/
            
            count = 0;
            foreach (string kelas in classes) {
                Console.WriteLine(kelas);
                switch (kelas) {
                    case "CALC":
                        response.Data[count, 0] = 1;
                        break;
                    case "CIRC":
                        response.Data[count, 0] = 2;
                        break;
                    case "SPIC":
                        response.Data[count, 0] = 3;
                        break;
                    case "MISC":
                        response.Data[count, 0] = 4;
                        break;
                    case "ARCH":
                        response.Data[count, 0] = 5;
                        break;
                    case "ASYM":
                        response.Data[count, 0] = 6;
                        break;
                }
                /*if (kelas == "CALC") {
                    responseCALC.Data[count, 0] = 1;
                    responseCIRC.Data[count, 0] = -1;
                    responseSPIC.Data[count, 0] = -1;
                    responseMISC.Data[count, 0] = -1;
                    responseARCH.Data[count, 0] = -1;
                    responseASYM.Data[count, 0] = -1;
                }
                else if (kelas == "CIRC") {
                    responseCALC.Data[count, 0] = -1;
                    responseCIRC.Data[count, 0] = 1;
                    responseSPIC.Data[count, 0] = -1;
                    responseMISC.Data[count, 0] = -1;
                    responseARCH.Data[count, 0] = -1;
                    responseASYM.Data[count, 0] = -1;
                }
                else if (kelas == "SPIC") {
                    responseCALC.Data[count, 0] = -1;
                    responseCIRC.Data[count, 0] = -1;
                    responseSPIC.Data[count, 0] = 1;
                    responseMISC.Data[count, 0] = -1;
                    responseARCH.Data[count, 0] = -1;
                    responseASYM.Data[count, 0] = -1;
                }
                else if (kelas == "MISC") {
                    responseCALC.Data[count, 0] = -1;
                    responseCIRC.Data[count, 0] = -1;
                    responseSPIC.Data[count, 0] = -1;
                    responseMISC.Data[count, 0] = 1;
                    responseARCH.Data[count, 0] = -1;
                    responseASYM.Data[count, 0] = -1;
                }
                else if (kelas == "ARCH") {
                    responseCALC.Data[count, 0] = -1;
                    responseCIRC.Data[count, 0] = -1;
                    responseSPIC.Data[count, 0] = -1;
                    responseMISC.Data[count, 0] = -1;
                    responseARCH.Data[count, 0] = 1;
                    responseASYM.Data[count, 0] = -1;
                }
                else if (kelas == "ASYM") {
                    responseCALC.Data[count, 0] = -1;
                    responseCIRC.Data[count, 0] = -1;
                    responseSPIC.Data[count, 0] = -1;
                    responseMISC.Data[count, 0] = -1;
                    responseARCH.Data[count, 0] = -1;
                    responseASYM.Data[count, 0] = 1;
                }*/
                count++;
            }
            //Console.WriteLine(responseCALC.Rows + " " + responseCIRC.Rows + " " + responseSPIC.Rows + " " + responseMISC.Rows + " " + responseARCH.Rows + " " + responseASYM.Rows);
            /*responses.Add(responseCALC);
            responses.Add(responseCIRC);
            responses.Add(responseSPIC);
            responses.Add(responseMISC);
            responses.Add(responseARCH);
            responses.Add(responseASYM);*/

            // Initialize SVM
            SVM model = new SVM();
            model.SetKernel(SVM.SvmKernelType.Linear);
            model.Type = SVM.SvmType.CSvc;
            model.C = 5;
            model.TermCriteria = new MCvTermCriteria(10000, 0.00001);
            model.Gamma = 0.10000000000000001;

            // Initialize TrainData
            //Matrix<int> respon = responses[j];
            TrainData trainData = new TrainData(data, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, response);

            model.Train(trainData);

            worker.ReportProgress(100);

            return model;
        }

        // Train SVM Background
        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = trainSVM(featureList, classes, worker, e);
        }

        private void backgroundWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar4.Value = e.ProgressPercentage;
        }

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

        private void metroButton8_Click(object sender, EventArgs e)
        {
            // Save SVM
            backgroundWorker5.WorkerSupportsCancellation = true;
            backgroundWorker5.WorkerReportsProgress = true;
            backgroundWorker5.RunWorkerAsync();
        }

        void saveSVM(SVM modelSVM, string path, BackgroundWorker worker, DoWorkEventArgs e) {
            modelSVM.Save(path + "model.xml");
            worker.ReportProgress(100);
        }

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

        private void backgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string path = metroTextBox3.Text + "\\";
            saveSVM(modelSVM, path, worker, e);
        }

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
