﻿using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
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
        private int _sorting;
        private static readonly int[] FxEq = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        private static readonly double[] Eq = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        private Eq _eq;
        public System.Timers.Timer TimerPosition = new System.Timers.Timer();
        public System.Timers.Timer Timer2 = new System.Timers.Timer();
        public bool FlagPlaylistVisible = true; //флаг для определения свернутости плейлиста
        public bool FlagPlay;
        public bool FlagPrev = true;

        public double Time1;
        public double Time2;
        public int CurrentPlayIndex;
        public static VkUser User; // текущий юзер
        public double SizeHeight; // переменная, хранящая ширину формы до свернутости
        public static readonly VkHelper VkApi = new VkHelper(); //для работы с вконтактом
        public static List<VkAlbom> Alboms = new List<VkAlbom>();
        public static List<VkGroup> Groups = new List<VkGroup>();
        public static List<VkUser> Friends = new List<VkUser>();

        public ListBox CurrentListBox;
        public static int Stream; //для играния
        public int OldNumber;

        public MainWindow()
        {
            BassNet.Registration("stha64@telia.com", "2X28183316182322");
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            SetBFX_EQ();
            _sorting = 0;
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
            try
            {
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
                    var form = new LoginVk();
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
                    var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
                    t.ScrollToHorizontalOffset(0);
                    TimerPosition.Elapsed += TimerPosition1_Tick;
                    TimerPosition.Interval = 1000;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(@"Ошибка авторизации.");
                }
                _eq = new Eq(Eq);
                SetBFX_EQ();
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        public void SetTagPlay(int i)
        {
            if (CurrentListBox.Items.Count <= i) return;
            var selectedItem = CurrentListBox.Items[i];
            var selectedListBoxItem =
                CurrentListBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
            if (selectedListBoxItem != null)
                selectedListBoxItem.Tag = "Played";
        }

        public void SetTagNull(int i)
        {
            if (CurrentListBox.Items.Count <= i) return;
            var selectedItem = CurrentListBox.Items[i];
            var selectedListBoxItem =
                CurrentListBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
            if (selectedListBoxItem != null)
                selectedListBoxItem.Tag = null;
        }

        private void TimerPosition1_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                Time1 = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream));
                Time2 = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream));

                var t1 = Convert.ToString((int) Time1/60);
                var t2 = Convert.ToString((int) Time1%60);
                if (t1.Length == 1) t1 = "0" + t1;
                if (t2.Length == 1) t2 = "0" + t2;
                TextTime.Text = t1 + " : " + t2;
                OldNumber = CurrentPlayIndex;

                SliderTrack.Value = Time1;
                MyTaskItem.ProgressValue = (Time1)/Time2;
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
                    NewPlaylist(albom.Title, list);
                }
            }));

        }

        public void NewPlaylist(string name, List<VkAudio> list2)
        {
            var bc = new BrushConverter();

            var item1 = new TabItem
            {
                Style = (Style) FindResource("TabItemStyle1"),
                Foreground = (Brush) bc.ConvertFrom("#FFFFFFFF"),
                Header = name,
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

            list.MouseDoubleClick += PlayListBox_MouseDoubleClick;
            item1.Content = list;
            PlayListTabs.Items.Add(item1);
            PlayListTabs.SelectedItem = item1;
            AddAudioPlayList((ListBox) (item1).Content, list2);
        }

        private void PlayListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OldNumber = CurrentPlayIndex;
            CurrentPlayIndex = ((ListBox) sender).SelectedIndex;
            CurrentListBox = (ListBox) sender;
            Play((ListBox) sender);
            TimerPosition.Start();
        }

        public void Play(ListBox sender)
        {
            try
            {
                SetTagNull(OldNumber);
                if (!((Audio) sender.Items[CurrentPlayIndex]).IsPlayed)
                    //todo проверять на индекс, возможно что то не так
                {
                    if (FlagPrev)
                    {
                        for (var i = CurrentPlayIndex + 1; i < sender.Items.Count; i++)
                        {
                            if (!((Audio) sender.Items[i]).IsPlayed) continue;
                            CurrentPlayIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        for (var i = CurrentPlayIndex - 1; i >= 0; i--)
                        {
                            if (!((Audio) sender.Items[i]).IsPlayed) continue;
                            CurrentPlayIndex = i;
                            break;
                        }
                    }
                }
                VkHelper.CheckEthernet();
                Bass.BASS_StreamFree(Stream);
                Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                Stream = Bass.BASS_StreamCreateURL(((Audio) sender.Items[CurrentPlayIndex]).Path, 0,
                    BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
                if (!Bass.BASS_ChannelPlay(Stream, false)) return;
                SetTagPlay(CurrentPlayIndex);
                OldNumber = CurrentPlayIndex;
                FlagPlay = true;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ((float) SliderVolum.Value)/100);
                SliderTrack.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream));
                BeginText.Text = ((Audio) sender.Items[CurrentPlayIndex]).Title;
                var font = new Font("Segoe UI Ligh", 12);
                var ta = new ThicknessAnimation
                {
                    From = new Thickness(240, 0, 0, 0),
                    To =
                        new Thickness(Convert.ToDouble(-(TextRenderer.MeasureText(BeginText.Text, font)).Width), 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(10000),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                BeginText.BeginAnimation(MarginProperty, ta);
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void BNewPL_Click(object sender, RoutedEventArgs e)
        {
            var form = new NewPlaylist();
            form.ShowDialog();
            if (form.OkOrcancel)
            {
                NewPlaylist(form.NamePlaylist, null);
                var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
                t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex*110);
            }
        }

        private void BPrevPl_Click(object sender, RoutedEventArgs e)
        {
            var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
            PlayListTabs.FindName("LeftClickButton");
            if (PlayListTabs.SelectedIndex - 1 < 0)
            {
                PlayListTabs.SelectedIndex = PlayListTabs.Items.Count - 1;
            }
            else
            {
                PlayListTabs.SelectedIndex = PlayListTabs.SelectedIndex - 1;
            }
            t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex*110);

        }

        private void BNextPl_Click(object sender, RoutedEventArgs e)
        {
            var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);

            if (PlayListTabs.SelectedIndex + 1 > PlayListTabs.Items.Count - 1)
            {
                PlayListTabs.SelectedIndex = 0;
            }
            else
            {
                PlayListTabs.SelectedIndex = PlayListTabs.SelectedIndex + 1;
            }
            t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex*110);

        }

        private void SliderTrack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(Math.Abs(e.NewValue - e.OldValue) <= 1.1))
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
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ((float) SliderVolum.Value)/100);

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
            if (CurrentListBox == null)
            {
                CurrentListBox = (ListBox) ((TabItem) PlayListTabs.Items[0]).FindName("PlayListBox");
                CurrentPlayIndex = 0;
                OldNumber = 0;
                Play(CurrentListBox);
            }
            else
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
        }

        private void ScrollViewer_ScrollChanged_1(object sender, ScrollChangedEventArgs e)
        {
            if (FlagPlay)
                SetTagPlay(CurrentPlayIndex);
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            if (!VkApi.AccessToken.StartsWith("#error"))
            {
                File.WriteAllText("token.txt", VkApi.AccessToken);
            }

        }

        private void TextBox_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)

                {
                    var list = VkApi.FindAudio(FindText.Text);
                    if (list == null) return;
                    for (var i = 0; i < PlayListTabs.Items.Count; i++)
                    {
                        if ((string) ((TabItem) PlayListTabs.Items[i]).Header == "Найденые...")
                        {
                            PlayListTabs.Items.Remove(PlayListTabs.Items[i]);
                            break;
                        }

                    }
                    NewPlaylist("Найденые...", list);
                    var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
                    t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex*110);
                }
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var item = (TabItem) ((Grid) ((System.Windows.Controls.Button) sender).Parent).TemplatedParent;
            if ((string) item.Header != "My music")
                PlayListTabs.Items.Remove(item);
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            _eq.Close();
            Close();
            Process.GetCurrentProcess().Kill();
        }

        private void minimyz_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;

            if (Topmost)
            {
                var imgBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"Image//button2.png", UriKind.Relative))
                };
                Button1.Background = imgBrush;
            }
            else
            {
                var imgBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"Image//button1.png", UriKind.Relative))
                };
                Button1.Background = imgBrush;
            }
        }

        private void SliderVolum_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                SliderVolum.Value += 5;
            }
            else
            {
                SliderVolum.Value -= 5;
            }
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ((float) SliderVolum.Value)/100);

        }

        private void SliderTrack_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                SliderTrack.Value += 5;
            }
            else
            {
                SliderTrack.Value -= 5;
            }
            Bass.BASS_ChannelSetPosition(Stream, SliderTrack.Value);
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Groups.Count == 0)
                {
                    Groups = VkApi.GetGroups(User.Uid);
                    GroupButton.ContextMenu = new System.Windows.Controls.ContextMenu();

                    foreach (var group in Groups)
                    {
                        var m = new System.Windows.Controls.MenuItem {Header = @group.Name};
                        var bc = new BrushConverter();
                        m.Background = (Brush) bc.ConvertFrom("#FF2D343A");
                        m.Foreground = (Brush) bc.ConvertFrom("#FFECFDFC");
                        m.Click += m_Click3;
                        GroupButton.ContextMenu.Items.Add(m);
                    }
                }

                GroupButton.ContextMenu.IsOpen = true;
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void m_Click3(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = GroupButton.ContextMenu.Items.IndexOf(sender);
                var list = VkApi.GetAudio(Groups[index].Gid, "");
                if (list == null) return;
                for (var i = 0; i < PlayListTabs.Items.Count; i++)
                {
                    if ((string) ((TabItem) PlayListTabs.Items[i]).Header == Groups[index].Name)
                    {
                        PlayListTabs.Items.Remove(PlayListTabs.Items[i]);
                        break;
                    }

                }
                NewPlaylist(Groups[index].Name, list);
                var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
                t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex*110);
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Friends.Count == 0)
                {
                    Friends = VkApi.GetFriends(User.Uid);
                    FriendsButton.ContextMenu = new System.Windows.Controls.ContextMenu();

                    foreach (var friend in Friends)
                    {
                        var m = new System.Windows.Controls.MenuItem {Header = friend.FirstName + " " + friend.LastName};
                        var bc = new BrushConverter();
                        m.Background = (Brush) bc.ConvertFrom("#FF2D343A");
                        m.Foreground = (Brush) bc.ConvertFrom("#FFECFDFC");
                        m.Click += m_ClickFriends;
                        FriendsButton.ContextMenu.Items.Add(m);
                    }
                }

                FriendsButton.ContextMenu.IsOpen = true;
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void m_ClickFriends(object sender, RoutedEventArgs e)
        {
            try
            {
                var index = FriendsButton.ContextMenu.Items.IndexOf(sender);
                var list = VkApi.GetAudio(Friends[index].Uid, "");
                if (list == null) return;
                for (var i = 0; i < PlayListTabs.Items.Count; i++)
                {
                    if ((string) ((TabItem) PlayListTabs.Items[i]).Header ==
                        Friends[index].FirstName + " " + Friends[index].LastName)
                    {
                        PlayListTabs.Items.Remove(PlayListTabs.Items[i]);
                        break;
                    }

                }
                NewPlaylist(Friends[index].FirstName + " " + Friends[index].LastName, list);
                var t = (ScrollViewer) PlayListTabs.Template.FindName("ScrollViewerTab", PlayListTabs);
                t.ScrollToHorizontalOffset(PlayListTabs.SelectedIndex*110);
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void BAny_Click(object sender, RoutedEventArgs e)
        {

        }

        public static void SetBFX_EQ()
        {
            var eq = new BASS_DX8_PARAMEQ();
            FxEq[0] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[1] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[2] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[3] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[4] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[5] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[6] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[7] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[8] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[9] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[10] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[11] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[12] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[13] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[14] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[15] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[16] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            FxEq[17] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
            eq.fBandwidth = 18f;
            eq.fGain = 0f;
            eq.fCenter = 31f;
            Bass.BASS_FXSetParameters(FxEq[0], eq);
            eq.fCenter = 63f;
            Bass.BASS_FXSetParameters(FxEq[1], eq);
            eq.fCenter = 87f;
            Bass.BASS_FXSetParameters(FxEq[2], eq);
            eq.fCenter = 125f;
            Bass.BASS_FXSetParameters(FxEq[3], eq);
            eq.fCenter = 175f;
            Bass.BASS_FXSetParameters(FxEq[4], eq);
            eq.fCenter = 250f;
            Bass.BASS_FXSetParameters(FxEq[5], eq);
            eq.fCenter = 350f;
            Bass.BASS_FXSetParameters(FxEq[6], eq);
            eq.fCenter = 500f;
            Bass.BASS_FXSetParameters(FxEq[7], eq);
            eq.fCenter = 700f;
            Bass.BASS_FXSetParameters(FxEq[8], eq);
            eq.fCenter = 1000f;
            Bass.BASS_FXSetParameters(FxEq[9], eq);
            eq.fCenter = 1400f;
            Bass.BASS_FXSetParameters(FxEq[10], eq);
            eq.fCenter = 2000f;
            Bass.BASS_FXSetParameters(FxEq[11], eq);
            eq.fCenter = 2800f;
            Bass.BASS_FXSetParameters(FxEq[12], eq);
            eq.fCenter = 4000f;
            Bass.BASS_FXSetParameters(FxEq[13], eq);
            eq.fCenter = 5600f;
            Bass.BASS_FXSetParameters(FxEq[14], eq);
            eq.fCenter = 8000f;
            Bass.BASS_FXSetParameters(FxEq[15], eq);
            eq.fCenter = 11200f;
            Bass.BASS_FXSetParameters(FxEq[16], eq);
            eq.fCenter = 16000f;
            Bass.BASS_FXSetParameters(FxEq[17], eq);
        }

        public static void UpdateEq(int band, double gain)
        {
            Eq[band] = gain;
            var eq = new BASS_DX8_PARAMEQ();
            if (Bass.BASS_FXGetParameters(FxEq[band], eq))
            {
                eq.fGain = (float) gain;
                Bass.BASS_FXSetParameters(FxEq[band], eq);
            }
        }

        private void equalizerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_eq != null && !_eq.Activate())
            {
                _eq = new Eq(Eq);
                SetBFX_EQ();
                _eq.Left = Left + Width;
                _eq.Top = Top;
                _eq.Show();
            }
            else if (_eq != null && _eq.Activate())
            {
                _eq.Close();
            }
        }

        private void BSort_Click(object sender, RoutedEventArgs e)
        {

            var listBox = (ListBox) ((TabItem) PlayListTabs.SelectedItem).FindName("PlayListBox");
            if (listBox != null)
            {
                if (_sorting == 0)
                {
                    listBox.Items.SortDescriptions.Clear();
                    listBox.Items.SortDescriptions.Add(
                        new System.ComponentModel.SortDescription("Title",
                            System.ComponentModel.ListSortDirection.Ascending));
                    _sorting++;
                }
                else if (_sorting == 1)
                {
                    listBox.Items.SortDescriptions.Clear();
                    listBox.Items.SortDescriptions.Add(
                        new System.ComponentModel.SortDescription("Title",
                            System.ComponentModel.ListSortDirection.Descending));
                    _sorting++;
                }
                else if (_sorting == 2)
                {
                    listBox.Items.SortDescriptions.Clear();
                    listBox.Items.SortDescriptions.Add(
                        new System.ComponentModel.SortDescription("Duration",
                            System.ComponentModel.ListSortDirection.Ascending));
                    _sorting++;
                }
                else if (_sorting == 3)
                {
                    listBox.Items.SortDescriptions.Clear();
                    listBox.Items.SortDescriptions.Add(
                        new System.ComponentModel.SortDescription("Duration",
                            System.ComponentModel.ListSortDirection.Descending));
                    _sorting = 0;
                }
            }
            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.ShowDialog();
            //string path = dialog.FileName;
            //List<string> listMusic = new List<string>();
            //StreamReader reader = new StreamReader(path);
            //for (int i = 0; i < 100-45; i++)
            //{
            //    listMusic.Add(reader.ReadLine());
            //}

            //foreach (var itemAudio in listMusic)
            //{
            //    var filename = "E://Музыка//100//" + itemAudio + ".mp3";
            //    var webClient = new WebClient();
            //    //VkApi.FindAudio(itemAudio);
            //    webClient.DownloadFile(new Uri(VkApi.FindAudio(itemAudio)[0].Url), filename);
            //}
        }

        private void BAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string) ((TabItem) PlayListTabs.SelectedItem).Header == "Найденые...")
                {
                    var listBox = (ListBox) PlayListTabs.SelectedContent;
                    var audio = (Audio) listBox.SelectedItem;
                    VkApi.AddAudio(audio.OwnerID, audio.ID);
                    foreach (TabItem tabItem in PlayListTabs.Items)
                    {
                        if ((string) tabItem.Header == "My music")
                        {
                            var listBox2 = (ListBox) (tabItem).FindName("PlayListBox");
                            if (listBox2 != null) listBox2.Items.Insert(0, audio);
                        }
                    }
                }
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void BDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var listBox = (ListBox) ((TabItem) PlayListTabs.SelectedItem).FindName("PlayListBox");
                if (listBox != null)
                {
                    var audio = (Audio) listBox.SelectedItem;
                    VkApi.DeleteAudio(audio.OwnerID, audio.ID);
                    listBox.Items.Remove(listBox.SelectedItem);
                }
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }
    }
}




