using Emgu.CV;
using FaceTracking;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EMGU_Webcam_Testing
{
    public partial class Form1 : Form
    {
        internal static string labelText;

        public static Form1 instance { get; private set; }
        public Form1()
        {
            instance = this;
            InitializeComponent();
        }

        FaceTracker tracker;
        Mat m;
        private void Form1_Load(object sender, EventArgs e)
        {
            m = new Mat();
            tracker = new FaceTracker(@"haar/haarcascade_frontalface_default.xml", 10);
            new WebcamCapture(OnNewImage);
        }

        private void OnNewImage(object sender, EventArgs e)
        {
            var vc = sender as VideoCapture;
            if (vc == null)
                return;
            vc.Retrieve(m);
            CvInvoke.CvtColor(m, m, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            pictureBox1.Image = tracker.ProcessImage(m,false);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = labelText;
        }
    }
}
