using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GraphU.Fractals;
using Microsoft.Win32;

namespace GraphU.Movement;

public partial class MovementWindow : INotifyPropertyChanged
{
    private double _x1;
    private double _x2;
    private double _x3;
    private double _y1;
    private double _y2;
    private double _y3;
    
    private static bool IsFirstTimeOpened = true;

    public double X1
    {
        get => _x1;
        set
        {
            if (value.Equals(_x1)) return;
            _x1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(X4));
        }
    }

    public double X2
    {
        get => _x2;
        set
        {
            if (value.Equals(_x2)) return;
            _x2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(X4));
        }
    }

    public double X3
    {
        get => _x3;
        set
        {
            if (value.Equals(_x3)) return;
            _x3 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(X4));
        }
    }

    public double X4
    {
        get
        {
            if (Math.Abs(X1 - X2) < 0.000001)
                return X3;
            if (Math.Abs(X1 - X3) < 0.000001)
                return X2;
            return X1;
        }
    }

    public double Y1
    {
        get => _y1;
        set
        {
            if (value.Equals(_y1)) return;
            _y1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Y4));
        }
    }

    public double Y2
    {
        get => _y2;
        set
        {
            if (value.Equals(_y2)) return;
            _y2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Y4));
        }
    }

    public double Y3
    {
        get => _y3;
        set
        {
            if (value.Equals(_y3)) return;
            _y3 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Y4));
        }
    }

    public double Y4
    {
        get
        {
            if (Math.Abs(Y1 - Y2) < 0.000001)
                return Y3;
            if (Math.Abs(Y1 - Y3) < 0.000001)
                return Y2;
            return Y1;
        }
    }
    
    public double Speed { get; set; }
    public double MoveHeight { get; set; }
    public double Angle { get; set; }

    private CancellationTokenSource CancellationTokenSource { get; set; }
    
    public MovementWindow()
    {
        InitializeComponent();

        this.DataContext = this;
        this.MainPlot.Configuration.MiddleClickDragZoom = false;
        if (IsFirstTimeOpened)
        {
            IsFirstTimeOpened = false;
            ShowNextLessonPopup(this, null);
        }
    }

    private void StartAnimation(object sender, RoutedEventArgs e)
    {
        Square a;

        try
        {
            a = new Square(new Point(X1, Y1), new Point(X2, Y2), new Point(X3, Y3), new Point(X4, Y4));
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, "Невірні координати квадрату", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (Angle == 0 && MoveHeight == 0)
        {
            MessageBox.Show(this, "Кут повороту і висота підйому дорівнюють нулю! Ти не побачиш жодного руху!", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        if (MoveHeight != 0 && Speed == 0)
        {
            MessageBox.Show(this, "Будь ласка, вкажи швидкість підйому, або постав висоту підйому рівну 0", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (Math.Sign(Speed) != Math.Sign(Height))
        {
            MessageBox.Show(this, "Будь ласка, вкажи швидкість підйому і висоту підйому однакову за знаком!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (X1 == X2 && X2 == X3 && X3 == X4)
        {
            MessageBox.Show(this, "Квадрат має мати різні вершини", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
            
        
        this.CancellationTokenSource?.Cancel();
        this.CancellationTokenSource = new CancellationTokenSource();
        const double FPS = 60;
        var seconds = MoveHeight / Speed;

        var startX = Math.Min(Math.Min(X1, X2), Math.Min(X3, X4)) - 0.5 - MoveHeight/2;
        var startY = Math.Min(Math.Min(Y1, Y2), Math.Min(Y3, Y4)) - 0.5;
        
        var endX = Math.Max(Math.Max(X1, X2), Math.Max(X3, X4)) + 0.5 + MoveHeight/2; 
        var endY = Math.Max(Math.Max(Y1, Y2), Math.Max(Y3, Y4)) + 0.5 + MoveHeight;
        
        this.MainPlot.Plot.SetAxisLimits(startX, endX, startY, endY);
        
        var frames = Speed == 0 ? 1 : Math.Ceiling(seconds * FPS);
        var frameTime = 1 / FPS;

        var dY = MoveHeight / frames;
        var dAngle = Angle * Math.PI / 180 / frames; 
        var cos = Math.Cos(dAngle);
        var sin = Math.Sin(dAngle);
        
        
        var cancellationToken = CancellationTokenSource.Token;

        StopAnimationButton.IsEnabled = true;
        
        Task.Run(() =>
        {
            for (int i = 0; i < frames && !cancellationToken.IsCancellationRequested; i++)
            {
                var limits = this.MainPlot.Plot.GetAxisLimits();
                this.Dispatcher.Invoke(() =>
                {
                    this.MainPlot.Plot.Clear();
                });
                
                /*
                var vector = a.Center.AsVector();
                a.TranslateBy(vector.Inverted());
                a.ApplyMatrix(Matrix22.Rotational(dAngle));
                a.TranslateBy(vector + new Vector2D(0, dY));
                */

                var x = a.Center.X;
                var y = a.Center.Y;
                
                /*
                var transposeToCenterMatrix = new Matrix33(
                    1, 0, -x,
                    0, 1, -y,
                    0, 0, 1);
                
                var transposeFromCenterMatrix = new Matrix33(
                    1, 0, x,
                    0, 1, y,
                    0, 0, 1);

                var transposeMatrix = new Matrix33(
                    1, 0, 0,
                    0, 1, dY,
                    0, 0, 1);
                
                var rotationalMatrix = new Matrix33(
                    cos, -sin, 0,
                    sin, cos, 0,
                    0, 0, 1);
                
                var resultingMatrix = transposeMatrix * transposeFromCenterMatrix * rotationalMatrix * transposeToCenterMatrix; */

                var resultingMatrix = new Matrix33(
                    cos, -sin, -cos*x + y*sin + x,
                    sin, cos, - x*sin - cos*y + y + dY,
                    0, 0, 1
                );
                
                var vectors = a.AsListOfExtendedVectors();
                

                var processedVectors = vectors.Select(vector => resultingMatrix * vector);
                a = Square.FromListOfExtendedVectors(processedVectors);
                
                this.Dispatcher.Invoke(() =>
                {
                    this.MainPlot.Plot.SetAxisLimits(limits);
                    this.MainPlot.Plot.PutSquare(a);
                    this.MainPlot.Refresh();
                });
                Thread.Sleep((int)(frameTime * 1000));
            }
            this.Dispatcher.Invoke(() =>
            {
                this.StopAnimationButton.IsEnabled = false;
            });
        });
    }
    
    private void SaveImage(object sender, RoutedEventArgs e)
    {
        SaveFileDialog openFileDialog = new SaveFileDialog() {Filter = "Png file (*.png)|*.png"};
        if(openFileDialog.ShowDialog() == false)
            return;

        var filename = openFileDialog.FileName;

        this.MainPlot.Plot.SaveFig(filename);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    private void StopAnimation(object sender, RoutedEventArgs e)
    {
        this.CancellationTokenSource?.Cancel();
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
}