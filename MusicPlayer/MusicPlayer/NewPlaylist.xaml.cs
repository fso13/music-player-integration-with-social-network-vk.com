using System;
using System.Windows;

namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для NewPlaylist.xaml
    /// </summary>
    public partial class NewPlaylist
    {
        public String NamePlaylist;
        public bool OkOrcancel;

        public NewPlaylist()
        {
            OkOrcancel = false;
            InitializeComponent();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NamePlaylist = NamePl.Text;
            OkOrcancel = true;
            Close();
        }
    }
}
