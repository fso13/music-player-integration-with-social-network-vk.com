using System.Xml;

namespace VKAudioPlayer.domain
{
    public class VkGroup
    {
        public string Gid { get; set; }
        public string Name { get; set; }

        public VkGroup(XmlNode xml)
        {
            LoadXml(xml);
        }

        public void LoadXml(XmlNode source)
        {
            var xmlElement = source["gid"];
            if (xmlElement != null) Gid = "-"+xmlElement.InnerText;
            xmlElement = source["name"];
            if (xmlElement != null) Name = xmlElement.InnerText;
        }
    }
}
