using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для EQ.xaml
    /// </summary>
    public partial class Eq
    {

        public void UpdateEq()
        {
            if (gridEQ == null) return;

            gridEQ.Children.Clear();
            var x = new double[18];
            for (var i = 0; i < 18; i++)
            {
                x[i] = i*16;
            }
            double[] y =
            {
                -1.5*SliderEq.Value, -1.5*SliderEq_Copy.Value, -1.5*SliderEq_Copy1.Value,
                -1.5*SliderEq_Copy2.Value, -1.5*SliderEq_Copy3.Value, -1.5*SliderEq_Copy4.Value,
                -1.5*SliderEq_Copy5.Value,-1.5*SliderEq_Copy6.Value, -1.5*SliderEq_Copy7.Value,
                -1.5*SliderEq_Copy8.Value, -1.5*SliderEq_Copy9.Value, -1.5*SliderEq_Copy10.Value,
                -1.5*SliderEq_Copy11.Value, -1.5*SliderEq_Copy12.Value, -1.5*SliderEq_Copy13.Value,
                -1.5*SliderEq_Copy14.Value, -1.5*SliderEq_Copy15.Value, -1.5*SliderEq_Copy16.Value
            };
            alglib.spline1dinterpolant c;
            alglib.spline1dbuildakima(x, y, out c);

            for (var i = 0; i < 280; i++)
            {
                var p = new Point(i, alglib.spline1dcalc(c, i));
                var ellipse = new Ellipse
                {
                    Width = 1,
                    Height = 1,
                    StrokeThickness = 2,
                    Stroke = Brushes.OrangeRed,
                    Margin = new Thickness(i - 140, p.Y, 0, 0)
                };

                gridEQ.Children.Add(ellipse);
                //new Point((double)(i + 1) / (double)Np, 1.0 - (Data1[i + 1] / 2.0)));
            }
        }

        public Eq(IList<double> eq)
        {
            InitializeComponent();
            SliderEq.Value = eq[0];
            SliderEq_Copy.Value = eq[1];
            SliderEq_Copy1.Value = eq[2];
            SliderEq_Copy2.Value = eq[3];
            SliderEq_Copy3.Value = eq[4];
            SliderEq_Copy4.Value = eq[5];
            SliderEq_Copy5.Value = eq[6];
            SliderEq_Copy6.Value = eq[7];
            SliderEq_Copy7.Value = eq[8];
            SliderEq_Copy8.Value = eq[9];
            SliderEq_Copy9.Value = eq[10];
            SliderEq_Copy10.Value = eq[11];
            SliderEq_Copy11.Value = eq[12];
            SliderEq_Copy12.Value = eq[13];
            SliderEq_Copy13.Value = eq[14];
            SliderEq_Copy14.Value = eq[15];
            SliderEq_Copy15.Value = eq[16];
            SliderEq_Copy16.Value = eq[17];
            UpdateEq();
        }

        public void SetEq(double[] eq)
        {
            SliderEq.Value = eq[0];
            SliderEq_Copy.Value = eq[1];
            SliderEq_Copy1.Value = eq[2];
            SliderEq_Copy2.Value = eq[3];
            SliderEq_Copy3.Value = eq[4];
            SliderEq_Copy4.Value = eq[5];
            SliderEq_Copy5.Value = eq[6];
            SliderEq_Copy6.Value = eq[7];
            SliderEq_Copy7.Value = eq[8];
            SliderEq_Copy8.Value = eq[9];
            SliderEq_Copy9.Value = eq[10];
            SliderEq_Copy10.Value = eq[11];
            SliderEq_Copy11.Value = eq[12];
            SliderEq_Copy12.Value = eq[13];
            SliderEq_Copy13.Value = eq[14];
            SliderEq_Copy14.Value = eq[15];
            SliderEq_Copy15.Value = eq[16];
            SliderEq_Copy16.Value = eq[17];
            UpdateEq();

        }

        private void SliderEq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(0, SliderEq.Value);
            UpdateEq();
        }

        private void SliderEq_Copy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(1, SliderEq_Copy.Value);
            UpdateEq();

        }

        private void SliderEq_Copy1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(2, SliderEq_Copy1.Value);
            UpdateEq();

        }

        private void SliderEq_Copy2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(3, SliderEq_Copy2.Value);
            UpdateEq();

        }

        private void SliderEq_Copy3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(4, SliderEq_Copy3.Value);
            UpdateEq();

        }

        private void SliderEq_Copy4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(5, SliderEq_Copy4.Value);
            UpdateEq();

        }

        private void SliderEq_Copy5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(6, SliderEq_Copy5.Value);
            UpdateEq();

        }

        private void SliderEq_Copy6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(7, SliderEq_Copy6.Value);
            UpdateEq();

        }

        private void SliderEq_Copy7_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(8, SliderEq_Copy7.Value);
            UpdateEq();

        }

        private void SliderEq_Copy8_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(9, SliderEq_Copy8.Value);
            UpdateEq();

        }

        private void SliderEq_Copy9_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(10, SliderEq_Copy9.Value);
            UpdateEq();

        }

        private void SliderEq_Copy10_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(11, SliderEq_Copy10.Value);
            UpdateEq();

        }

        private void SliderEq_Copy11_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(12, SliderEq_Copy11.Value);
            UpdateEq();

        }

        private void SliderEq_Copy12_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(13, SliderEq_Copy12.Value);
            UpdateEq();

        }

        private void SliderEq_Copy13_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(14, SliderEq_Copy13.Value);
            UpdateEq();

        }

        private void SliderEq_Copy14_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(15, SliderEq_Copy14.Value);
            UpdateEq();

        }

        private void SliderEq_Copy15_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(16, SliderEq_Copy15.Value);
            UpdateEq();

        }

        private void SliderEq_Copy16_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEq(17, SliderEq_Copy16.Value);
            UpdateEq();

        }

        private void EQForm_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
