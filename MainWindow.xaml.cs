using Censored.Helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Censored
{
    public partial class MainWindow : Window
    {
        private Timer _timer;

        public MainWindow()
        {
            InitializeComponent();

            // Установка размеров окна на весь экран
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            this.Width = screenWidth;
            this.Height = screenHeight;

            InitializeTimer();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsAPI.SetWindowExTransparent(hwnd);
        }

        private void InitializeTimer()
        {
            _timer = new Timer(5000); // Timer Interval
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
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

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileNameOriginal = Path.Combine(desktopPath, $"original_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            string fileNameMosaic = Path.Combine(desktopPath, $"mosaic_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            originalScreenshot.Save(fileNameOriginal, ImageFormat.Png);

            Bitmap mosaicBitmap = ApplyMosaic(originalScreenshot, 10); // Mosaic Cell Size
            mosaicBitmap.Save(fileNameMosaic, ImageFormat.Png);

            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            graphics.DrawImage(mosaicBitmap, new Point(startX, startY));
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