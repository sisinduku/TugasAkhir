using Emgu.CV;
using Emgu.CV.ML;
using Emgu.CV.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TugasAkhir
{
    public partial class PengujianForm : MetroFramework.Forms.MetroForm
    {
        private Hashtable listImage;
        private ArrayList roiImage = new ArrayList();
        List<Matrix<double>> feature = new List<Matrix<double>>();
        List<string> classes = new List<string>();
        SVM modelSVM = new SVM();

        public PengujianForm()
        {
            InitializeComponent();
        }
        private void PengujianForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            var main = (Form1)Tag;
            main.Show();
            Hide();
        }

        // Memilih direktori citra
        private void metroButton1_Click(object sender, EventArgs e)
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
        }

        // Fungsi untuk load citra
        Hashtable loadImage(String[] paths, BackgroundWorker worker, DoWorkEventArgs e) {
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

        // Thread untuk load citra
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file here
            //string[] imagePath = Directory.GetFiles(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\testing", "*.pgm");
            string[] imagePath = Directory.GetFiles(@metroTextBox1.Text, "*.pgm");

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadImage(imagePath, worker, e);
        }

        // Progress bar load citra
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar1.Value = e.ProgressPercentage;
        }

        // Action setelah load citra selesai
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
                metroButton2.Enabled = true;
            }
        }

        // Memilih ROI
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

        // Memilih lokasi SVM
        private void metroButton4_Click_1(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string namaFile = openFileDialog1.InitialDirectory + openFileDialog1.FileName;
                metroTextBox3.Text = namaFile;
                metroButton8.Enabled = true;
            }
        }

        // Load SVM
        private void metroButton8_Click(object sender, EventArgs e)
        {
            // Start loading images
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.RunWorkerAsync();
        }

        // Fungsi untuk load SVM
        SVM loadModel(String paths, BackgroundWorker worker, DoWorkEventArgs e) {
            worker.ReportProgress(0);
            SVM model = new SVM();
            FileStorage fsr = new FileStorage(@paths, FileStorage.Mode.Read);
            model.Read(fsr.GetFirstTopLevelNode());
            worker.ReportProgress(100);
            return model;
        }
        
        // Thread untuk load SVM
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file here
            string modelPath = @metroTextBox3.Text;

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadModel(modelPath, worker, e);
        }

        // Progress bar load SVM
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar5.Value = e.ProgressPercentage;
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
                modelSVM = (SVM)e.Result;
                
                metroButton1.Enabled = true;
            }
        }

        // Load ROI
        private void metroButton5_Click(object sender, EventArgs e)
        {
            // Start loading roi
            backgroundWorker3.WorkerSupportsCancellation = true;
            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.RunWorkerAsync();
        }


        // Fungsi untuk load ROI
        ArrayList loadROI(String paths, BackgroundWorker worker, DoWorkEventArgs e) {
            string line;
            string[] elements;
            ArrayList result = new ArrayList();
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

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            // Load file here
            string roiPath = @metroTextBox2.Text;

            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = loadROI(roiPath, worker, e);
        }

        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar2.Value = e.ProgressPercentage;
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
                roiImage = (ArrayList)e.Result;
                foreach (ArrayList container in roiImage)
                {
                    classes.Add(container[2].ToString());
                }
                metroButton6.Enabled = true;
            }
        }

        // Ekstraksi fitur
        private void metroButton6_Click(object sender, EventArgs e)
        {
            // Start extracting feature
            backgroundWorker4.WorkerSupportsCancellation = true;
            backgroundWorker4.WorkerReportsProgress = true;
            backgroundWorker4.RunWorkerAsync();
        }

        // Fungsi untuk ekstraksi fitur
        List<Matrix<double>> extractFeature(ArrayList roiImage, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int totalImageCount = roiImage.Count;
            int i = 1;
            int highestPercentageReached = 0;
            List<Matrix<double>> leshFeatures = new List<Matrix<double>>();

            LESH leshExtractor = new LESH();
            foreach (ArrayList container in roiImage)
            {
                Console.WriteLine((string)container[0]);
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

        // Thread untuk ekstraksi fitur
        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = extractFeature(roiImage, worker, e);
        }

        // Progress bar ekstraksi fitur
        private void backgroundWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar3.Value = e.ProgressPercentage;
        }

        // Action setelah ekstraksi fitur selesai
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
                feature = (List<Matrix<double>>)e.Result;                
                metroButton7.Enabled = true;
            }
        }

        // Klasifikasi citra
        private void metroButton7_Click(object sender, EventArgs e)
        {
            // Start extracting feature
            backgroundWorker5.WorkerSupportsCancellation = true;
            backgroundWorker5.WorkerReportsProgress = true;
            backgroundWorker5.RunWorkerAsync();
        }

        // Fungsi untuk klasifikasi
        string klasifikasi(List<Matrix<double>> feature, BackgroundWorker worker, DoWorkEventArgs e) {
            worker.ReportProgress(0);
            string result = "";
            Matrix<float> fitur = new Matrix<float>(feature.Count, feature[0].Cols);
            int count = 0;
            foreach (Matrix<double> elemen in feature) {
                for (int cols = 0; cols < elemen.Cols; cols++)
                {
                    fitur.Data[count, cols] = Convert.ToSingle(elemen.Data[0, cols]);
                }
                count++;
            }
            Console.WriteLine(modelSVM.C);
            for (int i = 0; i < fitur.Rows; i++) {
                float hasil = modelSVM.Predict(fitur.GetRow(i));
                int target = -1;
                string kelas = classes[i];
                switch (kelas)
                {
                    case "CALC":
                        target = 0;
                        break;
                    case "CIRC":
                        target = 1;
                        break;
                    case "SPIC":
                        target = 2;
                        break;
                    case "MISC":
                        target = 3;
                        break;
                    case "ARCH":
                        target = 4;
                        break;
                    case "ASYM":
                        target = 5;
                        break;
                }
                Console.WriteLine(target + " " + hasil);
                worker.ReportProgress((int)((float)i / (float)(fitur.Rows - 1) * 100));
            }
            return result;
        }

        private void backgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = klasifikasi(feature, worker, e);
        }

        private void backgroundWorker5_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.metroProgressBar4.Value = e.ProgressPercentage;
        }

        private void backgroundWorker5_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                string hasil = (string)e.Result;
                Console.WriteLine(hasil);
            }
        }
    }
}
