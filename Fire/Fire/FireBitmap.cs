using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fire
{
    public class FireBitmap : Image
    {
        // Если false - то рендеринг однопоточный, если true - многопоточный
        private const bool IsParallel = false;
        public double FPS
        {
            get { return (double)GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }

        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(double), typeof(FireBitmap), new PropertyMetadata(0.0));


        private const int RenderWidth = 500, RenderHeight = 250;

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

        //Битмап
        private readonly WriteableBitmap _writer;
        private int _backBuffer;
        private int _backBufferStride;
        private readonly int PixelWidth;
        private readonly int PixelHeight;

        //Обработчик события изменения DP, анимация меняет значение, а этот метод запускает перерисовку
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FireBitmap fire) fire.DoFire();
        }

        public FireBitmap() : base()
        {
            _writer = GetBitmap();
            PixelWidth = _writer.PixelWidth;
            PixelHeight = _writer.PixelHeight;

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
            //Создаем палитру
            BitmapPalette bitMapPalette = new BitmapPalette(GetColors(_colors));
            //Создаем растровую картинку (bitmap)
            WriteableBitmap writer = new WriteableBitmap(RenderWidth, RenderHeight, 96.0, 96.0, PixelFormats.Indexed8, bitMapPalette);
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
                        SetPixel(x, y, initColorIndex);
                    }
                }
                writer.AddDirtyRect(new Int32Rect(startX, startY, 4, 4));
                writer.Unlock();
            }
        }

        //Возвращает список цветов палитры 
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

        //Возвращает значение цвета пикселя
        private unsafe byte GetPixel(int x, int y)
        {
            int pixelPointer = _backBuffer + y * _backBufferStride + x;
            return *(byte*)pixelPointer;
        }

        //Задает значение цвета пикселя
        private unsafe void SetPixel(int x, int y, byte color)
        {
            int pixelPointer = (int)_backBuffer + y * _backBufferStride + x;
            *(byte*)pixelPointer = color;
        }

        //Инициализирует картинку (рисует полосу)
        private void InitFireBase(WriteableBitmap writer)
        {
            writer.Lock();

            _backBuffer = (int)_writer.BackBuffer;
            _backBufferStride = (int)_writer.BackBufferStride;

            byte initColorIndex = (byte)(writer.Palette.Colors.Count - 1);
            for (int x = 0; x < PixelWidth; x++)
            {
                SetPixel(x, PixelHeight - 1, initColorIndex);
            }
            writer.AddDirtyRect(new Int32Rect(0, PixelHeight - 1, PixelWidth, 1));
            writer.Unlock();
        }

        //Рендерит новый кадр
        private void DoFire()
        {
            var watch = new Stopwatch();
            watch.Start();

            var writer = _writer;
            writer.Lock();

            _backBuffer = (int)_writer.BackBuffer;
            _backBufferStride = (int)_writer.BackBufferStride;

            //Разбиваем Ось X на 8 частей
            IEnumerable<Range> steps = Range.Create(1, PixelWidth - 1).Split(8);

            Parallel.ForEach(steps, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, range =>
            {
                for (int x = range.Start; x < range.End; x++)
                {
                    for (int y = 0; y < PixelHeight - 1; y++)
                    {
                        SpreadFire(x, y);
                    }
                }
            });
            writer.AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
            writer.Unlock();
            watch.Stop();
            FPS = 1000.0d / watch.ElapsedMilliseconds;


        }

        
        //Так как тип Random не потокобезопасный, для каждого потока создается свой Random
        private static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());
        private byte RndNext(int start, int end)
        {
            return (byte)_random.Value.Next(start, end);
        }

        //Вычисляет значение точки для нового кадра
        private void SpreadFire(int x, int y)
        {
            byte topPixel = GetPixel(x, y + 1);
            byte rand = RndNext(0, 4);
            if (topPixel >= rand)
            {
                int xoffset = RndNext(0, 3) - 1;
                byte newColor = (byte)(topPixel - rand);
                if (newColor < 0) newColor = 0;
                SetPixel(x + xoffset, y, newColor);
            }
        }



        private struct Range
        {
            public static Range Create(int start, int end) => new Range(start, end);
            private Range(int start, int end) { Start = start; End = end; }
            public int Start;
            public int End;
            public IEnumerable<Range> Split(int count) => Split(Start, End, count);
            //Разделяет отрезок на заданное количество отрезков
            private static IEnumerable<Range> Split(int start, int end, int count)
            {
                int step = ((end - start) / count) + 1;
                for (int i = 0; i < count; i++)
                {
                    int begin = start + (i * step);
                    int finish = Math.Min(end, (begin + step) - 1);
                    yield return new Range(begin, finish);
                }
            }
        }

    }
}
