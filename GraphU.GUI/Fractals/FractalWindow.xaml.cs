using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphU.Movement;
using Microsoft.Win32;

namespace GraphU.Fractals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FractalWindow : Window
    {
        private static bool IsFirstTimeOpened = true;
        private double Zoom = 1;
        private Complex Start { get; set; } = new(-2, 2);
        private Complex End { get; set; } = new(2, -2);
        
        private int ImageWidth { get; }
        private int ImageHeight { get; }

        public Border? CurrentLessonPopup { get; set; }
        
        private List<Color> CurrentPalette { get; set; } = null!;
        
        public FractalWindow()
        {
            InitializeComponent();
            
            ImageHeight = (int)FractalImage.Height;
            ImageWidth = (int)FractalImage.Width;

            if (IsFirstTimeOpened)
            {
                IsFirstTimeOpened = false;
                ShowNextLessonPopup(this, null);
            }
        }

        private WriteableBitmap DrawMandelbrot(List<Color> palette)
        {
            var writeableBitmap = new WriteableBitmap(
                ImageWidth, 
                ImageHeight, 
                96, 
                96, 
                PixelFormats.Bgra32, 
                null);

            ConcurrentBag<(int x, int y, int colorIndex)> pixels = new ();
            
            Parallel.For(0, ImageWidth, x =>
            {
                Parallel.For(0, ImageHeight, y =>
                {
                    int index = CorrectSteps(CoordsToComplex(x, y));
                    pixels.Add((x, y, index));
                });
            });
            
            writeableBitmap.Lock();
            foreach (var (x, y, index) in pixels)
            {
                writeableBitmap.DrawPixel(x, y, palette[index].ToByteArray());
            }
            writeableBitmap.Unlock();

            return writeableBitmap;
        }

        private Complex CoordsToComplex(double x, double y)
        {
            var dRe = (End - Start).Real / ImageWidth;
            var dIm = (End - Start).Imaginary / ImageHeight;

            return Start + new Complex(dRe * x, dIm * y);
        }
        
        private static int CorrectSteps(Complex c)
        {
            const int MAX_ITERATIONS = 255;
            const int MAX_MAGNITUDE = 2;

            var currentC = c;

            var i = 0;
            for(; i < MAX_ITERATIONS; i++)
            {
                if (currentC.Magnitude >= MAX_MAGNITUDE) return i;
                currentC = currentC * currentC * currentC + c;
            }

            return i;
        }
        
        private void ZoomImage(object sender, MouseWheelEventArgs e)
        {
            if (FractalType == FractalTypeEnum.Pythagoras) return;
            
            var pos = e.GetPosition(FractalImage);
            var scale = Math.Pow(0.9, Math.Sign(e.Delta));

            var before = CoordsToComplex(pos.X, pos.Y);
            
            Start *= scale;
            End *= scale;

            var after = CoordsToComplex(pos.X, pos.Y);

            Start -= after - before;
            End -= after - before;
            
            FractalImage.Source = DrawMandelbrot(CurrentPalette);
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                button.Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }

        private List<Color>? GeneratePalette(int pow)
        {
            var from = (FirstColorPicker.Background as SolidColorBrush)?.Color;
            var to = (SecondColorPicker.Background as SolidColorBrush)?.Color;

            if (!from.HasValue || !to.HasValue)
                return null;

            if (from.Value == to.Value)
            {
                MessageBox.Show(this, "Ти вибрав однакові кольори! Все зображення буде однакового кольору", "Увага",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            
            return GeneratePaletteImpl(from.Value, to.Value, pow);
        }

        private static List<Color> GeneratePaletteImpl(Color from, Color to, int remaining)
        {
            if (remaining is 0)
            {
                return new List<Color>()
                {
                    Color.FromRgb((byte)((from.R + to.R) / 2), (byte)((from.G + to.G) / 2), (byte)((from.B + to.B) / 2))
                };
            }
            
            var minR = Math.Min(from.R, to.R);
            var maxR = Math.Max(from.R, to.R);
            
            var minG = Math.Min(from.G, to.G);
            var maxG = Math.Max(from.G, to.G);

            var minB = Math.Min(from.B, to.B);
            var maxB = Math.Max(from.B, to.B);

            var distR = maxR - minR;
            var distG = maxG - minG;
            var distB = maxB - minB;

            List<Color> leftPart = new List<Color>();
            List<Color> rightPart = new List<Color>();
            
            if (distR >= distG && distR >= distB)
            {
                byte midR = (byte)((maxR + minR) / 2);

                leftPart = GeneratePaletteImpl(
                    Color.FromRgb(minR, minG, minB),
                    Color.FromRgb(midR, maxG, maxB),
                    remaining - 1
                    );

                rightPart = GeneratePaletteImpl(
                    Color.FromRgb(midR, minG, minB),
                    Color.FromRgb(maxR, maxG, maxB),
                    remaining - 1
                );
            }
            
            if (distG >= distR && distG >= distB)
            {
                byte midG = (byte)((maxG + minG) / 2);

                leftPart = GeneratePaletteImpl(
                    Color.FromRgb(minR, minG, minB),
                    Color.FromRgb(maxR, midG, maxB),
                    remaining - 1
                );

                rightPart = GeneratePaletteImpl(
                    Color.FromRgb(minR, midG, minB),
                    Color.FromRgb(maxR, maxG, maxB),
                    remaining - 1
                );
            }
            
            if (distB >= distR && distB >= distG)
            {
                byte midB = (byte)((maxB + minB) / 2);

                leftPart = GeneratePaletteImpl(
                    Color.FromRgb(minR, minG, minB),
                    Color.FromRgb(maxR, maxG, midB),
                    remaining - 1
                );

                rightPart = GeneratePaletteImpl(
                    Color.FromRgb(minR, minG, midB),
                    Color.FromRgb(maxR, maxG, maxB),
                    remaining - 1
                );
            }

            return leftPart.Concat(rightPart).ToList();
        }

        private void RenderFractal(object sender, RoutedEventArgs e)
        {
            var palette = GeneratePalette(FractalType == FractalTypeEnum.Mandelbrot ? 8 : 4);
            
            if (palette is null)
                return;
            
            CurrentPalette = palette;
            
            if (FractalType == FractalTypeEnum.Mandelbrot)
            {
                Start  = new(-2, 2);
                End = new(2, -2);
   
                FractalImage.Source = DrawMandelbrot(palette);
            }
            else
            {
                FractalImage.Source = DrawPythagoras(palette);
            }
        }

        private WriteableBitmap DrawPythagoras(List<Color> palette)
        {
            var writeableBitmap = new WriteableBitmap(
                ImageWidth, 
                ImageHeight, 
                96, 
                96, 
                PixelFormats.Bgra32, 
                null);

            Rect test = new Rect { Color = palette[15], Height = 80, Width = 80, RotationDegree = 0, RotationCenterX = 250, RotationCenterY = 400 };

            var rects = DrawPythagorasImpl(test, palette);
            var lines = rects.SelectMany(rect => rect.AsLines()).ToList();
            
            foreach (var line in lines)
            {
                writeableBitmap.DrawLine(line);
            }
                
            return writeableBitmap;
        }
        
        private List<Rect> DrawPythagorasImpl(Rect baseRect, List<Color> palette, int remain = 9)
        {
            var result = new List<Rect> { baseRect };
            if (remain <= 0)
            {
                return result;
            }

            var topLine = baseRect.AsLines()[2];
            var topLeftPoint = baseRect.AsPoints()[2];
            var topRightPoint = baseRect.AsPoints()[3];

            var dx = topLine.X2 - topLine.X1;
            var dy = topLine.Y2 - topLine.Y1;

            var newRectsHeight = (int)(baseRect.Height * Math.Sqrt(2) / 2);
            var newRectsWidth = (int)(baseRect.Width * Math.Sqrt(2) / 2);
            
            var centerX1 = (int)(topLeftPoint.X + dy / 2);
            var centerY1 = (int)(topLeftPoint.Y - dx / 2);
            
            var left = DrawPythagorasImpl(new Rect()
            {
                Color = palette[remain - 1],
                Height = newRectsHeight,
                Width = newRectsWidth,
                RotationDegree = baseRect.RotationDegree - Math.PI / 4,
                RotationCenterX = centerX1,
                RotationCenterY = centerY1
            }, palette,
                remain: remain - 1);

            var right = DrawPythagorasImpl(new Rect()
                {
                    Color = palette[remain - 1],
                    Height = newRectsHeight,
                    Width = newRectsWidth,
                    RotationDegree = baseRect.RotationDegree + Math.PI / 4,
                    RotationCenterX = (int)(topRightPoint.X + dy / 2),
                    RotationCenterY = (int)(topRightPoint.Y - dx / 2)
                }, palette,
                    remain: remain - 1);

            return result.Concat(left).Concat(right).ToList();
        }

        private void ChangeFractalType(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox { SelectedValue: string fractal })
                return;

            FractalType = fractal == "Фрактал Мандельброта" ? FractalTypeEnum.Mandelbrot : FractalTypeEnum.Pythagoras;
        }

        private FractalTypeEnum FractalType { get; set; } = FractalTypeEnum.Mandelbrot;
        
        private enum FractalTypeEnum
        {
            Mandelbrot,
            Pythagoras
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            if (FractalImage.Source is not WriteableBitmap bitmap)
            {
                MessageBox.Show(this, "Немає картинки, нічого зберігати", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            
            SaveFileDialog openFileDialog = new SaveFileDialog() {Filter = "Png file (*.png)|*.png"};
            if(openFileDialog.ShowDialog() == false)
                return;

            var filename = openFileDialog.FileName;

            using var stream = new FileStream(filename, FileMode.Create);
            
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);
        }

        private void ShowNextLessonPopup(object sender, RoutedEventArgs e)
        {
            Border next;
            if (CurrentLessonPopup == Lesson1)
            {
                 next = Lesson2;
            }
            else if (CurrentLessonPopup == Lesson2)
            {
                next = Lesson3;
            }
            else if (CurrentLessonPopup == Lesson3)
            {
                next = Lesson4;
            }
            else if (CurrentLessonPopup == Lesson4)
            {
                next = Lesson5;
            }
            else
            {
                next = Lesson1;
            }

            if (CurrentLessonPopup is not null)
            {
                CurrentLessonPopup.Visibility = Visibility.Collapsed;
                CurrentLessonPopup.IsEnabled = false;
            }

            CurrentLessonPopup = next;
            CurrentLessonPopup.Visibility = Visibility.Visible;
            CurrentLessonPopup.IsEnabled = true;
            LessonPopup.IsOpen = true;
            InfoButton.IsEnabled = false;
        }

        private void CloseLessonPopup(object sender, RoutedEventArgs e)
        {
            if (CurrentLessonPopup == null) return;
            
            CurrentLessonPopup.IsEnabled = false;
            CurrentLessonPopup.Visibility = Visibility.Collapsed;
            CurrentLessonPopup = Lesson1;
            LessonPopup.IsOpen = false;
            InfoButton.IsEnabled = true;
        }

        private void BackToMain(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
        
        private void OpenFractalWindow(object sender, RoutedEventArgs e)
        {
            new FractalWindow().Show();
            this.Close();
        }

        private void OpenColorsWindow(object sender, RoutedEventArgs e)
        {
            new ColorsWindow().Show();
            this.Close();
        }

        private void OpenMovementWindow(object sender, RoutedEventArgs e)
        {
            new MovementWindow().Show();
            this.Close();
        }
    }
}