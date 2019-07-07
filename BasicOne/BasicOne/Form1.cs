using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

using LightBuzz.Vitruvius;

namespace BasicOne
{
    public partial class Form1 : Form
    {
        //启用的Kinect设备
        private KinectSensor sensor;
        FaceTracker faceTracker;
        FaceTrackFrame faceFrame;

        //数据缓冲存储空间
        ColorImageFormat colorImageFormat;
        DepthImageFormat depthImageFormat;

        private byte[] colorPixels;
        private DepthImagePixel[] depthPixels;
        private byte[] depthPixels4Channels;
        private short[] depthPixelsShort;
        private Skeleton[] skeletonData;
                
        //显示图像中间结构  Kinect -> Bitmap -> Emgu CV
        private Bitmap colBitmap;
        private Bitmap depBitmap;

        int depthWidth, depthHeight;
        int colorWidth, colorHeight;

        private Image<Bgr, Byte> colorImage;
        private Image<Bgr, Byte> depthImage;

        //EmguCV 绘制字符串时候使用的字体
        private MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 0.3, 0.3);
        //绘制骨骼使用的Image
        private Image<Bgr, Byte> skeletonImage;

        private ColorImagePoint[] colorCoordinates;


        //语音识别引擎
        private SpeechRecognitionEngine speechEngine;


        GestureController _gestureController;


        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENT_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENT_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENT_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENT_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENT_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENT_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENT_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENT_ABSOLUTE = 0x8000;

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern void keybd_event(
            byte bVk, //虚拟键值
            byte bScan,// 一般为0
            int dwFlags, //这里是整数类型 0 为按下，2为释放
            int dwExtraInfo //这里是整数类型 一般情况下设成为 0
            );
        const int KEY_DOWN = 0;
        const int KEY_UP = 2;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //停止设备
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
            }

            //退出语音识别
            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }      
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //枚举Kinect设备，并将第一个连接成功的设备做为当前设备
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                Console.WriteLine("OK1");
                //初始化Kinect设置            
                colorImageFormat = ColorImageFormat.RgbResolution640x480Fps30;
                depthImageFormat = DepthImageFormat.Resolution640x480Fps30;

                this.sensor.ColorStream.Enable(colorImageFormat);
                this.sensor.DepthStream.Enable(depthImageFormat);
                this.sensor.SkeletonStream.Enable();
                
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                this.depthPixelsShort = new short[this.sensor.DepthStream.FramePixelDataLength];
                this.depthPixels4Channels = new byte[this.sensor.DepthStream.FramePixelDataLength*4];
                this.skeletonData = new Skeleton[this.sensor.SkeletonStream.FrameSkeletonArrayLength];

                this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

                depthWidth = this.sensor.DepthStream.FrameWidth;
                depthHeight = this.sensor.DepthStream.FrameHeight;

                colorWidth = this.sensor.ColorStream.FrameWidth;
                colorHeight = this.sensor.ColorStream.FrameHeight;

                skeletonImage = new Image<Bgr, byte>(depthWidth, depthHeight);
                skeletonImage.Draw(new Rectangle(0, 0, depthWidth, depthHeight), new Bgr(0.0, 0.0, 0.0), -1);
                imageBoxSkeleton.Image = skeletonImage;

                this.sensor.ColorFrameReady += this.SensorColorFrameReady;
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                //this.sensor.AllFramesReady += this.SensorAllFrameReady;

                _gestureController = new GestureController(GestureType.All);
                _gestureController.GestureRecognized += GestureController_GestureRecognized;

                //启动设备
                try
                {
                    this.sensor.Start();
                }
                catch (System.IO.IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                MessageBox.Show("Kinect设备未准备好");
            }
            else
            {
                this.Text = "Kinect连接成功";

                try
                {
                    faceTracker = new FaceTracker(sensor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this.Text = ex.Message;
                }
            }

            //获取Kinect语音识别引擎
            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                //创建引擎
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);
                
                //创建识别语法
                var directions = new Choices();
                directions.Add(new SemanticResultValue("one", 1));
                directions.Add(new SemanticResultValue("two", 2));
                directions.Add(new SemanticResultValue("three", 3));
                directions.Add(new SemanticResultValue("four", 4));
                directions.Add(new SemanticResultValue("five", 5));

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(directions);
                var g = new Grammar(gb);
                speechEngine.LoadGrammar(g);

                //添加识别处理函数
                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;

                //设置语音引擎参数
                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(),
                    new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

                //启用识别
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        void GestureController_GestureRecognized(object sender, GestureEventArgs e)
        {
            Text = e.Name;            
                    
            switch (e.Type)
            {
                case GestureType.JoinedHands:
                    break;
                case GestureType.Menu:
                    break;
                case GestureType.SwipeDown:
                    break;
                case GestureType.SwipeLeft:
                    {
                        mouse_event(MOUSEEVENT_MOVE, -10, 0, 0, 0);
                    }
                    break;
                case GestureType.SwipeRight:
                    {
                        mouse_event(MOUSEEVENT_MOVE, 10, 0, 0, 0);
                    }
                    break;
                case GestureType.SwipeUp:
                    break;
                case GestureType.WaveLeft:
                    {                        
                        keybd_event((byte)Keys.PageDown, 0, KEY_DOWN, 0);
                        keybd_event((byte)Keys.PageDown, 0, KEY_UP, 0);
                    }
                    break;
                case GestureType.WaveRight:
                    {
                        keybd_event((byte)Keys.PageUp, 0, KEY_DOWN, 0);
                        keybd_event((byte)Keys.PageUp, 0, KEY_UP, 0);
                    }
                    break;
                case GestureType.ZoomIn:
                    break;
                case GestureType.ZoomOut:
                    break;
                default:
                    break;
            }
        }

        //识别处理函数
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                this.Text = e.Result.Semantics.Value.ToString() + ", " + e.Result.Confidence.ToString();
                //MessageBox.Show(e.Result.Semantics.Value.ToString());
            }
        }

        //识别拒绝函数，基本无意义
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
        }

        //获取Kinect语音识别引擎
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && 
                    "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }
        
        //骨骼数据处理事件
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool received = false;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                    received = true;
                }
            }

            if (received)
            {
                foreach (Skeleton skeleton in this.skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        _gestureController.Update(skeleton);
                    }
                }

                //重绘整个画面，冲掉原有骨骼图像
                skeletonImage.Draw(new Rectangle(0, 0, skeletonImage.Width, skeletonImage.Height), new Bgr(0.0, 0.0, 0.0), -1);
            
                DrawSkeletons(skeletonImage, 0);
                imageBoxSkeleton.Image = skeletonImage;
            }
        }
        
        private void DrawSkeletons(Image<Bgr,Byte> img, int depthOrColor)
        {
            //绘制所有正确Tracked的骨骼
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton == null) continue;
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    DrawTrackedSkeletonJoints(img, skeleton.Joints, depthOrColor);
                }
            }
        }

        private void DrawTrackedSkeletonJoints(Image<Bgr, Byte> img, JointCollection jointCollection, int depthOrColor)
        {
            // Render Head and Shoulders
            DrawBone(img, jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter], depthOrColor);
            DrawBone(img, jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft], depthOrColor);
            DrawBone(img, jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight], depthOrColor);

            // Render Left Arm
            DrawBone(img, jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft], depthOrColor);
            DrawBone(img, jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft], depthOrColor);
            DrawBone(img, jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft], depthOrColor);

            // Render Right Arm
            DrawBone(img, jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight], depthOrColor);
            DrawBone(img, jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight], depthOrColor);
            DrawBone(img, jointCollection[JointType.WristRight], jointCollection[JointType.HandRight], depthOrColor);

            // Render other bones...
            DrawBone(img, jointCollection[JointType.ShoulderCenter], jointCollection[JointType.Spine], depthOrColor);

            DrawBone(img, jointCollection[JointType.Spine], jointCollection[JointType.HipRight], depthOrColor);
            DrawBone(img, jointCollection[JointType.KneeRight], jointCollection[JointType.HipRight], depthOrColor);
            DrawBone(img, jointCollection[JointType.KneeRight], jointCollection[JointType.AnkleRight], depthOrColor);
            DrawBone(img, jointCollection[JointType.FootRight], jointCollection[JointType.AnkleRight], depthOrColor);

            DrawBone(img, jointCollection[JointType.Spine], jointCollection[JointType.HipLeft], depthOrColor);
            DrawBone(img, jointCollection[JointType.KneeLeft], jointCollection[JointType.HipLeft], depthOrColor);
            DrawBone(img, jointCollection[JointType.KneeLeft], jointCollection[JointType.AnkleLeft], depthOrColor);
            DrawBone(img, jointCollection[JointType.FootLeft], jointCollection[JointType.AnkleLeft], depthOrColor);
        }

        private void DrawBone(Image<Bgr, Byte> img, Joint jointFrom, Joint jointTo, int depthOrColor)
        {
            if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
            jointTo.TrackingState == JointTrackingState.NotTracked)
            {
                return; // nothing to draw, one of the joints is not tracked
            }

            if (jointFrom.TrackingState == JointTrackingState.Inferred ||
            jointTo.TrackingState == JointTrackingState.Inferred)
            {
                DrawBoneLine(img, jointFrom.Position, jointTo.Position, 1, depthOrColor);
            }

            if (jointFrom.TrackingState == JointTrackingState.Tracked &&
            jointTo.TrackingState == JointTrackingState.Tracked)
            {
                DrawBoneLine(img, jointFrom.Position, jointTo.Position, 3, depthOrColor);
            }
        }

        private System.Drawing.Point SkeletonPointToDepthScreen(SkeletonPoint skelpoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, depthImageFormat);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }

        private System.Drawing.Point SkeletonPointToColorScreen(SkeletonPoint skelpoint)
        {
            ColorImagePoint colorPoint = this.sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skelpoint, colorImageFormat);
            return new System.Drawing.Point(colorPoint.X, colorPoint.Y);
        }

        private void DrawBoneLine(Image<Bgr, Byte> img, SkeletonPoint p1, SkeletonPoint p2, int lineWidth, int depthOrColor)
        {
            System.Drawing.Point p_1, p_2;

            //depthOrColor = 0;
            if (depthOrColor == 0)
            {
                p_1 = SkeletonPointToDepthScreen(p1);
                p_2 = SkeletonPointToDepthScreen(p2);
            }
            else
            {
                p_1 = SkeletonPointToColorScreen(p1);
                p_2 = SkeletonPointToColorScreen(p2);
            }

            img.Draw(new LineSegment2D(p_1, p_2), new Bgr(255, 255, 0), lineWidth);
            img.Draw(new CircleF(p_1, 5), new Bgr(0, 0, 255), -1);

            StringBuilder str = new StringBuilder();
            str.AppendFormat("({0},{1},{2})", p1.X.ToString("0.0"), p1.Y.ToString("0.0"), p1.Z.ToString("0.0"));

            img.Draw(str.ToString(), ref font, p_1, new Bgr(0, 255, 0));
            img.Draw(new CircleF(p_2, 5), new Bgr(0, 0, 255), -1);

            str.Clear();
            str.AppendFormat("({0},{1},{2})", p2.X.ToString("0.0"), p2.Y.ToString("0.0"), p2.Z.ToString("0.0"));
            img.Draw(str.ToString(), ref font, p_2, new Bgr(0, 255, 0));            
        }

        private void SensorAllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                }
                else
                    return;
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyPixelDataTo(this.depthPixelsShort);
                }
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                }
            }

            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton == null) continue;

                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    _gestureController.Update(skeleton);

                    faceFrame = faceTracker.Track(this.sensor.ColorStream.Format, this.colorPixels,
                        this.sensor.DepthStream.Format, this.depthPixelsShort, skeleton);                    
                }
            }

            colBitmap = new Bitmap(colorWidth, colorHeight);

            BitmapData colImageBitmapData = colBitmap.LockBits(new Rectangle(0, 0, colBitmap.Width, colBitmap.Height),
                                            ImageLockMode.WriteOnly,
                                            colBitmap.PixelFormat);
            IntPtr IptrColImage = colImageBitmapData.Scan0;

            Marshal.Copy(colorPixels, 0, IptrColImage, colorPixels.Length);
            colBitmap.UnlockBits(colImageBitmapData);

            //显示Bitmap
            UpdateColImage(colBitmap);
        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            bool received = false;
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    //缓存数据
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    received = true;
                }
            }

            if (received)
            {
                //将byte数组转为Bitmap
                colBitmap = new Bitmap(colorWidth, colorHeight);

                BitmapData colImageBitmapData = colBitmap.LockBits(new Rectangle(0, 0, colBitmap.Width, colBitmap.Height),
                                                ImageLockMode.WriteOnly,
                                                colBitmap.PixelFormat);
                IntPtr IptrColImage = colImageBitmapData.Scan0;

                Marshal.Copy(colorPixels, 0, IptrColImage, colorPixels.Length);
                colBitmap.UnlockBits(colImageBitmapData);

                //显示Bitmap
                UpdateColImage(colBitmap);
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            bool received = false;
            int minDepth = 1000;
            int maxDepth = 0;
            
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    //获取有效深度范围
                    minDepth = depthFrame.MinDepth;
                    maxDepth = depthFrame.MaxDepth;

                    //缓存深度数据
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
                    depthFrame.CopyPixelDataTo(this.depthPixelsShort);
                    received = true;
                }
            }
            
            if (received)
            {
                this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    depthImageFormat,
                    this.depthPixels,
                    colorImageFormat,
                    this.colorCoordinates);

                //将深度数据转为Bitmap
                depBitmap = new Bitmap(depthWidth, depthHeight);
                    
                BitmapData depImageBitmapData = depBitmap.LockBits(new Rectangle(0, 0, depBitmap.Width, depBitmap.Height),
                                                ImageLockMode.WriteOnly,
                                                depBitmap.PixelFormat);
                IntPtr IptrDepImage = depImageBitmapData.Scan0;


                for (int y = 0; y < this.depthHeight; ++y)
                {
                    for (int x = 0; x < this.depthWidth; ++x)
                    {
                        int i = x + (y * this.depthWidth);

                        int player = depthPixels[i].PlayerIndex;
                        
                        short depth = depthPixels[i].Depth;
                        byte intensity = (byte)((1.0f * (depth - minDepth) / (maxDepth - minDepth)) * 255f);
                        if (depth < minDepth) intensity = 0;
                        if (depth > maxDepth) intensity = 255;
                                                    
                        if (player > 0)
                        {
                            int j = x + (y * this.colorWidth);

                            if (cbBody.Checked)
                            {
                                this.depthPixels4Channels[i * 4 + 0] = this.colorPixels[j * 4 + 0];
                                this.depthPixels4Channels[i * 4 + 1] = this.colorPixels[j * 4 + 1];
                                this.depthPixels4Channels[i * 4 + 2] = this.colorPixels[j * 4 + 2];
                            }
                            else
                            {
                                this.depthPixels4Channels[i * 4 + 0] = intensity;
                                this.depthPixels4Channels[i * 4 + 1] = 255;
                                this.depthPixels4Channels[i * 4 + 2] = intensity;
                            }
                        }
                        else
                        {
                            if (depth > minDepth && depth < minDepth + 1000)
                            {
                                this.depthPixels4Channels[i * 4 + 0] = 255;
                                this.depthPixels4Channels[i * 4 + 1] = intensity;
                                this.depthPixels4Channels[i * 4 + 2] = intensity;
                            }
                            else
                            {
                                this.depthPixels4Channels[i * 4 + 0] = intensity;
                                this.depthPixels4Channels[i * 4 + 1] = intensity;
                                this.depthPixels4Channels[i * 4 + 2] = intensity;
                            }
                        }
                        this.depthPixels4Channels[i * 4 + 3] = 255;                        
                    }
                }

                Marshal.Copy(depthPixels4Channels, 0, IptrDepImage, depthPixels4Channels.Length);
                depBitmap.UnlockBits(depImageBitmapData);

                //显示深度
                UpdateDepImage(depBitmap);
            }
        }

        private void UpdateColImage(Bitmap bmp)
        {
            try
            {
                colorImage = new Image<Bgr, Byte>(bmp);

                if (skeletonData != null)
                {
                    DrawSkeletons(colorImage, 1);
                }

                if (faceFrame != null)
                {
                    if (faceFrame.TrackSuccessful)
                    {
                        Rectangle rect = new Rectangle(faceFrame.FaceRect.Left,
                            faceFrame.FaceRect.Top,
                            faceFrame.FaceRect.Width,
                            faceFrame.FaceRect.Height);
                        colorImage.Draw(rect, new Bgr(0, 0, 255), 2);
                    }
                }

                imageBoxColor.Image = colorImage;
            }
            catch { }
        }

        private void UpdateDepImage(Bitmap bmp)
        {
            try
            {
                depthImage = new Image<Bgr, Byte>(bmp);

                if(this.skeletonData != null)
                    DrawSkeletons(depthImage, 0);

                imageBoxDepth.Image = depthImage;
            }
            catch { }
        }

        private void imageBoxDepth_Click(object sender, EventArgs e)
        {

        }

        private void cbNearMode_CheckedChanged(object sender, EventArgs e)
        {
            //近模式和默认模式切换
            if (this.sensor != null)
            {
                try
                {
                    if (this.cbNearMode.Checked)
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                        this.sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                        this.sensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void cbSeat_CheckedChanged(object sender, EventArgs e)
        {
            //坐姿和默认模式切换
            if (this.sensor != null)
            {
                try
                {
                    if (this.cbSeat.Checked)
                    {
                        this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    else
                    {
                        this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}
