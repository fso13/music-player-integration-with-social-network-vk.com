using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VKAudioPlayer.domain;

namespace MusicPlayer.domain
{
    class PlayList
    {
        public string Name { get; set; }
        public List<Audio> ListAudio = new List<Audio>();

        public PlayList(string name)
        {
            Name = name;
        }
        public List<Audio> GetListAudio()
        {
            return ListAudio;
        }
    }
}
