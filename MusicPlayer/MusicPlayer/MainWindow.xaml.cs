using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MusicPlayer.domain;
using MusicPlayer.local;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;

namespace MusicPlayer
{
    public partial class MainWindow
    {
        private const string LibraryTab = "Музыка";
        private const string RadioTab = "Радио";

        private int _sorting;
        private static readonly int[] FxEq = new int[18];
        private static readonly double[] EqValues = new double[18];
        private Eq _eq;
        public System.Timers.Timer TimerPosition = new System.Timers.Timer();
        public bool FlagPlaylistVisible = true;
        public bool FlagPlay;
        public bool FlagPrev = true;
        public int CurrentPlayIndex;
        public double SizeHeight;
        public ListBox CurrentListBox;
        public static int Stream;
        public int OldNumber;
        private string _musicFolder = "";
        private bool _seeking;

        public MainWindow()
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
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
            FindText.TextChanged += (s, args) => ApplyFilter();
            TimerPosition.Elapsed += TimerPosition1_Tick;
            TimerPosition.Interval = 500;
            EnsureRadioTab();
            var saved = LibraryStore.Load();
            _musicFolder = saved.MusicFolder ?? "";
            if (Directory.Exists(_musicFolder))
                ReplaceTracks(LibraryList(), LocalLibrary.Scan(_musicFolder));
            foreach (var playlist in saved.Playlists)
            {
                var tracks = new List<Audio>();
                foreach (var path in playlist.Paths)
                {
                    if (!File.Exists(path)) continue;
                    tracks.Add(LocalLibrary.FromFile(path, string.IsNullOrEmpty(_musicFolder) ? Path.GetDirectoryName(path) : _musicFolder));
                }
                ProbeDurations(tracks);
                AddPlaylistTab(playlist.Name, tracks);
            }
            var radios = saved.Radios.Select(radio => new Audio
            {
                Title = radio.Name,
                Info = radio.Url,
                Path = radio.Url,
                IsRadio = true,
                IsLocal = false,
                IsPlayed = true,
                Duration = "эфир"
            }).ToList();
            ReplaceTracks(RadioList(), radios);
            _eq = new Eq(EqValues);
            PlayListTabs.SelectedIndex = 0;
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            SaveLibrary();
            Bass.BASS_StreamFree(Stream);
            Bass.BASS_Free();
        }

        private ListBox LibraryList()
        {
            return (ListBox)((TabItem)PlayListTabs.Items[0]).Content;
        }

        private ListBox RadioList()
        {
            for (var i = 0; i < PlayListTabs.Items.Count; i++)
            {
                var tab = (TabItem)PlayListTabs.Items[i];
                if ((string)tab.Header == RadioTab) return (ListBox)tab.Content;
            }
            return null;
        }

        private void EnsureRadioTab()
        {
            if (RadioList() != null) return;
            AddPlaylistTab(RadioTab, new List<Audio>());
        }

        private void SetPlayIcon(bool playing)
        {
            BPlay.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(playing ? "pack://application:,,,/Image/pause1.png" : "pack://application:,,,/Image/play1.png"))
            };
        }

        private void SelectTab(ListBox list)
        {
            foreach (TabItem tab in PlayListTabs.Items)
            {
                if (tab.Content == list) PlayListTabs.SelectedItem = tab;
            }
        }

        private TabItem SelectedTab()
        {
            return PlayListTabs.SelectedItem as TabItem;
        }

        private List<Audio> SourceOf(ListBox list)
        {
            var source = list.Tag as List<Audio>;
            if (source != null) return source;
            source = new List<Audio>();
            list.Tag = source;
            return source;
        }

        private void ReplaceTracks(ListBox list, List<Audio> tracks)
        {
            ProbeDurations(tracks.Where(track => track.IsLocal).ToList());
            list.Tag = tracks;
            Bind(list, tracks);
        }

        private static void ProbeDurations(IList<Audio> tracks)
        {
            foreach (var track in tracks)
            {
                if (!track.IsLocal || !File.Exists(track.Path) || track.Duration != "--:--") continue;
                var channel = Bass.BASS_StreamCreateFile(track.Path, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                if (channel == 0) continue;
                var seconds = Bass.BASS_ChannelBytes2Seconds(channel, Bass.BASS_ChannelGetLength(channel));
                Bass.BASS_StreamFree(channel);
                track.Duration = LocalLibrary.FormatDuration(seconds);
            }
        }

        private void Bind(ListBox list, IEnumerable<Audio> tracks)
        {
            var query = (FindText.Text ?? "").Trim();
            list.Items.Clear();
            foreach (var track in tracks)
            {
                if (query.Length > 0 &&
                    (track.Title ?? "").IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                    (track.Info ?? "").IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0)
                    continue;
                list.Items.Add(track);
            }
        }

        private void ApplyFilter()
        {
            var tab = SelectedTab();
            if (tab == null) return;
            var list = tab.Content as ListBox;
            if (list == null) return;
            Bind(list, SourceOf(list));
        }

        private void AddPlaylistTab(string name, List<Audio> tracks)
        {
            var bc = new BrushConverter();
            var item = new TabItem
            {
                Style = (Style)FindResource("TabItemStyle1"),
                Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF"),
                Header = name,
                FontFamily = new FontFamily("Segoe UI Light"),
                FontSize = 16
            };
            var list = new ListBox
            {
                Style = (Style)FindResource("ListBoxStyle2"),
                Background = (Brush)bc.ConvertFrom("#FF000000"),
                BorderBrush = (Brush)bc.ConvertFrom("#FF000000"),
                ItemContainerStyle = (Style)FindResource("ListBoxItemStyle1"),
                Foreground = (Brush)bc.ConvertFrom("#FF5D6655"),
                Tag = tracks ?? new List<Audio>()
            };
            list.MouseDoubleClick += PlayListBox_MouseDoubleClick;
            item.Content = list;
            var radio = RadioList();
            if (radio != null && name != RadioTab)
                PlayListTabs.Items.Insert(PlayListTabs.Items.Count - 1, item);
            else
                PlayListTabs.Items.Add(item);
            Bind(list, SourceOf(list));
            PlayListTabs.SelectedItem = item;
        }

        private void SaveLibrary()
        {
            var file = new LibraryFile { MusicFolder = _musicFolder ?? "" };
            for (var i = 0; i < PlayListTabs.Items.Count; i++)
            {
                var tab = (TabItem)PlayListTabs.Items[i];
                var name = (string)tab.Header;
                var list = (ListBox)tab.Content;
                var tracks = SourceOf(list);
                if (name == LibraryTab) continue;
                if (name == RadioTab)
                {
                    file.Radios = tracks.Where(track => track.IsRadio).Select(track => new RadioFile
                    {
                        Name = track.Title,
                        Url = track.Path
                    }).ToList();
                    continue;
                }
                file.Playlists.Add(new PlaylistFile
                {
                    Name = name,
                    Paths = tracks.Where(track => track.IsLocal).Select(track => track.Path).ToList()
                });
            }
            LibraryStore.Save(file);
        }

        private void PlayListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = (ListBox)sender;
            if (list.SelectedIndex < 0) return;
            OldNumber = CurrentPlayIndex;
            CurrentPlayIndex = list.SelectedIndex;
            CurrentListBox = list;
            Play(list);
            TimerPosition.Start();
        }

        public void Play(ListBox sender)
        {
            try
            {
                if (sender == null || sender.Items.Count == 0) return;
                if (CurrentPlayIndex < 0 || CurrentPlayIndex >= sender.Items.Count) CurrentPlayIndex = 0;
                SetTagNull(OldNumber);
                var audio = (Audio)sender.Items[CurrentPlayIndex];
                if (!audio.IsPlayed) return;

                Bass.BASS_StreamFree(Stream);
                Stream = audio.IsRadio
                    ? Bass.BASS_StreamCreateURL(audio.Path, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero)
                    : Bass.BASS_StreamCreateFile(audio.Path, 0, 0, BASSFlag.BASS_DEFAULT);
                if (Stream == 0 || !Bass.BASS_ChannelPlay(Stream, false))
                {
                    MessageBox.Show("Не удалось воспроизвести: " + audio.Title);
                    return;
                }
                try { SetEqOnStream(); } catch (Exception) { }
                SetTagPlay(CurrentPlayIndex);
                OldNumber = CurrentPlayIndex;
                FlagPlay = true;
                SetPlayIcon(true);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)SliderVolum.Value / 100);
                var length = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream));
                SliderTrack.Maximum = audio.IsRadio || length <= 0 ? 1 : length;
                SliderTrack.Value = 0;
                BeginText.Text = audio.Title;
                var font = new System.Drawing.Font("Segoe UI", 12);
                var animation = new ThicknessAnimation
                {
                    From = new Thickness(240, 0, 0, 0),
                    To = new Thickness(Convert.ToDouble(-(System.Windows.Forms.TextRenderer.MeasureText(BeginText.Text, font)).Width), 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(10000),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                BeginText.BeginAnimation(MarginProperty, animation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TimerPosition1_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                if (CurrentListBox == null || CurrentPlayIndex < 0 || CurrentPlayIndex >= CurrentListBox.Items.Count) return;
                var audio = (Audio)CurrentListBox.Items[CurrentPlayIndex];
                if (audio.IsRadio)
                {
                    TextTime.Text = "RADIO";
                    return;
                }
                var time = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream));
                var length = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream));
                TextTime.Text = LocalLibrary.FormatDuration(time);
                if (!_seeking && length > 0) SliderTrack.Value = Math.Min(time, SliderTrack.Maximum);
                if (length > 0) MyTaskItem.ProgressValue = time / length;
                if (length <= 0 || time < length - 0.4) return;
                ClickNext();
            }));
        }

        private void BNewPL_Click(object sender, RoutedEventArgs e)
        {
            var form = new NewPlaylist { Owner = this };
            form.ShowDialog();
            if (!form.OkOrcancel || string.IsNullOrWhiteSpace(form.NamePlaylist)) return;
            if (form.NamePlaylist == LibraryTab || form.NamePlaylist == RadioTab)
            {
                MessageBox.Show("Это имя занято.");
                return;
            }
            AddPlaylistTab(form.NamePlaylist.Trim(), new List<Audio>());
            SaveLibrary();
        }

        private void BPrevPl_Click(object sender, RoutedEventArgs e)
        {
            if (PlayListTabs.Items.Count == 0) return;
            PlayListTabs.SelectedIndex = PlayListTabs.SelectedIndex <= 0
                ? PlayListTabs.Items.Count - 1
                : PlayListTabs.SelectedIndex - 1;
        }

        private void BNextPl_Click(object sender, RoutedEventArgs e)
        {
            if (PlayListTabs.Items.Count == 0) return;
            PlayListTabs.SelectedIndex = PlayListTabs.SelectedIndex >= PlayListTabs.Items.Count - 1
                ? 0
                : PlayListTabs.SelectedIndex + 1;
        }

        private void SliderTrack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CurrentListBox == null || CurrentPlayIndex < 0 || CurrentPlayIndex >= CurrentListBox.Items.Count) return;
            if (((Audio)CurrentListBox.Items[CurrentPlayIndex]).IsRadio) return;
            if (Math.Abs(e.NewValue - e.OldValue) <= 1.1) return;
            _seeking = true;
            Bass.BASS_ChannelSetPosition(Stream, SliderTrack.Value);
            _seeking = false;
        }

        private void thumbNext_Click(object sender, EventArgs e) { ClickNext(); }
        private void thumbPrevious_Click(object sender, EventArgs e) { ClickPrev(); }
        private void thumbPlay_Click(object sender, EventArgs e) { ClickPlay(); }
        private void BPrev_Click(object sender, RoutedEventArgs e) { ClickPrev(); }
        private void BNext_Click(object sender, RoutedEventArgs e) { ClickNext(); }
        private void BPlay_Click(object sender, RoutedEventArgs e) { ClickPlay(); }

        private void SliderVolum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)SliderVolum.Value / 100);
        }

        public void ClickPrev()
        {
            if (!EnsureCurrentList()) return;
            FlagPrev = false;
            if (CurrentListBox.Items.Count == 0) return;
            var audio = CurrentPlayIndex >= 0 && CurrentPlayIndex < CurrentListBox.Items.Count
                ? (Audio)CurrentListBox.Items[CurrentPlayIndex]
                : null;
            if (audio != null && !audio.IsRadio)
            {
                var time = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream));
                if (time > 3)
                {
                    Bass.BASS_ChannelSetPosition(Stream, 0);
                    return;
                }
            }
            OldNumber = CurrentPlayIndex;
            CurrentPlayIndex = CurrentPlayIndex <= 0 ? CurrentListBox.Items.Count - 1 : CurrentPlayIndex - 1;
            CurrentListBox.ScrollIntoView(CurrentListBox.Items[CurrentPlayIndex]);
            Play(CurrentListBox);
        }

        public void ClickNext()
        {
            if (!EnsureCurrentList()) return;
            FlagPrev = true;
            if (CurrentListBox.Items.Count == 0) return;
            OldNumber = CurrentPlayIndex;
            CurrentPlayIndex = CurrentPlayIndex >= CurrentListBox.Items.Count - 1 ? 0 : CurrentPlayIndex + 1;
            CurrentListBox.ScrollIntoView(CurrentListBox.Items[CurrentPlayIndex]);
            Play(CurrentListBox);
        }

        private bool EnsureCurrentList()
        {
            if (CurrentListBox != null) return true;
            var tab = SelectedTab();
            if (tab == null) return false;
            CurrentListBox = tab.Content as ListBox;
            CurrentPlayIndex = 0;
            OldNumber = 0;
            return CurrentListBox != null;
        }

        public void ClickPlay()
        {
            if (!EnsureCurrentList()) return;
            if (FlagPlay)
            {
                FlagPlay = false;
                Bass.BASS_ChannelPause(Stream);
                TimerPosition.Stop();
                SetPlayIcon(false);
                return;
            }
            if (Stream != 0)
            {
                Bass.BASS_ChannelPlay(Stream, false);
                FlagPlay = true;
                TimerPosition.Start();
                SetPlayIcon(true);
                return;
            }
            if (CurrentListBox.Items.Count <= 0) return;
            if (CurrentPlayIndex < 0 || CurrentPlayIndex >= CurrentListBox.Items.Count) CurrentPlayIndex = 0;
            Play(CurrentListBox);
            TimerPosition.Start();
        }

        private void BStop_Click(object sender, RoutedEventArgs e)
        {
            Bass.BASS_ChannelStop(Stream);
            SliderTrack.Value = 0;
            TextTime.Text = "00 : 00";
            FlagPlay = false;
            TimerPosition.Stop();
        }

        private void ScrollViewer_ScrollChanged_1(object sender, ScrollChangedEventArgs e)
        {
            if (FlagPlay) SetTagPlay(CurrentPlayIndex);
        }

        private void TextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ApplyFilter();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var item = (TabItem)((Grid)((Button)sender).Parent).TemplatedParent;
            var name = (string)item.Header;
            if (name == LibraryTab || name == RadioTab) return;
            PlayListTabs.Items.Remove(item);
            SaveLibrary();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (_eq != null) _eq.Close();
            Close();
        }

        private void minimyz_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        private void SliderVolum_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SliderVolum.Value = Math.Max(0, Math.Min(100, SliderVolum.Value + (e.Delta > 0 ? 5 : -5)));
        }

        private void SliderTrack_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CurrentListBox != null && CurrentPlayIndex >= 0 && CurrentPlayIndex < CurrentListBox.Items.Count &&
                ((Audio)CurrentListBox.Items[CurrentPlayIndex]).IsRadio) return;
            SliderTrack.Value += e.Delta > 0 ? 5 : -5;
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку с музыкой",
                SelectedPath = Directory.Exists(_musicFolder) ? _musicFolder : ""
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            _musicFolder = dialog.SelectedPath;
            ReplaceTracks(LibraryList(), LocalLibrary.Scan(_musicFolder));
            PlayListTabs.SelectedIndex = 0;
            SaveLibrary();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Audio station;
            if (!AskRadio(out station)) return;
            var list = RadioList();
            var tracks = SourceOf(list);
            tracks.Add(station);
            Bind(list, tracks);
            SelectTab(list);
            SaveLibrary();
        }

        private void BAny_Click(object sender, RoutedEventArgs e)
        {
            GroupButton_Click(sender, e);
        }

        private void BAdd_Click(object sender, RoutedEventArgs e)
        {
            var tab = SelectedTab();
            if (tab == null) return;
            if ((string)tab.Header == RadioTab)
            {
                Button_Click_2(sender, e);
                return;
            }
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Аудио|*.mp3;*.wav;*.ogg;*.flac;*.m4a;*.aac;*.wma;*.aiff;*.aif|Все файлы|*.*"
            };
            if (dialog.ShowDialog() != true) return;
            var list = (ListBox)tab.Content;
            var tracks = SourceOf(list);
            var root = string.IsNullOrEmpty(_musicFolder) ? Path.GetDirectoryName(dialog.FileName) : _musicFolder;
            foreach (var file in dialog.FileNames)
                tracks.Add(LocalLibrary.FromFile(file, root));
            ProbeDurations(tracks);
            Bind(list, tracks);
            SaveLibrary();
        }

        private void BDelete_Click(object sender, RoutedEventArgs e)
        {
            var tab = SelectedTab();
            if (tab == null) return;
            var list = (ListBox)tab.Content;
            var audio = list.SelectedItem as Audio;
            if (audio == null) return;
            SourceOf(list).Remove(audio);
            Bind(list, SourceOf(list));
            SaveLibrary();
        }

        private void BSort_Click(object sender, RoutedEventArgs e)
        {
            var tab = SelectedTab();
            if (tab == null) return;
            var list = (ListBox)tab.Content;
            var tracks = SourceOf(list);
            _sorting = (_sorting + 1) % 2;
            if (_sorting == 1)
                tracks.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase));
            else
                tracks.Sort((a, b) => string.Compare(b.Title, a.Title, StringComparison.CurrentCultureIgnoreCase));
            Bind(list, tracks);
        }

        private bool AskRadio(out Audio station)
        {
            station = null;
            var nameBox = new TextBox { Margin = new Thickness(8), Text = "Станция" };
            var urlBox = new TextBox { Margin = new Thickness(8), Text = "http://" };
            var ok = new Button { Content = "Добавить", Width = 90, Margin = new Thickness(8), IsDefault = true };
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = "Название", Margin = new Thickness(8, 8, 8, 0) });
            panel.Children.Add(nameBox);
            panel.Children.Add(new TextBlock { Text = "Адрес потока", Margin = new Thickness(8, 4, 8, 0) });
            panel.Children.Add(urlBox);
            panel.Children.Add(ok);
            var dialog = new Window
            {
                Title = "Интернет-радио",
                Content = panel,
                Width = 360,
                Height = 210,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };
            var accepted = false;
            ok.Click += (s, args) => { accepted = true; dialog.Close(); };
            dialog.ShowDialog();
            if (!accepted) return false;
            var name = (nameBox.Text ?? "").Trim();
            var url = (urlBox.Text ?? "").Trim();
            Uri parsed;
            if (name.Length == 0 || !Uri.TryCreate(url, UriKind.Absolute, out parsed) ||
                (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("Нужны название и адрес http:// или https://");
                return false;
            }
            station = new Audio
            {
                Title = name,
                Info = url,
                Path = url,
                IsRadio = true,
                IsLocal = false,
                IsPlayed = true,
                Duration = "эфир"
            };
            return true;
        }

        public void SetTagPlay(int i)
        {
            if (CurrentListBox == null || CurrentListBox.Items.Count <= i || i < 0) return;
            var item = CurrentListBox.ItemContainerGenerator.ContainerFromItem(CurrentListBox.Items[i]) as ListBoxItem;
            if (item != null) item.Tag = "Played";
        }

        public void SetTagNull(int i)
        {
            if (CurrentListBox == null || CurrentListBox.Items.Count <= i || i < 0) return;
            var item = CurrentListBox.ItemContainerGenerator.ContainerFromItem(CurrentListBox.Items[i]) as ListBoxItem;
            if (item != null) item.Tag = null;
        }

        private void equalizerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_eq != null && !_eq.Activate())
            {
                _eq = new Eq(EqValues);
                try { SetEqOnStream(); } catch (Exception) { }
                _eq.Left = Left + Width;
                _eq.Top = Top;
                _eq.Show();
            }
            else if (_eq != null && _eq.Activate())
            {
                _eq.Close();
            }
        }

        private static void SetEqOnStream()
        {
            if (Stream == 0) return;
            var eq = new BASS_DX8_PARAMEQ { fBandwidth = 18f, fGain = 0f };
            float[] centers = { 31, 63, 87, 125, 175, 250, 350, 500, 700, 1000, 1400, 2000, 2800, 4000, 5600, 8000, 11200, 16000 };
            for (var i = 0; i < centers.Length; i++)
            {
                FxEq[i] = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);
                eq.fCenter = centers[i];
                eq.fGain = (float)EqValues[i];
                Bass.BASS_FXSetParameters(FxEq[i], eq);
            }
        }

        public static void UpdateEq(int band, double gain)
        {
            if (band < 0 || band >= EqValues.Length) return;
            EqValues[band] = gain;
            var eq = new BASS_DX8_PARAMEQ();
            if (Bass.BASS_FXGetParameters(FxEq[band], eq))
            {
                eq.fGain = (float)gain;
                Bass.BASS_FXSetParameters(FxEq[band], eq);
            }
        }
    }
}
