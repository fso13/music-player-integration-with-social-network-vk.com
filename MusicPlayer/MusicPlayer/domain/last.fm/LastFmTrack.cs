using System.Xml;

namespace VKAudioPlayer.domain
{
    class LastFmTrack
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }

        public LastFmTrack(XmlNode xml)
        {
            LoadXml(xml);
        }

        public void LoadXml(XmlNode source)
        {
            var xmlElement = source["artist"];
            if (xmlElement != null) Artist = xmlElement.InnerText;
            var element = source["name"];
            if (element != null) Title = element.InnerText;
            Image = null;
            if (source["image"] == null) return;
            Image = source["image"].InnerText;
            var x = source["image"].NextSibling;
            if (x != null && x.Name != "image") return;
            if (x != null && x.InnerText == "") return;
            if (x == null) return;
            Image = x.InnerText;
            var xx = x.NextSibling;
            if (xx != null && xx.Name != "image") return;
            if (xx == null || xx.InnerText == "") return;
            Image = xx.InnerText;
            var xxx = xx.NextSibling;

            if (xxx == null || xxx.Name != "image") return;
            if (xxx.InnerText != "")
                Image = xxx.InnerText;
        }
    }
}
