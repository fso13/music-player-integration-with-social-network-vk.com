using System;
using System.Xml;

namespace VKAudioPlayer.domain
{
    public class VkAudio
    {
        private string Artist { get; set; }
        private string Title { get; set; }
        private string Url { get; set; }
        private string Id { get; set; }
        private string AlbomId { get; set; }
        private string OwnerId { get; set; }
        private int Duration { get; set; }

        public VkAudio()
        {
        }

        public VkAudio(XmlNode xml)
        {
            LoadXml(xml);
        }
        public void LoadXml(XmlNode source)
        {
            var xmlElement = source["artist"];
            if (xmlElement != null) Artist = xmlElement.InnerText;
            xmlElement = source["title"];
            if (xmlElement != null) Title = xmlElement.InnerText;
            xmlElement = source["url"];
            if (xmlElement != null) Url = xmlElement.InnerText;
            xmlElement = source["aid"];
            if (xmlElement != null) Id = xmlElement.InnerText;
            xmlElement = source["owner_id"];
            if (xmlElement != null) OwnerId = xmlElement.InnerText;
            xmlElement = source["duration"];
            if (xmlElement != null) Duration = Convert.ToInt32(xmlElement.InnerText);
            AlbomId = source["album"]!=null ? source["album"].InnerText : "";
        }
    }
}
