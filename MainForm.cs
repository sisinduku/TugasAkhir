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

        private void metroButton2_Click(object sender, EventArgs e)
        {
            PengujianForm pengujian = new PengujianForm();
            pengujian.Tag = this;
            pengujian.Show(this);
            //HiddenForms.Add(this);
            Hide();
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            Debug debug = new Debug();
            debug.Tag = this;
            debug.Show(this);
            Hide();
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
