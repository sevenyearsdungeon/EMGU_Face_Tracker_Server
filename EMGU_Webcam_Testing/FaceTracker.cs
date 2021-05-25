using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FaceTracking
{
    public class FaceTracker
    {
        public static byte[] data { get; private set; }

        static readonly Size minSize = new Size(60, 60);

        int filterLength;
        Color color = Color.Red;
        CascadeClassifier classifier;

        Queue<Rectangle> faceRectFilter;
        public Rectangle runningAvgRect => _runningAvgRect;
        Rectangle _runningAvgRect;

        public FaceTracker(string classifierPath, int filterLength)
        {
            this.classifier = new CascadeClassifier(classifierPath);
            this.filterLength = filterLength;
            this.faceRectFilter = new Queue<Rectangle>(filterLength);
            _runningAvgRect = new Rectangle();
        }

        public Bitmap ProcessImage(Mat grayImage, bool returnImage = true)
        {
            Bitmap bitmap = null;
            if (returnImage)
                bitmap = grayImage.ToImage<Bgr, byte>().AsBitmap();
            var faces = classifier.DetectMultiScale(image: grayImage, scaleFactor: 1.1, minNeighbors: 5, minSize: minSize);
            var centerFaceIndex = GetCenterRect(faces, grayImage.SizeOfDimension);
            {
                for (int i = 0; i < faces.Length; i++)
                {
                    if (returnImage)
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            using (Pen pen = new Pen((i == centerFaceIndex) ? color : Color.Blue, 1))
                                graphics.DrawRectangle(pen, faces[i]);
                        }
                    }
                    if (i == centerFaceIndex)
                    {
                        float n = faceRectFilter.Count;
                        while (faceRectFilter.Count >= filterLength)
                            faceRectFilter.Dequeue();
                        faceRectFilter.Enqueue(faces[i]);
                        float x = 0, y = 0, w = 0, h = 0;
                        foreach (var item in faceRectFilter)
                        {
                            x += item.X / n;
                            y += item.Y / n;
                            w += item.Width / n;
                            h += item.Height / n;
                        }
                        _runningAvgRect.X = (int)x;
                        _runningAvgRect.Y = (int)y;
                        _runningAvgRect.Width = (int)w;
                        _runningAvgRect.Height = (int)h;
                        if (returnImage)
                        {
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                using (Pen pen = new Pen(Color.Red, 2))
                                    graphics.DrawRectangle(pen, _runningAvgRect);
                            }
                        }
                    }
                }
            }
            PackData(grayImage.Width, grayImage.Height, centerFaceIndex, faces);
            return bitmap;
        }

        private void PackData(int w, int h, int centerIdx, Rectangle[] rects)
        {
            using (MemoryStream ms = new MemoryStream(8+4+4+4+16*rects.Length))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(DateTime.Now.ToBinary());
                    bw.Write(w);
                    bw.Write(h);
                    bw.Write(centerIdx);
                    foreach (var item in rects)
                    {
                        bw.Write(item.Left);
                        bw.Write(item.Top);
                        bw.Write(item.Width);
                        bw.Write(item.Height);
                    }
                }
                data = ms.GetBuffer();
            }
        }

        private int GetCenterRect(Rectangle[] faces, int[] dims)
        {
            float ch = dims[0] / 2;
            float cw = dims[1] / 2;
            float mindist = float.MaxValue;
            int result = -1;
            int i = 0;
            foreach (var rect in faces)
            {
                var dx = cw - (rect.Left + rect.Width / 2);
                var dy = ch - (rect.Top + rect.Height / 2);
                var dist = dx * dx + dy * dy;
                if (dist < mindist)
                {
                    mindist = dist;
                    result = i;
                }
                i++;
            }
            return result;
        }
    }
}
