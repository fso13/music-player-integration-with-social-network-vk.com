using System;
using System.Collections.Generic;
using System.IO;
using MusicPlayer.domain;

namespace MusicPlayer.local
{
    public static class LocalLibrary
    {
        private static readonly HashSet<string> Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aac", ".wma", ".aiff", ".aif", ".mp2"
        };

        public static List<Audio> Scan(string folder)
        {
            var list = new List<Audio>();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return list;

            foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                if (!Extensions.Contains(Path.GetExtension(file))) continue;
                list.Add(FromFile(file, folder));
            }

            list.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        public static Audio FromFile(string file, string root)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var artist = "";
            var title = name;
            var parts = name.Split(new[] { " - " }, 2, StringSplitOptions.None);
            if (parts.Length == 2 && parts[0].Trim().Length > 0 && parts[1].Trim().Length > 0)
            {
                artist = parts[0].Trim();
                title = parts[1].Trim();
            }

            var parent = Path.GetDirectoryName(file);
            var info = "";
            if (!string.IsNullOrEmpty(parent) && !string.Equals(Path.GetFullPath(parent), Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
                info = Path.GetFileName(parent);
            if (artist.Length > 0)
                info = info.Length > 0 ? artist + " · " + info : artist;

            return new Audio
            {
                Title = title,
                Info = info,
                Path = file,
                IsLocal = true,
                IsRadio = false,
                IsPlayed = true,
                Duration = "--:--"
            };
        }

        public static string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) return "--:--";
            var time = TimeSpan.FromSeconds(seconds);
            if (time.TotalHours >= 1)
                return string.Format("{0}:{1:00}:{2:00}", (int)time.TotalHours, time.Minutes, time.Seconds);
            return string.Format("{0:00}:{1:00}", (int)time.TotalMinutes, time.Seconds);
        }
    }
}
