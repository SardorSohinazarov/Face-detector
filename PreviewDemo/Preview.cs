using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using OpenCvSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Media.Media3D;

namespace PreviewDemo
{	
	public class Preview : System.Windows.Forms.Form
	{
        private Int32 m_lUserID = -1;
		private bool m_bInitSDK = false;
      
		private Int32 m_lRealHandle = -1;

        private string str;

        CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;

		private System.Windows.Forms.Button btnLogin;
        private System.ComponentModel.Container components = null;
		public Preview()
		{			
			InitializeComponent();
			m_bInitSDK = CHCNetSDK.NET_DVR_Init();
			if (m_bInitSDK == false)
			{
				MessageBox.Show("NET_DVR_Init error!");
				return;
			}
			else
			{               
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
			}

        }
		protected override void Dispose( bool disposing )
		{
			if (m_lRealHandle >= 0)
			{
				CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
			}
			if (m_lUserID >= 0)
			{
				CHCNetSDK.NET_DVR_Logout(m_lUserID);
			}
			if (m_bInitSDK == true)
			{
				CHCNetSDK.NET_DVR_Cleanup();
			}
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#region Windows 窗体设计器生成的代码		
		private void InitializeComponent()
        {
            this.btnLogin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.Location = new System.Drawing.Point(293, 60);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(162, 54);
            this.btnLogin.TabIndex = 1;
            this.btnLogin.Text = "Login";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // Preview
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(759, 198);
            this.Controls.Add(this.btnLogin);
            this.Name = "Preview";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preview";
            this.ResumeLayout(false);

		}
		#endregion		
		[STAThread]
		static void Main() 
		{
			Application.Run(new Preview());
		}		
		private void btnLogin_Click(object sender, System.EventArgs e)
		{
            string DVRIPAddress = "192.168.0.22";
            Int16 DVRPortNumber = 8000;
            string DVRUserName = "admin";
            string DVRPassword = "Admin12345";
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);

            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            //lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;
            lpPreviewInfo.lChannel = 1;
            lpPreviewInfo.dwStreamType = 0;
            lpPreviewInfo.dwLinkMode = 0;
            lpPreviewInfo.bBlocked = true;
            lpPreviewInfo.dwDisplayBufNum = 1;
            lpPreviewInfo.byProtoType = 0;
            lpPreviewInfo.byPreviewMode = 0;

            IntPtr pUser = new IntPtr();
            m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null, pUser);

            #region Hekvision kameraga ulash uchun shu codeni ochish kerak
            /*VideoCapture capture = new VideoCapture($"rtsp://{DVRUserName}:{DVRPassword}@{DVRIPAddress}/Streaming/Channels/1");

            if (!capture.IsOpened())
            {
                MessageBox.Show("Failed to open video stream!");
                return;
            }

            HekvisionniYoqish(capture);
            return;*/
            #endregion

            #region Komputer webcamerasiga ulanish uchun
            WebCameraniYoqish();
            return;
            #endregion


        }

        private void HekvisionniYoqish(VideoCapture capture)
        {
            var cascade = new CascadeClassifier(@"D:\CameraCodi\Cam + Face (v_1) - Copy\1-Preview-PreviewDemo\PreviewDemo\Data\haarcascade_frontalface_alt.xml");
            var nestedCascade = new CascadeClassifier(@"D:\CameraCodi\Cam + Face (v_1) - Copy\1-Preview-PreviewDemo\PreviewDemo\Data\haarcascade_eye.xml");
            var color = Scalar.FromRgb(0, 255, 0);

            using (Window window = new Window("Webcam"))
            using (Mat srcImage = new Mat())
            using (var grayImage = new Mat())
            using (var detectedFaceGrayImage = new Mat())
            {

                while (capture.IsOpened())
                {
                    capture.Read(srcImage);

                    Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGRA2GRAY);
                    Cv2.EqualizeHist(grayImage, grayImage);

                    var faces = cascade.DetectMultiScale(
                        image: grayImage,
                        minSize: new OpenCvSharp.Size(60, 60)
                        );
                    foreach (var faceRect in faces)
                    {
                        using (var detectedFaceImage = new Mat(srcImage, faceRect))
                        {
                            Cv2.Rectangle(srcImage, faceRect, color, 3);

                            Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGRA2GRAY);
                            var nestedObjects = nestedCascade.DetectMultiScale(
                                image: detectedFaceGrayImage,
                                minSize: new OpenCvSharp.Size(30, 30)
                                );

                            foreach (var nestedObject in nestedObjects)
                            {
                                var center = new OpenCvSharp.Point
                                {
                                    X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
                                    Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
                                };
                                var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
                                Cv2.Circle(srcImage, center, (int)radius, color, thickness: 2);
                            }
                        }
                    }

                    window.ShowImage(srcImage);
                    int key = Cv2.WaitKey(1);
                    if (key == 27)
                    {
                        break;
                    }
                }
            }
        }

        private void WebCameraniYoqish()
        {
            var cascade = new CascadeClassifier(@"D:\CameraCodi\Cam + Face (v_1) - Copy\1-Preview-PreviewDemo\PreviewDemo\Data\haarcascade_frontalface_alt.xml");
            var nestedCascade = new CascadeClassifier(@"D:\CameraCodi\Cam + Face (v_1) - Copy\1-Preview-PreviewDemo\PreviewDemo\Data\haarcascade_eye.xml");
            var color = Scalar.FromRgb(0, 255, 0);

            using (Window window = new Window("Webcam"))
            using (VideoCapture capture = new VideoCapture(0))
            using (Mat srcImage = new Mat())
            using (var grayImage = new Mat())
            using (var detectedFaceGrayImage = new Mat())
            {

                while (capture.IsOpened())
                {
                    capture.Read(srcImage);

                    Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGRA2GRAY);
                    Cv2.EqualizeHist(grayImage, grayImage);

                    var faces = cascade.DetectMultiScale(
                        image: grayImage,
                        minSize: new OpenCvSharp.Size(60, 60)
                        );
                    foreach (var faceRect in faces)
                    {
                        using (var detectedFaceImage = new Mat(srcImage, faceRect))
                        {
                            Cv2.Rectangle(srcImage, faceRect, color, 3);

                            Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGRA2GRAY);
                            var nestedObjects = nestedCascade.DetectMultiScale(
                                image: detectedFaceGrayImage,
                                minSize: new OpenCvSharp.Size(30, 30)
                                );

                            foreach (var nestedObject in nestedObjects)
                            {
                                var center = new OpenCvSharp.Point
                                {
                                    X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
                                    Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
                                };
                                var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
                                Cv2.Circle(srcImage, center, (int)radius, color, thickness: 2);
                            }
                        }
                    }

                    window.ShowImage(srcImage);
                    int key = Cv2.WaitKey(1);
                    if (key == 27)
                    {
                        break;
                    }
                }
            }
        }
    }
}
