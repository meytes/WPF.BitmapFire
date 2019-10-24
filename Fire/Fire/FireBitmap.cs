using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fire
{
    public class FireBitmap : Image
    {
        private const int RenderWidth = 250, RenderHeight = 65;

        private static readonly uint[] _colors = new uint[] { 0x000000, 0x070707, 0x1F0707, 0x2F0F07, 0x470F07, 0x571707, 0x671F07, 0x771F07,
                0x8F2707, 0x9F2F07, 0xAF3F07, 0xBF4707, 0xBF4707, 0xC74707, 0xDF4F07, 0xDF4F07,0xC74707, 0xC74707, 0xDF5707, 0xDF5707, 0xD75F07,
                0xD7670F, 0xCF6F0F, 0xCF770F, 0xCF7F0F, 0xCF7F0F, 0xCF8717, 0xC78717, 0xC78F17, 0xC78F17, 0xC7971F, 0xBF9F1F, 0xBFA727,
                0xBFAF2F, 0xB7B737, 0xCFCF6F, 0xDFDF9F, 0xEFEFC7, 0xFFFFFF };

        private static readonly byte[] _smart = new byte[]
        {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 32, 32, 32, 0, 0, 32, 0, 0, 0, 32, 0, 0, 32, 32, 32, 0, 0, 32, 32, 32, 32, 0, 0, 32, 32, 32, 32, 32, 0,
                    0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 0, 0, 32, 0, 0, 0,
                    0, 32, 0, 0, 0, 0, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 0, 0, 32, 0, 0, 0,
                    0, 32, 0, 0, 0, 0, 0, 32, 32, 0, 32, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 0, 0, 32, 0, 0, 0,
                    0, 0, 32, 32, 32, 0, 0, 32, 0, 32, 0, 32, 0, 32, 32, 32, 32, 32, 0, 32, 32, 32, 32, 0, 0, 0, 0, 32, 0, 0, 0,
                    0, 0, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 32, 0, 0, 0, 0, 32, 0, 0, 0,
                    0, 0, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 0, 0, 32, 0, 0, 0,
                    0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 0, 0, 32, 0, 0, 0,
                    0, 0, 32, 32, 32, 0, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 32, 0, 0, 0, 32, 0, 0, 0, 32, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private readonly WriteableBitmap _writer;
        private readonly Random _rnd = new Random();



        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FireBitmap fire) fire.DoFire();
        }

        public FireBitmap() : base()
        {
            _writer = GetBitmap();
            InitFireBase(_writer);
            Source = _writer;
            PreviewMouseMove += FireBitmap_PreviewMouseMove1;
            PreviewMouseDown += FireBitmap_PreviewMouseDown;
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(int), typeof(FireBitmap),
                new FrameworkPropertyMetadata(0, OnPropertyChanged));

        public int Offset
        {
            get { return (int)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        private WriteableBitmap GetBitmap()
        {
            var bitMapPalette = new BitmapPalette(GetColors(_colors));
            var writer = new WriteableBitmap(RenderWidth, RenderHeight, 96.0, 96.0, PixelFormats.Indexed8, bitMapPalette);
            return writer;
        }

        private void FireBitmap_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(this);
                int startX = (int)(_writer.PixelWidth * (position.X / ActualWidth)) - 15;
                int startY = (int)(_writer.PixelHeight * (position.Y / ActualHeight)) - 5;
                DrawSmart(startX, startY);
            }
        }

        private void DrawSmart(int startX, int startY)
        {
            var writer = _writer;

            if (startX > 0 && startX + 32 < writer.PixelWidth
                && startY > 0 && startY + 12 < writer.PixelHeight)
            {
                writer.Lock();
                writer.WritePixels(new Int32Rect(startX, startY, 31, 11), _smart, 31, 0);
                writer.AddDirtyRect(new Int32Rect(startX, startY, 31, 11));
                writer.Unlock();
            }

        }

        private void FireBitmap_PreviewMouseMove1(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(this);
                int startX = (int)(_writer.PixelWidth * (position.X / ActualWidth)) - 2;
                int startY = (int)(_writer.PixelHeight * (position.Y / ActualHeight)) - 2;
                DrawPoint(startX, startY);
            }
        }

        private void DrawPoint(int startX, int startY)
        {
            var writer = _writer;

            if (startX > 0 && startX + 4 < writer.PixelWidth
                 && startY > 0 && startY + 4 < writer.PixelHeight)
            {
                writer.Lock();
                byte initColorIndex = (byte)(writer.Palette.Colors.Count - 1);
                for (int x = startX; x < startX + 2; x++)
                {
                    for (int y = startY; y < startY + 2; y++)
                    {
                        SetPixel(writer, x, y, initColorIndex);
                    }
                }
                writer.AddDirtyRect(new Int32Rect(startX, startY, 4, 4));
                writer.Unlock();
            }
        }

        private static Color[] GetColors(uint[] colors)
        {
            var result = new Color[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                result[i] = Color.FromRgb((byte)(color >> 16 & 0xFF), (byte)(color >> 8 & 0xFF), (byte)(color & 0xFF));
            }
            return result;
        }

        private unsafe byte GetPixel(WriteableBitmap writer, int x, int y)
        {
            int pixelPointer = (int)writer.BackBuffer + y * writer.BackBufferStride + x;
            return *(byte*)pixelPointer;
        }

        private unsafe void SetPixel(WriteableBitmap writer, int x, int y, byte color)
        {
            int pixelPointer = (int)writer.BackBuffer + y * writer.BackBufferStride + x;
            *(byte*)pixelPointer = color;
        }

        private void InitFireBase(WriteableBitmap writer)
        {
            writer.Lock();
            byte initColorIndex = (byte)(writer.Palette.Colors.Count - 1);
            for (int x = 0; x < writer.PixelWidth; x++)
            {
                SetPixel(writer, x, writer.PixelHeight - 1, initColorIndex);
            }
            writer.AddDirtyRect(new Int32Rect(0, writer.PixelHeight - 1, writer.PixelWidth, 1));
            writer.Unlock();
        }

        private void DoFire()
        {
            var writer = _writer;
            writer.Lock();
            for (int x = 1; x < writer.PixelWidth - 1; x++)
            {
                for (int y = 0; y < writer.PixelHeight - 1; y++)
                {
                    SpreadFire(writer, x, y);
                }
            }
            writer.AddDirtyRect(new Int32Rect(0, 0, writer.PixelWidth, writer.PixelHeight));
            writer.Unlock();

        }

        private void SpreadFire(WriteableBitmap writer, int x, int y)
        {
            byte topPixel = GetPixel(writer, x, y + 1);
            byte rand = (byte)_rnd.Next(0, 4);
            if (topPixel >= rand)
            {
                int xoffset =  _rnd.Next(0, 3) - 1;
                byte newColor = (byte)(topPixel - rand);
                SetPixel(writer, x + xoffset, y, newColor);

                if (rand < 2)
                {
                    if (y + 1 < writer.PixelHeight - 1)
                    {
                        var decrColor = (byte)(topPixel - 1);
                        SetPixel(writer, x, y + 1, decrColor);
                    }
                }
            }
        }
    }
}
