using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VKAudioPlayer.domain;

namespace MusicPlayer.local
{
    class LocalAudio : Audio
    {
        private string Artist { get; set; }
        private string Title { get; set; }
    }
}
