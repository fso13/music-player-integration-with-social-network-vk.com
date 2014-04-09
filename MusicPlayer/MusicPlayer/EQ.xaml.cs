using System.Windows;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для EQ.xaml
    /// </summary>
    public partial class EQ
    {
        public EQ(double[] _EQ)
        {
            InitializeComponent();
            SliderEq.Value = _EQ[0];
            SliderEq_Copy.Value = _EQ[1];
            SliderEq_Copy1.Value = _EQ[2];
            SliderEq_Copy2.Value = _EQ[3];
            SliderEq_Copy3.Value = _EQ[4];
            SliderEq_Copy4.Value = _EQ[5];
            SliderEq_Copy5.Value = _EQ[6];
            SliderEq_Copy6.Value = _EQ[7];
            SliderEq_Copy7.Value = _EQ[8];
            SliderEq_Copy8.Value = _EQ[9];
            SliderEq_Copy9.Value = _EQ[10];
            SliderEq_Copy10.Value = _EQ[11];
            SliderEq_Copy11.Value = _EQ[12];
            SliderEq_Copy12.Value = _EQ[13];
            SliderEq_Copy13.Value = _EQ[14];
            SliderEq_Copy14.Value = _EQ[15];
            SliderEq_Copy15.Value = _EQ[16];
            SliderEq_Copy16.Value = _EQ[17];
        }


        private void SliderEq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(0, SliderEq.Value);

        }

        private void SliderEq_Copy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(1, SliderEq_Copy.Value);

        }

        private void SliderEq_Copy1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(2, SliderEq_Copy1.Value);

        }

        private void SliderEq_Copy2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(3, SliderEq_Copy2.Value);

        }

        private void SliderEq_Copy3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(4, SliderEq_Copy3.Value);

        }

        private void SliderEq_Copy4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(5, SliderEq_Copy4.Value);

        }

        private void SliderEq_Copy5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(6, SliderEq_Copy5.Value);

        }

        private void SliderEq_Copy6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(7, SliderEq_Copy6.Value);

        }

        private void SliderEq_Copy7_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(8, SliderEq_Copy7.Value);

        }

        private void SliderEq_Copy8_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(9, SliderEq_Copy8.Value);

        }

        private void SliderEq_Copy9_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(10, SliderEq_Copy9.Value);

        }

        private void SliderEq_Copy10_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(11, SliderEq_Copy10.Value);

        }

        private void SliderEq_Copy11_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(12, SliderEq_Copy11.Value);

        }

        private void SliderEq_Copy12_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(13, SliderEq_Copy12.Value);

        }

        private void SliderEq_Copy13_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(14, SliderEq_Copy13.Value);

        }
        private void SliderEq_Copy14_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(15, SliderEq_Copy14.Value);

        }
        private void SliderEq_Copy15_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(16, SliderEq_Copy15.Value);

        }

        private void SliderEq_Copy16_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.UpdateEQ(17, SliderEq_Copy16.Value);

        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

      

    }
}
