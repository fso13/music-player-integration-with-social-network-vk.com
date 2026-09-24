using System.Collections.Generic;

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
