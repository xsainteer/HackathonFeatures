using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Application.Interfaces;
using Timer = System.Windows.Forms.Timer;

namespace WinFormsPresentation
{
    public partial class MainForm : Form
    {
        private PictureBox pictureBox;
        private VideoCapture capture;
        private Timer timer;
        private CascadeClassifier faceDetector;

        private readonly IFaceRecognizerService _faceRecognizerService;

        public MainForm(IFaceRecognizerService faceRecognizerService)
        {
            _faceRecognizerService = faceRecognizerService;
            Width = 800;
            Height = 600;
            Text = "EmguCV Camera with Face Detection";

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(pictureBox);

            capture = new VideoCapture(0, VideoCapture.API.DShow);
            capture.Set(CapProp.FrameWidth, 640);
            capture.Set(CapProp.FrameHeight, 480);

            faceDetector = new CascadeClassifier("haarcascade_frontalface_default.xml");

            timer = new Timer { Interval = 30 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private DateTime _lastSendTime = DateTime.MinValue;
        private readonly TimeSpan _sendInterval = TimeSpan.FromSeconds(2);
        
        private async void Timer_Tick(object sender, EventArgs e)
        {
            using var frame = capture.QueryFrame();
            if (frame == null)
                return;

            using var image = frame.ToImage<Bgr, byte>();
            var gray = image.Convert<Gray, byte>();
            var faces = faceDetector.DetectMultiScale(gray, 1.1, 4);

            if (faces.Length > 0)
            {
                var now = DateTime.Now;

                if (now - _lastSendTime >= _sendInterval)
                {
                    _lastSendTime = now; // обновляем время отправки перед запуском задачи
                    
                    try
                    {
                        byte[] jpegBytes = MatToJpegBytes(frame);
                        var result = BinaryData.FromBytes(jpegBytes);

                        _ = Task.Run(async () =>
                        {
                            var ids = await _faceRecognizerService.IdentifyFacesAsync(result);
                            Console.WriteLine("something after identifying");
                            // здесь можно обновить UI через Invoke, если нужно
                        });
                    }
                    catch (Exception ex)
                    {
                        // Логировать ошибку если нужно
                    }
                }
            }

            foreach (var face in faces)
            {
                CvInvoke.Rectangle(image, face, new Bgr(Color.Red).MCvScalar, 2);
            }

            pictureBox.Image?.Dispose();
            pictureBox.Image = image.ToBitmap();
        }

        private static byte[] MatToJpegBytes(Mat mat)
        {
            using var ms = new MemoryStream();
            mat.ToBitmap().Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            timer.Stop();
            capture?.Dispose();
            faceDetector?.Dispose();
        }
    }
}
