using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Timer = System.Windows.Forms.Timer;

namespace WinFormsPresentation
{
    public partial class MainForm : Form
    {
        private PictureBox pictureBox;
        private VideoCapture capture;
        private Timer timer;
        private CascadeClassifier faceDetector;

        private HttpClient httpClient;
        private const string AzureFaceApiKey = "YOUR_AZURE_FACE_API_KEY";
        private const string AzureFaceApiEndpoint = "https://YOUR_REGION.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true";

        private bool _faceAlreadySent = false;
        private DateTime _lastFaceDetectedTime;

        public MainForm()
        {
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

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AzureFaceApiKey);

            timer = new Timer { Interval = 30 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

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
                _lastFaceDetectedTime = DateTime.Now;

                if (!_faceAlreadySent)
                {
                    var faceRect = faces[0];
                    try
                    {
                        using var faceMat = new Mat(frame, faceRect);
                        byte[] jpegBytes = MatToJpegBytes(faceMat);
                        await SendFaceToAzureApiAsync(jpegBytes);
                        _faceAlreadySent = true;
                    }
                    catch (Exception ex)
                    {
                        // Логировать ошибку если нужно
                    }
                }
            }
            else
            {
                // Если прошло более 1 секунды с момента последнего обнаружения лица — сбрасываем флаг
                if (_faceAlreadySent && (DateTime.Now - _lastFaceDetectedTime).TotalSeconds > 1)
                {
                    _faceAlreadySent = false;
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

        private async Task SendFaceToAzureApiAsync(byte[] imageBytes)
        {
            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await httpClient.PostAsync(AzureFaceApiEndpoint, content);
            var result = await response.Content.ReadAsStringAsync();

            // Здесь можно обработать результат
            Console.WriteLine(result);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            timer.Stop();
            capture?.Dispose();
            faceDetector?.Dispose();
            httpClient?.Dispose();
        }
    }
}
