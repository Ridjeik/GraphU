using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphU.Fractals;
using GraphU.Movement;

namespace GraphU
{
    /// <summary>
    /// Interaction logic for ColorsWindow.xaml
    /// </summary>
    public partial class ColorsWindow : Window
    {
        private static bool IsFirstTimeOpened = true;
        private Bitmap LoadedBitmap { get; set; }
        private Bitmap CurrentBitmap { get; set; }

        public ColorsWindow()
        {
            InitializeComponent();
            if (IsFirstTimeOpened)
            {
                IsFirstTimeOpened = false;
                ShowNextLessonPopup(this, null);
            }
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images|*.jpg;*.png;";
            if (!openFileDialog.ShowDialog() ?? false)
            {
                return;
            }

            var filename = openFileDialog.FileName;
            Bitmap bitmap = new(filename);

            var x = Math.Max(bitmap.Width / 2 - 250, 0);
            var y = Math.Max(bitmap.Height / 2 - 250, 0);

            var cloningArea = new System.Drawing.Rectangle(x, y, Math.Min(bitmap.Width, 500), Math.Min(bitmap.Height, 500));

            CurrentBitmap = bitmap.Clone(cloningArea, System.Drawing.Imaging.PixelFormat.DontCare);
            LoadedBitmap = bitmap.Clone(cloningArea, System.Drawing.Imaging.PixelFormat.DontCare);

            ColorImage.Source = Convert(CurrentBitmap);
            ColorsToBeChanged = Enumerable.Range(0, LoadedBitmap.Width)
                .SelectMany(x => Enumerable.Range(0, LoadedBitmap.Height).Select(y => (x, y))
                .Where(coords => RgbToHsl(LoadedBitmap.GetPixel(coords.x, coords.y)).Saturation <= 0.05)).ToList();
        }

        private static BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {

        }

        private void DisplayHoveredPixel(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this.ColorImage);
            var x = (int)(pos.X * CurrentBitmap.Width / 500);
            var y = (int)(pos.Y * CurrentBitmap.Height / 500);

            x = Math.Min(x, CurrentBitmap.Width - 1);
            y = Math.Min(y, CurrentBitmap.Height - 1);

            var colorRGB = CurrentBitmap.GetPixel(x, y);
            var colorHSL = RgbToHsl(colorRGB);
            var colorCMYK = RgbToCmyk(colorRGB);

            this.H.Text = $"{(int)colorHSL.Hue}°";
            this.S.Text = $"{(int)(colorHSL.Saturation * 100)}%";
            this.L.Text = $"{(int)(colorHSL.Lightness * 100)}%";

            this.C.Text = $"{(int)(colorCMYK.Cyan * 100)}%";
            this.M.Text = $"{(int)(colorCMYK.Magenta * 100)}%";
            this.Y.Text = $"{(int)(colorCMYK.Yellow * 100)}%";
            this.K.Text = $"{(int)(colorCMYK.Black * 100)}%";
        }

        private ColorHSL RgbToHsl(System.Drawing.Color color)
        {
            double R1 = color.R / 255.0;
            double G1 = color.G / 255.0;
            double B1 = color.B / 255.0;

            double Cmax = Math.Max(R1, Math.Max(G1, B1));
            double Cmin = Math.Min(R1, Math.Min(G1, B1));

            double delta = Cmax - Cmin;

            double Hue = 0;
            if (delta == 0)
            {
                Hue = 0;
            }
            else if (Cmax == R1)
            {
                Hue = 60 * ((G1 - B1) / delta % 6);
            }
            else if (Cmax == G1)
            {
                Hue = 60 * (((B1 - R1) / delta + 2) % 6);
            }
            else if (Cmax == B1)
            {
                Hue = 60 * (((R1 - G1) / delta + 4) % 6);
            }

            Hue += 30;

            if(Hue < 0)
            {
                Hue = 360 + Hue;
            }
            else if(Hue > 360)
            {
                Hue = Hue % 360;
            }

            double Lightness = (Cmax + Cmin) / 2;
            double Saturation = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * Lightness - 1));

            return new ColorHSL { Hue = Hue, Lightness = Lightness, Saturation = Saturation };
        }

        private ColorCMYK RgbToCmyk(System.Drawing.Color color)
        {
            double R1 = color.R / 255.0;
            double G1 = color.G / 255.0;
            double B1 = color.B / 255.0;

            double K = 1 - Math.Max(R1, Math.Max(G1, B1));
            double C = K == 1 ? 0 : (1 - R1 - K) / (1 - K);
            double M = K == 1 ? 0 : (1 - G1 - K) / (1 - K);
            double Y = K == 1 ? 0 : (1 - B1 - K) / (1 - K);

            return new ColorCMYK { Cyan = C, Magenta = M, Yellow = Y, Black = K };
        }

        private System.Drawing.Color CmykToRgb(ColorCMYK color)
        {
            var result = System.Drawing.Color.FromArgb(
                (int)(255 * (1 - color.Cyan) * (1 - color.Black)),
                (int)(255 * (1 - color.Magenta) * (1 - color.Black)),
                (int)(255 * (1 - color.Yellow) * (1 - color.Black)));

            return result;
        }

        private System.Drawing.Color HslToRgb(ColorHSL color)
        {
            var C = (1 - Math.Abs(2 * color.Lightness - 1)) * color.Saturation;
            var X = C * (1 - Math.Abs(color.Hue / 60 % 2 - 1));
            var m = color.Lightness - C / 2;

            var H = color.Hue;

            (double, double, double) colors = H switch
            {
                >= 0 and < 60 => (C, X, 0),
                < 120 => (X, C, 0),
                < 180 => (0, C, X),
                < 240 => (0, X, C),
                < 300 => (X, 0, C),
                _ => (C, 0, X)
            };

            (double R1, double G1, double B1) = colors;
            (int R, int G, int B) = ((int)((R1 + m) * 255), (int)((G1 + m) * 255), (int)((B1 + m) * 255));
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        private IEnumerable<(int, int)> ColorsToBeChanged { get; set; }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LoadedBitmap == null) return;

            foreach ((int x, int y) in ColorsToBeChanged)
            {
                var oldRgb = LoadedBitmap.GetPixel(x, y);
                var hsl = RgbToHsl(oldRgb);
                hsl.Lightness = Math.Min(1, LightnessSlider.Value * hsl.Lightness);
                var rgb = HslToRgb(hsl);
                CurrentBitmap.SetPixel(x, y, rgb);
            }

            ColorImage.Source = Convert(CurrentBitmap);
        }

        private void BitmapToHsl(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < CurrentBitmap.Width * CurrentBitmap.Height; i++)
            {
                var x = i % CurrentBitmap.Width;
                var y = i / CurrentBitmap.Height;

                var pixel = CurrentBitmap.GetPixel(x, y);
                var hsl = RgbToHsl(pixel);
                CurrentBitmap.SetPixel(x, y, HslToRgb(hsl));
            };

            ColorImage.Source = Convert(CurrentBitmap);
        }

        private void BitmapToCmyk(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < CurrentBitmap.Width * CurrentBitmap.Height; i++)
            {
                var x = i % CurrentBitmap.Width;
                var y = i / CurrentBitmap.Height;

                var pixel = CurrentBitmap.GetPixel(x, y);
                var hsl = RgbToCmyk(pixel);
                CurrentBitmap.SetPixel(x, y, CmykToRgb(hsl));
            };

            ColorImage.Source = Convert(CurrentBitmap);
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

        public Border? CurrentLessonPopup { get; set; }

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
