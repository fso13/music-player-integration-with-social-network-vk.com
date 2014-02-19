using MusicPlayer.local;
using System.Windows;

namespace VKAudioPlayer.domain
{
	public class Audio
	{
		public Audio(LocalAudio audio)
		{
            Artist = audio.Artist;
            Title = audio.Title;	
		}
		
		public Audio(VkAudio audio)
		{
            Artist = audio.Artist;
            Title = audio.Title;
		}
		
		 public string Artist{get; set;}
		 public string Title{get; set;}
	}
}