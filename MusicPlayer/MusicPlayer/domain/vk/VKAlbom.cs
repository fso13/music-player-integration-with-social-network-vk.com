using System.Xml;

namespace VKAudioPlayer.domain
{
    class VkAlbom
    {
        public string OwnerId { get; set; }
        public string AlbumId { get; set; }
        public string Title { get; set; }

        public VkAlbom(XmlNode xml)
        {
            LoadXml(xml);
        }

        public void LoadXml(XmlNode source)
        {
            var xmlElement = source["owner_id"];
            if (xmlElement != null) OwnerId = xmlElement.InnerText;
            xmlElement = source["album_id"];
            if (xmlElement != null) AlbumId = xmlElement.InnerText;
            xmlElement = source["title"];
            if (xmlElement != null) Title = xmlElement.InnerText;
        }
    }
}
