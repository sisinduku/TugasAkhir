using Emgu.CV;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TugasAkhir
{
    public partial class PelatihanForm : MetroFramework.Forms.MetroForm
    {        
        private Hashtable listImage;
        private ArrayList roi;
        private ArrayList roiImage = new ArrayList();

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
            string[] filePaths = Directory.GetFiles(@"E:\Data\Project\TA\Diagnosa kanker payudara dengan SVM dan ekstraksi fitur LESH\core\all-mias", "*.pgm");

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
                    Image<Gray, double> My_Image = new Image<Gray, double>(@paths[i-1]);
                    Image<Gray, double> CLAHEImage = My_Image;
                    CvInvoke.CLAHE(My_Image, 40, new Size(8, 8), CLAHEImage);
                    String fullFileName = paths[i - 1].Split('\\', '/').Last();
                    String fileName = fullFileName.Split('.').First();                    
                    listImage.Add(fileName, CLAHEImage);

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

            // Disable the Cancel button.
            metroButton4.Enabled = false;
            
        }

        private void metroButton1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string namaFolder = folderBrowserDialog1.SelectedPath;
                metroTextBox1.Text = namaFolder;
                metroButton3.Enabled = true;
                metroButton4.Enabled = true;
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
            this.metroButton4.Enabled = true;
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            // Cancel the asynchronous operation.
            this.backgroundWorker1.CancelAsync();

            // Disable the Cancel button.
            metroButton4.Enabled = false;
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
                        container.Add(elements[0]);                 // File name
                        container.Add(Int32.Parse(elements[4]));    // X0
                        container.Add(Int32.Parse(elements[5]));    // Y0    
                        container.Add(Int32.Parse(elements[6]));    // Radius
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
                roi = (ArrayList)e.Result;

                foreach (ArrayList row in roi) {
                    ArrayList container = new ArrayList();
                    Image<Gray, double> My_Image = (Image<Gray, double>)listImage[row[0]];
                    int x = (int)row[1];
                    int y = 1024 - (int)row[2];
                    int radius = (int)row[3];

                    Image<Gray, double> newImage = My_Image.Copy(new Rectangle(x, y, radius, radius));
                    Matrix<double> imageMatrix = new Matrix<double>(radius, radius);
                    newImage.CopyTo(imageMatrix);

                    container.Add(row[0]);          // File name
                    container.Add(row[3]);          // Radius
                    container.Add(imageMatrix);     // ROI Image
                    roiImage.Add(container);
                }
                Console.WriteLine(roiImage.Count);
                //ArrayList test = (ArrayList)roiImage[10];
                //imageBox1.SizeMode = PictureBoxSizeMode.Zoom;
                //imageBox1.Image = (Image<Gray, Byte>)test[2];
                //Console.WriteLine(test[0]);
            }
        }
    }
}
