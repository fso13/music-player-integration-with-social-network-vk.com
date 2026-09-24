using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MusicPlayer.domain
{
    public class LibraryFile
    {
        public string MusicFolder { get; set; }
        public List<PlaylistFile> Playlists { get; set; }
        public List<RadioFile> Radios { get; set; }

        public LibraryFile()
        {
            MusicFolder = "";
            Playlists = new List<PlaylistFile>();
            Radios = new List<RadioFile>();
        }
    }

    public class PlaylistFile
    {
        public string Name { get; set; }
        public List<string> Paths { get; set; }

        public PlaylistFile()
        {
            Name = "";
            Paths = new List<string>();
        }
    }

    public class RadioFile
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public RadioFile()
        {
            Name = "";
            Url = "";
        }
    }

    public static class LibraryStore
    {
        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library.xml");

        public static LibraryFile Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new LibraryFile();
                var serializer = new XmlSerializer(typeof(LibraryFile));
                using (var stream = File.OpenRead(FilePath))
                {
                    return (LibraryFile)serializer.Deserialize(stream) ?? new LibraryFile();
                }
            }
            catch (Exception)
            {
                return new LibraryFile();
            }
        }

        public static void Save(LibraryFile library)
        {
            var serializer = new XmlSerializer(typeof(LibraryFile));
            using (var stream = File.Create(FilePath))
            {
                serializer.Serialize(stream, library);
            }
        }
    }
}
