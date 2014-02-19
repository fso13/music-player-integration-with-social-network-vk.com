using MusicPlayer.local;
using System.Windows;

namespace VKAudioPlayer.domain
{
	public class Audio
	{
			
		 private string Artist{get; set;}
         private string Title { get; set; }
         private string Number { get; set; }
         private string Info { get; set; }
         private bool IsPlayed { get; set; }
         private string Duration { get; set; }
		
		public Audio(string artist, string title, string number, string info, bool isPlayed, string duration)
		{
			Artist = artist;
			Title =title;
			Number = number;
			Info = info;
			IsPlayed = isPlayed;
			Duration=duration;
		}
		
	}
}