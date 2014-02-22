using System.Globalization;
using MusicPlayer.domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MusicPlayer.domain.vk;
using Un4seen.Bass;
using VKAudioPlayer.domain;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        public System.Timers.Timer TimerPosition = new System.Timers.Timer();
        public System.Timers.Timer Timer2 = new System.Timers.Timer();
        
        public double Time1;
        public double Time2;

        private static VkUser _user;// текущий юзер
        bool _flagPlaylistVisible = true;//флаг для определения свернутости плейлиста
        private int _flagPossition;
        private double _sizeHeight;// переменная, хранящая ширину формы до свернутости
        private static readonly VkHelper VkApi = new VkHelper();//для работы с вконтактом
        private readonly List<PlayList> _playLists = new List<PlayList>();
        private static List<VkAlbom> _alboms = new List<VkAlbom>();

        public ListBox CurrentListBox;

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
            var list = VkApi.GetAudio(_user.Uid, "");
            
            AddAudioPlayList(PlayListBox, list);
            GetAlbomsUser();
            PlayListTabs.SelectedIndex = 0;
            var t = (ScrollViewer)PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
            t.ScrollToHorizontalOffset(0);
            TimerPosition.Elapsed += TimerPosition1_Tick;
            TimerPosition.Interval = 1000;

        }

        private void TimerPosition1_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                Time1 = Bass.BASS_ChannelBytes2Seconds(_stream, Bass.BASS_ChannelGetPosition(_stream));
                Time2 = Bass.BASS_ChannelBytes2Seconds(_stream, Bass.BASS_ChannelGetLength(_stream));

                var t1 = Convert.ToString((int)Time1 / 60);
                var t2 = Convert.ToString((int)Time1 % 60);
                if (t1.Length == 1) t1 = "0" + t1;
                if (t2.Length == 1) t2 = "0" + t2;
                TextTime.Text = t1 + " : " + t2;

                _flagPossition = 1;
                SliderTrack.Value = Time1;
                MyTaskItem.ProgressValue = (Time1) / Time2;
                if (!Time1.Equals(Time2)) return;
                CurrentListBox.SelectedIndex += 1;
                Play(CurrentListBox);
            }));
        }
        public void GetAlbomsUser()
        {
            _alboms = VkApi.GetAlbom();
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                foreach (var albom in _alboms)
                {
                    var list = VkApi.GetAudio(_user.Uid, albom.AlbumId);
                    NewPlaylist(albom.Title,list);
                }
            }));

        }

        public void NewPlaylist(string name, List<VkAudio> list2 )
        {
            var bc = new BrushConverter();

            var item1 = new TabItem
            {
                Style = (Style)FindResource("TabItemStyle1"),
                Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF"),
                Header = name,
                FontFamily = new FontFamily("Segoe UI Light"),
                FontSize = 16
            };

            var list = new ListBox
            {
                Name = "PlayListBox",
                Style = (Style)FindResource("ListBoxStyle2"),
                Background = (Brush)bc.ConvertFrom("#FF000000"),
                BorderBrush = (Brush)bc.ConvertFrom("#FF000000"),
                ItemContainerStyle = (Style)FindResource("ListBoxItemStyle1"),
                Foreground = (Brush)bc.ConvertFrom("#FF5D6655")
            };

            list.MouseDoubleClick += PlayListBox_MouseDoubleClick;
            item1.Content = list;
            PlayListTabs.Items.Add(item1);
            PlayListTabs.SelectedItem = item1;
            AddAudioPlayList((ListBox)(item1).Content,list2);
        }

        private void PlayListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CurrentListBox = (ListBox)sender;
            Play((ListBox)sender);
            TimerPosition.Start();
        }

        public void Play(ListBox sender)
        {
            Bass.BASS_StreamFree(_stream);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            _stream = Bass.BASS_StreamCreateURL(((Audio)sender.SelectedItem).Path, 0,
                BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
            if (!Bass.BASS_ChannelPlay(_stream, false)) return;
            Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, ((float)SliderVolum.Value) / 100);
            SliderTrack.Maximum = Bass.BASS_ChannelBytes2Seconds(_stream, Bass.BASS_ChannelGetLength(_stream));
        }

        private void BNewPL_Click(object sender, RoutedEventArgs e)
        {
           NewPlaylist("New playlist",null);
           var t = (ScrollViewer)PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
           t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex * 110);
        }

        private void BPrevPl_Click(object sender, RoutedEventArgs e)
        {
            var t = (ScrollViewer)PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
            PlayListTabs.FindName("LeftClickButton");
            if (PlayListTabs.SelectedIndex - 1 < 0)
            {
                PlayListTabs.SelectedIndex = PlayListTabs.Items.Count - 1;
            }
            else
            {
                PlayListTabs.SelectedIndex = PlayListTabs.SelectedIndex - 1;
            }
            t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex * 110);
            
        }

        private void BNextPl_Click(object sender, RoutedEventArgs e)
        {
            var t = (ScrollViewer)PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
            
            if (PlayListTabs.SelectedIndex + 1 > PlayListTabs.Items.Count-1)
            {
                PlayListTabs.SelectedIndex = 0;
            }
            else
            {
                PlayListTabs.SelectedIndex = PlayListTabs.SelectedIndex + 1;
            }
            t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex * 110);

        }

        private void RightClickButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void LeftClickButton_Click(object sender, RoutedEventArgs e)
        {
            //LeftClickButton
        }

        private void SliderTrack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(Math.Abs(e.NewValue-e.OldValue)<=1.1))
            {
                //TimerPosition.Stop();
                Bass.BASS_ChannelSetPosition(_stream, SliderTrack.Value);
                //TimerPosition.Start();
            }
            
        }


        private void thumbNext_Click(object sender, EventArgs e)
        {
            //_nextPrev = true;
            //if (PlayList.Items.Count <= 0) return;
            //_oldNumber = Number;
            //Number++;
            //if (Number == PlayList.Items.Count) Number = 0;
            //Play();
            //_flagDowloadImage = false;
        }

        private void thumbPrevious_Click(object sender, EventArgs e)
        {
            //_nextPrev = false;
            //if (PlayList.Items.Count <= 0) return;
            //_oldNumber = Number;
            //Number--;
            //if (Number < 0) Number = PlayList.Items.Count - 1;
            //Play();
            //_flagDowloadImage = false;
        }

        private void thumbPlay_Click(object sender, EventArgs e)
        {
            //if (FlagStart == 0)
            //{
            //    if (PlayList.Items.Count <= 0) return;
            //    Bass.BASS_ChannelPlay(Stream, false);
            //    FlagStart = 1;
            //    Timer1.Start();
            //    BPlay.Background = new ImageBrush(GetThumbnailSimple("scin//b_play.png"));
            //}
            //else
            //{
            //    FlagStart = 0;
            //    Bass.BASS_ChannelStop(Stream);
            //    Timer1.Stop();
            //    BPlay.Background = new ImageBrush(GetThumbnailSimple("scin//b_play2.png"));
            //}
        }

        private void SliderVolum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(_stream, BASSAttribute.BASS_ATTRIB_VOL, ((float)SliderVolum.Value) / 100);

        }

        public void AddAudioPlayList(ListBox listbox, List<VkAudio> list)
        {
            if (list != null)
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    foreach (var vkaudio in list)
                    {
                        listbox.Items.Add(new Audio(vkaudio));
                    }
                }));
            }    
        }
    }
}
