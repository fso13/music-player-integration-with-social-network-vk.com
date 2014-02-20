using System.Windows;
using System.Windows.Input;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {

        bool _flagPlaylistVisible = true;//флаг для определения свернутости плейлиста
        private double _sizeHeight;// переменная, хранящая ширину формы до свернутости

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ViziblePlayList_Click(object sender, RoutedEventArgs e)
        {
            _flagPlaylistVisible = !_flagPlaylistVisible;
            if (!_flagPlaylistVisible)
            {
                _sizeHeight = Height;
                Height = 94;
            }
            else
            {
                Height = _sizeHeight;
            }
        }
    }
}
