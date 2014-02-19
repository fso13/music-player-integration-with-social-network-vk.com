using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VKAudioPlayer.domain;

namespace MusicPlayer.domain
{
    class PlayList
    {
        private string Name { get; set; }
        private List<Audio> ListAudio = new List<Audio>();

        public List<Audio> GetListAudio()
        {
            return ListAudio;
        }
    }
}
