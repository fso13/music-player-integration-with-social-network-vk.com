using MusicPlayer.domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Un4seen.Bass;
using VKAudioPlayer.domain;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        private static VkUser _user;// текущий юзер
        bool _flagPlaylistVisible = true;//флаг для определения свернутости плейлиста
        private double _sizeHeight;// переменная, хранящая ширину формы до свернутости
        private static readonly VkHelper VkApi = new VkHelper();//для работы с вконтактом
        private readonly List<PlayList> _playLists = new List<PlayList>();
        int _stream;//для играния
 
        public MainWindow()
        {
            InitializeComponent();
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

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            VkApi.AccessToken = "501e669057305704920915316838e10b8c30adff0f7f037d135b0a9a92d4b3c20d2a5ed3dfd0bc1414d20";
            //if (File.Exists("token.txt"))
            //{
            //    VkApi.AccessToken = File.ReadAllText("token.txt");
            //}
            //else
            //{
            //    File.Create("token.txt");
            //    VkApi.AccessToken = "";
            //}
            //if (VkApi.AccessToken == "")
            //{
            //    var form = new LoginForm();
            //    form.ShowDialog();
            //    VkApi.AccessToken = form.AccessToken;
            //}
            //if (!Directory.Exists("image"))
            //    Directory.CreateDirectory("image");
            _user = VkApi.GetUser();

            var list = VkApi.GetAudio(_user.Uid, "", "0");

            if (list != null)
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    var pl = new PlayList("My music");
                    foreach (var vkaudio in list)
                    {
                        pl.ListAudio.Add(new Audio(vkaudio));
                        PlayListBox.Items.Add(new Audio(vkaudio));
                    }
                    _playLists.Add(pl);
                }));
            }
        }

        private void PlayListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Play((ListBox)sender);
        }

        public void Play(ListBox sender)
        {
            Bass.BASS_StreamFree(_stream);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            _stream = Bass.BASS_StreamCreateURL(((Audio)sender.SelectedItem).Path, 0,
                BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
            if (!Bass.BASS_ChannelPlay(_stream, false)) return;
            Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, ((float)SliderVolum.Value) / 100);
        }

        private void BNewPL_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            var item1 = new TabItem
            {
                Style = (Style) FindResource("TabItemStyle1"),
                Foreground = (Brush) bc.ConvertFrom("#FFFFFFFF"),
                Header = "Tab1",
                FontFamily = new FontFamily("Segoe UI Light"),
                FontSize = 16
            };

            var list = new ListBox
            {
                Name = "PlayListBox",
                Style = (Style) FindResource("ListBoxStyle2"),
                Background = (Brush) bc.ConvertFrom("#FF000000"),
                BorderBrush = (Brush) bc.ConvertFrom("#FF000000"),
                ItemContainerStyle = (Style) FindResource("ListBoxItemStyle1"),
                Foreground = (Brush) bc.ConvertFrom("#FF5D6655")
            };

            list.MouseDoubleClick+= PlayListBox_MouseDoubleClick;
            item1.Content = list;
            PlayListTabs.Items.Add(item1);
            PlayListTabs.SelectedItem = item1;

            var list2 = VkApi.GetAudio(_user.Uid, "", "0");

            if (list2 != null)
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    var pl = new PlayList("My music");
                    foreach (var vkaudio in list2)
                    {
                        
                        pl.ListAudio.Add(new Audio(vkaudio));
                       ((ListBox) (item1).Content).Items.Add(new Audio(vkaudio));
                    }
                    _playLists.Add(pl);
                }));
            }
        }

       

    }
}
