using MusicPlayer.local;
using System.Windows;

namespace VKAudioPlayer.domain
{
	public class Audio
	{

        public string Title { get; set; }
        public string Duration { get; set; }
        public string Info { get; set; }
        public bool IsPlayed { get; set; }
        public string Path { get; set; }
        public string ID { get; set; }
        public string OwnerID { get; set; }
        public bool IsLocal { get; set; }

         public Audio(VkAudio audio)
         {
             Title = audio.Title + " - " + audio.Artist;
             Duration = ((audio.Duration / 60) % 60).ToString("00") + ":" + (audio.Duration % 60).ToString("00");
             Info = "";
             IsPlayed = true;
             IsLocal = false;
             Path = audio.Url;
             OwnerID = audio.OwnerId;
             ID = audio.Id;
         }
	}
}