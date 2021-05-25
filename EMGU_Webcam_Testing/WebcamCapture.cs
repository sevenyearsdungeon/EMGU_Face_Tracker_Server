using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Text;

namespace FaceTracking
{
    public class WebcamCapture
    {
        VideoCapture capture;
        public WebcamCapture(EventHandler onImageGrabbed, int index = -1)
        {
            if (index < 0)
                capture = new VideoCapture(captureApi: VideoCapture.API.DShow);
            else
                capture = new VideoCapture(index, captureApi: VideoCapture.API.DShow);
            capture.ImageGrabbed += onImageGrabbed;
            // capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            // capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);
            capture.Start();
        }
    }
}
