using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GraphU.Fractals;
using GraphU.Movement;

namespace GraphU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
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
