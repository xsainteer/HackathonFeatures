using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Timer = System.Windows.Forms.Timer;

namespace WinFormsPresentation
{
    public partial class MainForm : Form
    {
        private PictureBox pictureBox;
        private VideoCapture capture;
        private Timer timer;

        public MainForm()
        {
            Width = 800;
            Height = 600;
            Text = "EmguCV Camera";

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(pictureBox);

            capture = new VideoCapture(0, VideoCapture.API.DShow); // 0 — индекс камеры
            capture.Set(CapProp.FrameWidth, 640);
            capture.Set(CapProp.FrameHeight, 480);

            timer = new Timer { Interval = 30 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            using var frame = capture.QueryFrame();
            if (frame != null)
            {
                using var image = frame.ToImage<Bgr, byte>();
                pictureBox.Image?.Dispose();
                pictureBox.Image = image.ToBitmap();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            timer.Stop();
            capture?.Dispose();
        }
    }
}