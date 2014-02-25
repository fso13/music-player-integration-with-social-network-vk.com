using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;
using ListBox = System.Windows.Controls.ListBox;
using System.IO;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        public System.Timers.Timer TimerPosition = new System.Timers.Timer();
        public System.Timers.Timer Timer2 = new System.Timers.Timer();

        public bool FlagPlaylistVisible = true;//флаг для определения свернутости плейлиста
        public bool FlagPlay;
        public bool FlagPrev = true;

        public double Time1;
        public double Time2;
        public int CurrentPlayIndex;
        public static VkUser User;// текущий юзер
        public double SizeHeight;// переменная, хранящая ширину формы до свернутости
        public static readonly VkHelper VkApi = new VkHelper();//для работы с вконтактом
        public static List<VkAlbom> Alboms = new List<VkAlbom>();
        public ListBox CurrentListBox;
        public int Stream;//для играния
        public int OldNumber;
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
            FlagPlaylistVisible = !FlagPlaylistVisible;
            if (!FlagPlaylistVisible)
            {
                SizeHeight = Height;
                Height = 94;
            }
            else
            {
                Height = SizeHeight;
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            //VkApi.AccessToken = "501e669057305704920915316838e10b8c30adff0f7f037d135b0a9a92d4b3c20d2a5ed3dfd0bc1414d20";
            if (File.Exists("token.txt"))
            {
                VkApi.AccessToken = File.ReadAllText("token.txt");
            }
            else
            {
                File.Create("token.txt");
                VkApi.AccessToken = "";
            }
            if (VkApi.AccessToken == "")
            {
                var form = new LoginVK();
                form.ShowDialog();
                VkApi.AccessToken = form.AccessToken;
            }
            if (!Directory.Exists("image"))
                Directory.CreateDirectory("image");

            if (VkApi.AccessToken != null && !VkApi.AccessToken.StartsWith("#error"))
            {
                User = VkApi.GetUser();
                var list = VkApi.GetAudio(User.Uid, "");

                AddAudioPlayList(PlayListBox, list);
                GetAlbomsUser();
                PlayListTabs.SelectedIndex = 0;
                var t = (ScrollViewer)PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
                t.ScrollToHorizontalOffset(0);
                TimerPosition.Elapsed += TimerPosition1_Tick;
                TimerPosition.Interval = 1000;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Ошибка авторизации.");
            }

        }

        public void SetTagPlay(int i)
        {
            if (CurrentListBox.Items.Count <= i) return;
            var selectedItem = CurrentListBox.Items[i];
            var selectedListBoxItem = CurrentListBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
            if (selectedListBoxItem != null)
                selectedListBoxItem.Tag = "Played";
        }

        public void SetTagNull(int i)
        {
            if (CurrentListBox.Items.Count <= i) return;
            var selectedItem = CurrentListBox.Items[i];
            var selectedListBoxItem = CurrentListBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
            if (selectedListBoxItem != null)
                selectedListBoxItem.Tag = null;
        }

        private void TimerPosition1_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                Time1 = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream));
                Time2 = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream));

                var t1 = Convert.ToString((int)Time1 / 60);
                var t2 = Convert.ToString((int)Time1 % 60);
                if (t1.Length == 1) t1 = "0" + t1;
                if (t2.Length == 1) t2 = "0" + t2;
                TextTime.Text = t1 + " : " + t2;
                OldNumber = CurrentPlayIndex;

                SliderTrack.Value = Time1;
                MyTaskItem.ProgressValue = (Time1) / Time2;
                if (!Time1.Equals(Time2)) return;

                if (CurrentPlayIndex == CurrentListBox.Items.Count - 1)
                {
                    CurrentPlayIndex = 0;
                }
                else
                {
                    CurrentPlayIndex = CurrentPlayIndex + 1;
                }
                Play(CurrentListBox);
            }));
        }

        public void GetAlbomsUser()
        {
            Alboms = VkApi.GetAlbom();
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                foreach (var albom in Alboms)
                {
                    var list = VkApi.GetAudio(User.Uid, albom.AlbumId);
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
            OldNumber = CurrentPlayIndex;
            CurrentPlayIndex = ((ListBox)sender).SelectedIndex;
            CurrentListBox = (ListBox)sender;
            Play((ListBox)sender);
            TimerPosition.Start();
        }

        public void Play(ListBox sender)
        {
            SetTagNull(OldNumber);
            if (!((Audio) sender.Items[CurrentPlayIndex]).IsPlayed)
            {
                if (FlagPrev)
                {
                    for (var i = CurrentPlayIndex + 1; i < sender.Items.Count; i++)
                    {
                        if (!((Audio)sender.Items[i]).IsPlayed) continue;
                        CurrentPlayIndex = i;
                        break;
                    }
                }
                else
                {
                    for (var i = CurrentPlayIndex - 1; i >= 0; i--)
                    {
                        if (!((Audio)sender.Items[i]).IsPlayed) continue;
                        CurrentPlayIndex = i;
                        break;
                    }
                }
            }
            Bass.BASS_StreamFree(Stream);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Stream = Bass.BASS_StreamCreateURL(((Audio)sender.Items[CurrentPlayIndex]).Path, 0,
                BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
            if (!Bass.BASS_ChannelPlay(Stream, false)) return;
            SetTagPlay(CurrentPlayIndex);
            OldNumber = CurrentPlayIndex;
            FlagPlay = true;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ((float) SliderVolum.Value)/100);
            SliderTrack.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream));
            BeginText.Text = ((Audio)sender.Items[CurrentPlayIndex]).Title;
            var font = new Font("Segoe UI Ligh", 12);
            var ta = new ThicknessAnimation
            {
                From = new Thickness(240, 0, 0, 0),
                To = new Thickness(Convert.ToDouble(-(TextRenderer.MeasureText(BeginText.Text, font)).Width), 0, 0, 0),
                Duration = TimeSpan.FromMilliseconds(10000),
                RepeatBehavior = RepeatBehavior.Forever
            };
            BeginText.BeginAnimation(MarginProperty, ta);
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

        private void SliderTrack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(Math.Abs(e.NewValue-e.OldValue)<=1.1))
            {
                Bass.BASS_ChannelSetPosition(Stream, SliderTrack.Value);
            }
        }

        private void thumbNext_Click(object sender, EventArgs e)
        {
            ClickNext();
        }

        private void thumbPrevious_Click(object sender, EventArgs e)
        {
            ClickPrev();
        }

        private void thumbPlay_Click(object sender, EventArgs e)
        {
            ClickPlay();
        }

        private void SliderVolum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ((float)SliderVolum.Value) / 100);

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

        private void BPrev_Click(object sender, RoutedEventArgs e)
        {
            ClickPrev();
        }

        private void BNext_Click(object sender, RoutedEventArgs e)
        {
            ClickNext();
        }

        private void BStop_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(Stream);
            SliderTrack.Value = 0;
            TextTime.Text = "00 : 00";
            FlagPlay = false;
        }

        private void BPlay_Click(object sender, RoutedEventArgs e)
        {
            ClickPlay();
        }

        public void ClickPrev()
        {
            FlagPrev = false;
            OldNumber = CurrentPlayIndex;
            if (CurrentPlayIndex == 0)
            {
                CurrentPlayIndex = CurrentListBox.Items.Count - 1;
            }
            else
            {
                CurrentPlayIndex = CurrentPlayIndex - 1;
            }
            CurrentListBox.ScrollIntoView(CurrentListBox.Items[CurrentPlayIndex]);
            Play(CurrentListBox);
        }

        public void ClickNext()
        {
            FlagPrev = true;
            OldNumber = CurrentPlayIndex;
            if (CurrentPlayIndex == CurrentListBox.Items.Count - 1)
            {
                CurrentPlayIndex = 0;
            }
            else
            {
                CurrentPlayIndex = CurrentPlayIndex + 1;
            }
            CurrentListBox.ScrollIntoView(CurrentListBox.Items[CurrentPlayIndex]);
            Play(CurrentListBox);
        }

        public void ClickPlay()
        {
            if (!FlagPlay)
            {
                if (CurrentListBox.Items.Count <= 0) return;
                Bass.BASS_ChannelPlay(Stream, false);
                FlagPlay = true;
                TimerPosition.Start();
                var imgBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"Image//play1.png", UriKind.Relative))
                };
                BPlay.Background = imgBrush;
            }
            else
            {
                FlagPlay = false;
                Bass.BASS_ChannelPause(Stream);
                TimerPosition.Stop();
                var imgBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"Image//pause1.png", UriKind.Relative))
                };
                BPlay.Background = imgBrush;
            }
        }

        private void ScrollViewer_ScrollChanged_1(object sender, ScrollChangedEventArgs e)
        {
            if(FlagPlay)
                SetTagPlay(CurrentPlayIndex);
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            if (!VkApi.AccessToken.StartsWith("#error"))
            {
                File.WriteAllText("token.txt", VkApi.AccessToken);
            }

        }
        
    }
}
