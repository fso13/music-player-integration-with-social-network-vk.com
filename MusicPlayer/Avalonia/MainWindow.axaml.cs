using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MusicPlayer.domain;
using MusicPlayer.local;

namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        const string LibraryName = "Музыка";
        const string RadioName = "Радио";

        readonly Playback _playback = new Playback();
        readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        readonly List<Audio> _library = new List<Audio>();
        readonly List<PlaylistFile> _playlists = new List<PlaylistFile>();
        readonly List<RadioFile> _radios = new List<RadioFile>();
        readonly List<Sheet> _sheets = new List<Sheet>();
        readonly double[] _eqCenters = { 60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000 };
        readonly double[] _eqGains = new double[10];

        string _folder = "";
        List<Audio> _queue = new List<Audio>();
        int _index = -1;
        int _sorting;
        bool _playing;
        bool _seeking;
        bool _started;
        bool _playlistOpen = true;
        bool _repeat;
        bool _random;
        bool _muted;
        double _openHeight = 484;
        double _savedVolume = 50;
        Window _eqWindow;

        public MainWindow()
        {
            InitializeComponent();
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MusicPlayer");
            Directory.CreateDirectory(dir);
            LibraryStore.FilePath = Path.Combine(dir, "library.xml");
            _timer.Tick += Timer_Tick;
            Closed += (_, __) =>
            {
                if (_eqWindow != null) _eqWindow.Close();
                Save();
                _playback.Shutdown();
            };
            Opened += (_, __) => LoadSaved();
        }

        void LoadSaved()
        {
            if (!EnsureAudio()) return;
            var saved = LibraryStore.Load();
            _folder = saved.MusicFolder ?? "";
            _playlists.AddRange(saved.Playlists ?? new List<PlaylistFile>());
            _radios.AddRange(saved.Radios ?? new List<RadioFile>());
            if (Directory.Exists(_folder))
                _library.AddRange(WithDuration(LocalLibrary.Scan(_folder)));
            BuildSheets();
        }

        void BuildSheets()
        {
            _sheets.Clear();
            TabStrip.Children.Clear();
            AddSheet(LibraryName, "library", false);
            foreach (var playlist in _playlists)
                AddSheet(playlist.Name, "playlist", true);
            AddSheet(RadioName, "radio", false);
            SelectSheet(0);
        }

        void AddSheet(string name, string kind, bool closable)
        {
            var box = new ListBox();
            box.Classes.Add("tracks");
            box.ItemTemplate = (DataTemplate)Resources["TrackTemplate"];
            box.DoubleTapped += (_, __) => PlayRow(box.SelectedItem as TrackRow);
            var header = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 6 };
            header.Children.Add(new TextBlock { Text = name, Foreground = Brushes.White, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            var button = new Button { Classes = { "tab" }, Content = header };
            var sheet = new Sheet { Name = name, Kind = kind, Box = box, Header = button };
            if (closable)
            {
                var close = new Button { Classes = { "flat" }, Width = 12, Height = 12, Content = new Image { Source = Skin("close1.png"), Width = 8, Height = 8 } };
                close.Click += (_, __) => RemovePlaylist(sheet);
                header.Children.Add(close);
            }
            button.Click += (_, __) => SelectSheet(_sheets.IndexOf(sheet));
            _sheets.Add(sheet);
            TabStrip.Children.Add(button);
        }

        void SelectSheet(int index)
        {
            if (index < 0 || index >= _sheets.Count) return;
            foreach (var sheet in _sheets)
            {
                if (sheet.Header.Classes.Contains("on")) sheet.Header.Classes.Remove("on");
            }
            var current = _sheets[index];
            current.Header.Classes.Add("on");
            TabBody.Content = current.Box;
            Refresh(current);
        }

        Sheet CurrentSheet()
        {
            return _sheets.FirstOrDefault(sheet => sheet.Header.Classes.Contains("on"));
        }

        void Refresh(Sheet sheet)
        {
            if (sheet == null) return;
            var query = (FindText.Text ?? "").Trim();
            var rows = Source(sheet).Where(track => query.Length == 0
                || (track.Title ?? "").Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || (track.Info ?? "").Contains(query, StringComparison.CurrentCultureIgnoreCase))
                .Select(track => new TrackRow(track))
                .ToList();
            sheet.Box.ItemsSource = rows;
        }

        List<Audio> Source(Sheet sheet)
        {
            if (sheet.Kind == "library") return _library;
            if (sheet.Kind == "radio")
            {
                return _radios.Select(radio => new Audio
                {
                    Title = radio.Name,
                    Info = radio.Url,
                    Path = radio.Url,
                    IsRadio = true,
                    Duration = "эфир"
                }).ToList();
            }
            var playlist = _playlists.FirstOrDefault(item => item.Name == sheet.Name);
            var tracks = new List<Audio>();
            if (playlist == null) return tracks;
            foreach (var path in playlist.Paths)
            {
                if (File.Exists(path))
                    tracks.Add(LocalLibrary.FromFile(path, string.IsNullOrEmpty(_folder) ? Path.GetDirectoryName(path) : _folder));
            }
            WithDuration(tracks);
            return tracks;
        }

        bool EnsureAudio()
        {
            if (_started) return true;
            try
            {
                _started = _playback.Start();
            }
            catch (Exception ex)
            {
                BeginText.Text = ex.Message;
                return false;
            }
            if (_started) return true;
            BeginText.Text = _playback.LastError();
            return false;
        }

        List<Audio> WithDuration(List<Audio> tracks)
        {
            foreach (var track in tracks)
            {
                if (!track.IsLocal) continue;
                track.Duration = LocalLibrary.FormatDuration(_playback.ProbeSeconds(track.Path));
            }
            return tracks;
        }

        void PlayRow(TrackRow row)
        {
            if (row == null || !EnsureAudio()) return;
            var sheet = CurrentSheet();
            _queue = (sheet.Box.ItemsSource as IEnumerable<TrackRow> ?? Enumerable.Empty<TrackRow>()).Select(item => item.Source).ToList();
            _index = _queue.FindIndex(track => track.Path == row.Source.Path && track.Title == row.Source.Title);
            if (_index < 0) _index = 0;
            StartCurrent();
        }

        void StartCurrent()
        {
            if (_index < 0 || _index >= _queue.Count) return;
            var track = _queue[_index];
            var ok = track.IsRadio ? _playback.PlayUrl(track.Path) : _playback.PlayFile(track.Path);
            if (!ok)
            {
                BeginText.Text = track.Title;
                _playing = false;
                SetPlayIcon(false);
                return;
            }
            _playback.SetVolume((_muted ? 0 : Volume.Value) / 100.0);
            ApplyAllEq();
            _playing = true;
            SetPlayIcon(true);
            BeginText.Text = track.Title;
            SeekSlider.IsEnabled = !track.IsRadio;
            _timer.Start();
        }

        void SetPlayIcon(bool playing)
        {
            PlayImage.Source = Skin(playing ? "pause1.png" : "play1.png");
        }

        void ApplyAllEq()
        {
            if (!OperatingSystem.IsWindows()) return;
            for (var i = 0; i < _eqGains.Length; i++)
                _playback.ApplyEq(i, _eqCenters[i], _eqGains[i]);
        }

        static Bitmap Skin(string file)
        {
            return new Bitmap(AssetLoader.Open(new Uri("avares://MusicPlayer/Image/" + file)));
        }

        static string Clock(double seconds)
        {
            if (double.IsNaN(seconds) || seconds < 0) seconds = 0;
            var time = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:00} : {1:00}", (int)time.TotalMinutes, time.Seconds);
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            var track = _index >= 0 && _index < _queue.Count ? _queue[_index] : null;
            if (track == null) return;
            if (track.IsRadio)
            {
                TextTime.Text = "RADIO";
                return;
            }
            var position = _playback.PositionSeconds();
            var length = _playback.LengthSeconds();
            TextTime.Text = Clock(position);
            if (!_seeking && length > 0)
            {
                SeekSlider.Maximum = length;
                SeekSlider.Value = Math.Min(position, length);
            }
            if (length > 1 && position >= length - 0.4)
            {
                if (_repeat) _playback.Seek(0);
                else Next_Click(null, null);
            }
        }

        void Save()
        {
            LibraryStore.Save(new LibraryFile
            {
                MusicFolder = _folder ?? "",
                Playlists = _playlists,
                Radios = _radios
            });
        }

        async void Folder_Click(object sender, RoutedEventArgs e)
        {
            var picked = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Выберите папку с музыкой",
                AllowMultiple = false
            });
            if (picked == null || picked.Count == 0) return;
            var path = picked[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(path) || !EnsureAudio()) return;
            _folder = path;
            _library.Clear();
            _library.AddRange(WithDuration(LocalLibrary.Scan(_folder)));
            SelectSheet(0);
            Save();
        }

        async void Add_Click(object sender, RoutedEventArgs e)
        {
            var sheet = CurrentSheet();
            if (sheet == null) return;
            if (sheet.Kind == "radio")
            {
                Radio_Click(sender, e);
                return;
            }
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Добавить музыку",
                AllowMultiple = true,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Аудио") { Patterns = new[] { "*.mp3", "*.wav", "*.ogg", "*.flac", "*.m4a", "*.aac", "*.wma", "*.aiff" } }
                }
            });
            if (files == null || files.Count == 0 || !EnsureAudio()) return;
            if (sheet.Kind == "library")
            {
                foreach (var file in files)
                {
                    var path = file.TryGetLocalPath();
                    if (!string.IsNullOrEmpty(path))
                        _library.Add(LocalLibrary.FromFile(path, string.IsNullOrEmpty(_folder) ? Path.GetDirectoryName(path) : _folder));
                }
                WithDuration(_library);
            }
            else
            {
                var playlist = _playlists.FirstOrDefault(item => item.Name == sheet.Name);
                if (playlist == null) return;
                foreach (var file in files)
                {
                    var path = file.TryGetLocalPath();
                    if (!string.IsNullOrEmpty(path) && !playlist.Paths.Contains(path))
                        playlist.Paths.Add(path);
                }
            }
            Refresh(sheet);
            Save();
        }

        void Delete_Click(object sender, RoutedEventArgs e)
        {
            var sheet = CurrentSheet();
            var row = sheet == null ? null : sheet.Box.SelectedItem as TrackRow;
            if (sheet == null || row == null) return;
            if (sheet.Kind == "library")
                _library.RemoveAll(track => track.Path == row.Source.Path);
            else if (sheet.Kind == "radio")
                _radios.RemoveAll(radio => radio.Url == row.Source.Path && radio.Name == row.Source.Title);
            else
            {
                var playlist = _playlists.FirstOrDefault(item => item.Name == sheet.Name);
                if (playlist != null) playlist.Paths.Remove(row.Source.Path);
            }
            Refresh(sheet);
            Save();
        }

        async void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var name = await AskText("Новый плейлист", "Название", "Вечер");
            if (string.IsNullOrWhiteSpace(name) || name == LibraryName || name == RadioName) return;
            name = name.Trim();
            if (_playlists.Any(item => item.Name == name)) return;
            _playlists.Add(new PlaylistFile { Name = name });
            var radio = _sheets.FirstOrDefault(sheet => sheet.Kind == "radio");
            var index = radio == null ? _sheets.Count : _sheets.IndexOf(radio);
            var box = new ListBox();
            box.Classes.Add("tracks");
            box.ItemTemplate = (DataTemplate)Resources["TrackTemplate"];
            box.DoubleTapped += (_, __) => PlayRow(box.SelectedItem as TrackRow);
            var header = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 6 };
            header.Children.Add(new TextBlock { Text = name, Foreground = Brushes.White, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            var button = new Button { Classes = { "tab" }, Content = header };
            var sheet = new Sheet { Name = name, Kind = "playlist", Box = box, Header = button };
            var close = new Button { Classes = { "flat" }, Width = 12, Height = 12, Content = new Image { Source = Skin("close1.png"), Width = 8, Height = 8 } };
            close.Click += (_, __) => RemovePlaylist(sheet);
            header.Children.Add(close);
            button.Click += (_, __) => SelectSheet(_sheets.IndexOf(sheet));
            _sheets.Insert(index, sheet);
            TabStrip.Children.Insert(index, button);
            SelectSheet(index);
            Save();
        }

        void RemovePlaylist(Sheet sheet)
        {
            _playlists.RemoveAll(item => item.Name == sheet.Name);
            var index = _sheets.IndexOf(sheet);
            _sheets.Remove(sheet);
            TabStrip.Children.Remove(sheet.Header);
            SelectSheet(Math.Max(0, index - 1));
            Save();
        }

        async void Radio_Click(object sender, RoutedEventArgs e)
        {
            var station = await AskRadio();
            if (station == null) return;
            _radios.Add(station);
            var index = _sheets.FindIndex(sheet => sheet.Kind == "radio");
            SelectSheet(index);
            Save();
        }

        void PrevSheet_Click(object sender, RoutedEventArgs e)
        {
            var index = _sheets.FindIndex(sheet => sheet.Header.Classes.Contains("on"));
            SelectSheet(index <= 0 ? _sheets.Count - 1 : index - 1);
        }

        void NextSheet_Click(object sender, RoutedEventArgs e)
        {
            var index = _sheets.FindIndex(sheet => sheet.Header.Classes.Contains("on"));
            SelectSheet(index >= _sheets.Count - 1 ? 0 : index + 1);
        }

        void Sort_Click(object sender, RoutedEventArgs e)
        {
            var sheet = CurrentSheet();
            if (sheet == null) return;
            _sorting = (_sorting + 1) % 2;
            Comparison<Audio> compare = (a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase);
            if (_sorting == 0) compare = (a, b) => string.Compare(b.Title, a.Title, StringComparison.CurrentCultureIgnoreCase);
            if (sheet.Kind == "library") _library.Sort(compare);
            else if (sheet.Kind == "radio")
                _radios.Sort((a, b) => _sorting == 1
                    ? string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase)
                    : string.Compare(b.Name, a.Name, StringComparison.CurrentCultureIgnoreCase));
            else
            {
                var tracks = Source(sheet);
                tracks.Sort(compare);
                var playlist = _playlists.FirstOrDefault(item => item.Name == sheet.Name);
                if (playlist != null)
                {
                    playlist.Paths.Clear();
                    playlist.Paths.AddRange(tracks.Select(track => track.Path));
                }
            }
            Refresh(sheet);
            Save();
        }

        void FindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh(CurrentSheet());
        }

        void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!_playing)
            {
                if (_playback.Stream != 0)
                {
                    _playback.Resume();
                    _playing = true;
                    SetPlayIcon(true);
                    _timer.Start();
                    return;
                }
                var sheet = CurrentSheet();
                var row = sheet == null ? null : sheet.Box.SelectedItem as TrackRow
                    ?? (sheet.Box.ItemsSource as IEnumerable<TrackRow>)?.FirstOrDefault();
                PlayRow(row);
                return;
            }
            _playback.Pause();
            _playing = false;
            SetPlayIcon(false);
            _timer.Stop();
        }

        void Stop_Click(object sender, RoutedEventArgs e)
        {
            _playback.Stop();
            _playing = false;
            _timer.Stop();
            SetPlayIcon(false);
            SeekSlider.Value = 0;
            TextTime.Text = "00 : 00";
        }

        void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (_queue.Count == 0) return;
            if (!_queue[_index].IsRadio && _playback.PositionSeconds() > 3)
            {
                _playback.Seek(0);
                return;
            }
            _index = _index <= 0 ? _queue.Count - 1 : _index - 1;
            StartCurrent();
        }

        void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_queue.Count == 0) return;
            if (_random && _queue.Count > 1)
            {
                var next = _index;
                var roll = new Random();
                while (next == _index) next = roll.Next(_queue.Count);
                _index = next;
            }
            else
                _index = _index >= _queue.Count - 1 ? 0 : _index + 1;
            StartCurrent();
        }

        void Position_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_seeking || _index < 0 || _index >= _queue.Count || _queue[_index].IsRadio) return;
            if (Math.Abs(e.NewValue - e.OldValue) < 1.1) return;
            _seeking = true;
            _playback.Seek(SeekSlider.Value);
            _seeking = false;
        }

        void Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_muted) _playback.SetVolume(Volume.Value / 100.0);
        }

        void Repeat_Click(object sender, RoutedEventArgs e)
        {
            _repeat = !_repeat;
            if (_repeat) RepeatButton.Classes.Add("on");
            else RepeatButton.Classes.Remove("on");
        }

        void Random_Click(object sender, RoutedEventArgs e)
        {
            _random = !_random;
            RandomImage.Source = Skin(_random ? "random2.png" : "random1.png");
            if (_random) RandomButton.Classes.Add("on");
            else RandomButton.Classes.Remove("on");
        }

        void Mute_Click(object sender, RoutedEventArgs e)
        {
            _muted = !_muted;
            if (_muted)
            {
                _savedVolume = Volume.Value;
                _playback.SetVolume(0);
                VolumeButton.Classes.Add("on");
            }
            else
            {
                _playback.SetVolume(_savedVolume / 100.0);
                VolumeButton.Classes.Remove("on");
            }
        }

        void PlaylistToggle_Click(object sender, RoutedEventArgs e)
        {
            _playlistOpen = !_playlistOpen;
            PlaylistBar.IsVisible = _playlistOpen;
            LibraryBody.IsVisible = _playlistOpen;
            InfoBar.IsVisible = _playlistOpen;
            FindPanel.IsVisible = _playlistOpen;
            ResizeGrip.IsVisible = _playlistOpen;
            if (!_playlistOpen)
            {
                _openHeight = Height;
                Root.RowDefinitions[1].Height = new GridLength(0);
                Root.RowDefinitions[2].Height = new GridLength(0);
                Root.RowDefinitions[3].Height = new GridLength(0);
                Root.RowDefinitions[4].Height = new GridLength(0);
                Height = 94;
            }
            else
            {
                Root.RowDefinitions[1].Height = new GridLength(47);
                Root.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                Root.RowDefinitions[3].Height = new GridLength(23);
                Root.RowDefinitions[4].Height = new GridLength(36);
                Height = _openHeight < 200 ? 484 : _openHeight;
            }
        }

        void Chrome_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            var visual = e.Source as Visual;
            while (visual != null && visual != this)
            {
                if (visual is Button || visual is Slider || visual is Thumb) return;
                visual = visual.GetVisualParent();
            }
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }

        void Resize_Drag(object sender, VectorEventArgs e)
        {
            if (!_playlistOpen) return;
            Height = Math.Max(200, Height + e.Vector.Y);
            _openHeight = Height;
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void Pin_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            PinImage.Source = Skin(Topmost ? "button2.png" : "button1.png");
        }

        void Equalizer_Click(object sender, RoutedEventArgs e)
        {
            if (_eqWindow != null)
            {
                _eqWindow.Activate();
                return;
            }
            var row = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 6,
                Margin = new Thickness(8, 10, 8, 0),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            for (var i = 0; i < _eqGains.Length; i++)
            {
                var band = i;
                var slider = new Slider
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    Minimum = -15,
                    Maximum = 15,
                    Value = _eqGains[band],
                    Height = 70,
                    Width = 16
                };
                slider.ValueChanged += (_, args) =>
                {
                    _eqGains[band] = args.NewValue;
                    _playback.ApplyEq(band, _eqCenters[band], args.NewValue);
                };
                row.Children.Add(slider);
            }
            _eqWindow = new Window
            {
                Width = 230,
                Height = 94,
                WindowDecorations = WindowDecorations.None,
                Background = new SolidColorBrush(Color.Parse("#BF3517")),
                Content = new Border { Background = Brushes.Black, Child = row },
                WindowStartupLocation = WindowStartupLocation.Manual,
                Position = new PixelPoint(Position.X + 360, Position.Y)
            };
            _eqWindow.PointerPressed += Chrome_PointerPressed;
            _eqWindow.Closed += (_, __) => _eqWindow = null;
            _eqWindow.Show(this);
        }

        async System.Threading.Tasks.Task<string> AskText(string title, string label, string initial)
        {
            var box = new TextBox { Text = initial, Margin = new Thickness(12), Foreground = Brushes.White };
            var ok = new Button { Content = "Сохранить", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Margin = new Thickness(12) };
            var panel = new StackPanel { Background = Brushes.Black };
            panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(12, 12, 12, 0), Foreground = Brushes.White });
            panel.Children.Add(box);
            panel.Children.Add(ok);
            var dialog = new Window
            {
                Title = title,
                Width = 320,
                Height = 140,
                Content = panel,
                Background = Brushes.Black,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            string result = null;
            ok.Click += (_, __) => { result = box.Text; dialog.Close(); };
            await dialog.ShowDialog(this);
            return result;
        }

        async System.Threading.Tasks.Task<RadioFile> AskRadio()
        {
            var name = new TextBox { Margin = new Thickness(12, 4, 12, 0), Text = "Станция", Foreground = Brushes.White };
            var url = new TextBox { Margin = new Thickness(12, 4, 12, 0), Text = "https://", Foreground = Brushes.White };
            var ok = new Button { Content = "Добавить", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Margin = new Thickness(12) };
            var panel = new StackPanel { Background = Brushes.Black };
            panel.Children.Add(new TextBlock { Text = "Название", Margin = new Thickness(12, 12, 12, 0), Foreground = Brushes.White });
            panel.Children.Add(name);
            panel.Children.Add(new TextBlock { Text = "Адрес потока", Margin = new Thickness(12, 8, 12, 0), Foreground = Brushes.White });
            panel.Children.Add(url);
            panel.Children.Add(ok);
            var dialog = new Window
            {
                Title = "Интернет-радио",
                Width = 360,
                Height = 210,
                Content = panel,
                Background = Brushes.Black,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            RadioFile station = null;
            ok.Click += (_, __) =>
            {
                Uri parsed;
                var title = (name.Text ?? "").Trim();
                var address = (url.Text ?? "").Trim();
                if (title.Length > 0 && Uri.TryCreate(address, UriKind.Absolute, out parsed)
                    && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
                {
                    station = new RadioFile { Name = title, Url = address };
                    dialog.Close();
                }
            };
            await dialog.ShowDialog(this);
            return station;
        }

        sealed class Sheet
        {
            public string Name;
            public string Kind;
            public ListBox Box;
            public Button Header;
        }
    }

    public class TrackRow
    {
        public TrackRow(Audio source)
        {
            Source = source;
        }

        public Audio Source { get; private set; }
        public string Title { get { return Source.Title; } }
        public string Info { get { return Source.Info; } }
        public string Duration { get { return Source.Duration; } }
    }
}
