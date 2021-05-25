using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Markup;
using Emgu;
using Emgu.CV;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using EMGU_Webcam_Testing;

public static class EmguTesting
{
    private const bool startVideoCapture = true;
    static List<ClassifierGroup> classifierGroups = new List<ClassifierGroup>();
    class ClassifierGroup
    {
        public CascadeClassifier classifier;
        public Color color;
    }
    static VideoCapture vc;
    static PictureBox pictureBox;
    static int frame = 0;
    static Mat grayImage = new Mat();
    static readonly string[] files = new string[]
    {
        // eyes
        //@"haar\haarcascade_eye_tree_eyeglasses.xml",
        //@"haar\haarcascade_lefteye_2splits.xml",
        //@"haar\haarcascade_righteye_2splits.xml",

        // faces
        //@"haar\haarcascade_profileface.xml",
        //@"haar\haarcascade_frontalface_alt.xml",
        //@"haar\haarcascade_frontalface_alt2.xml",
        //@"haar\haarcascade_frontalface_alt_tree.xml",
        @"haar\haarcascade_frontalface_default.xml",

        //@"haar\haarcascade_licence_plate_rus_16stages.xml",
        //@"haar\haarcascade_lowerbody.xml",
        //@"haar\haarcascade_russian_plate_number.xml",
        //@"haar\haarcascade_smile.xml",
        //@"haar\haarcascade_upperbody.xml",
    };
    static readonly Color[] colors = new Color[] {
        Color.Red,
        Color.Blue,
        Color.Green,
        Color.Yellow,
        Color.Orange,
        Color.Teal,
        Color.Purple,
        Color.Brown,
    };
    static Color RandomColor => Color.FromArgb(RandomNumberGenerator.GetInt32(int.MaxValue));
    static Label rectLabel;
    internal static void Init(PictureBox pb, Label label)
    {
        int i = 0;
        rectLabel = label;
        foreach (var item in files)
        {
            classifierGroups.Add(
                new ClassifierGroup()
                {
                    classifier = new CascadeClassifier(item),
                    color = colors[i % colors.Length],
                }
                );
            i++;

        }
        pictureBox = pb;

        if (startVideoCapture)
        {
            vc = new VideoCapture(0, VideoCapture.API.DShow);
            vc.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 240);
            vc.ImageGrabbed += OnGrabbed;
            vc.Start();
        }
    }
    static readonly Size minSize = new Size(30, 30);
    const int FILTER_LENGTH = 10;
    static Queue<Rectangle> rectChain = new Queue<Rectangle>(FILTER_LENGTH);
    static Rectangle avg = new Rectangle();
    private static void OnGrabbed(object sender, EventArgs e)
    {
        bool drawImage = true;
        var vc = sender as VideoCapture;
        vc.Retrieve(grayImage);
        CvInvoke.CvtColor(grayImage, grayImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
        Bitmap bitmap = null;

        if (drawImage)
            bitmap = grayImage.ToImage<Bgr, byte>().AsBitmap();
        foreach (var group in classifierGroups)
        {
            int i = 0;
            foreach (var rect in group.classifier.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.1,
                minNeighbors: 5,
                minSize: minSize
                ))
            {
                if (drawImage)
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        using (Pen pen = new Pen(group.color, 1))
                            graphics.DrawRectangle(pen, rect);
                        i++;
                    }
                }
                float n = rectChain.Count;
                while (rectChain.Count >= FILTER_LENGTH)
                    rectChain.Dequeue();
                rectChain.Enqueue(rect);
                float x = 0, y = 0, w = 0, h = 0;
                foreach (var item in rectChain)
                {
                    x += item.X / n;
                    y += item.Y / n;
                    w += item.Width / n;
                    h += item.Height / n;
                }
                Form1.labelText = ($"x:{(int)x}  y:{(int)y} || {(int)w} by {(int)h}");
                break;
            }
        }
        if (drawImage)
            pictureBox.Image = bitmap;
        Debug.WriteLine($"{frame++}: {DateTime.Now.ToString()}");
    }
}

