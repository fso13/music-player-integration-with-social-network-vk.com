namespace MusicPlayer.domain
{
    public class Audio
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Info { get; set; }
        public bool IsPlayed { get; set; }
        public string Path { get; set; }
        public bool IsLocal { get; set; }
        public bool IsRadio { get; set; }

        public Audio()
        {
            IsPlayed = true;
            Duration = "--:--";
            Info = "";
        }
    }
}
