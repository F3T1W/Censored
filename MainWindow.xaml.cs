using Censored.Helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Censored
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.Timer _timer;
        private Graphics _graphics;

        public MainWindow()
        {
            InitializeComponent();

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Width = screenWidth;
            Height = screenHeight;

            InitializeTimer();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsAPI.SetWindowExTransparent(hwnd);

            _graphics = Graphics.FromHwnd(IntPtr.Zero);
        }

        private void InitializeTimer()
        {
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 30 // 30 milliseconds (approx. 33 fps)
            };
            _timer.Tick += OnTimedEvent;
            _timer.Start();
        }

        private void OnTimedEvent(Object source, EventArgs e)
        {
            CaptureAndApplyMosaic();
        }

        private void CaptureAndApplyMosaic()
        {
            int mosaicSize = 200;
            int centerX = (int)SystemParameters.PrimaryScreenWidth / 2;
            int centerY = (int)SystemParameters.PrimaryScreenHeight / 2;
            int startX = centerX - mosaicSize / 2;
            int startY = centerY - mosaicSize / 2;

            Bitmap originalScreenshot = CaptureScreenArea(startX, startY, mosaicSize, mosaicSize);
            Bitmap mosaicBitmap = ApplyMosaic(originalScreenshot, 10); // Mosaic Cell Size

            _graphics.DrawImage(mosaicBitmap, new Point(startX, startY));
        }

        private Bitmap CaptureScreenArea(int startX, int startY, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(startX, startY, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            return bitmap;
        }

        private Bitmap ApplyMosaic(Bitmap originalBitmap, int cellSize)
        {
            Bitmap mosaicBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(mosaicBitmap))
            {
                for (int y = 0; y < originalBitmap.Height; y += cellSize)
                {
                    for (int x = 0; x < originalBitmap.Width; x += cellSize)
                    {
                        Color avgColor = GetAverageColor(originalBitmap, x, y, cellSize);
                        using (SolidBrush brush = new SolidBrush(avgColor))
                        {
                            graphics.FillRectangle(brush, x, y, cellSize, cellSize);
                        }
                    }
                }
            }
            return mosaicBitmap;
        }

        private Color GetAverageColor(Bitmap bitmap, int startX, int startY, int size)
        {
            int r = 0, g = 0, b = 0, count = 0;
            for (int y = 0; y < size && startY + y < bitmap.Height; y++)
            {
                for (int x = 0; x < size && startX + x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(startX + x, startY + y);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    count++;
                }
            }
            if (count == 0) return Color.Black;
            return Color.FromArgb(r / count, g / count, b / count);
        }
    }
}