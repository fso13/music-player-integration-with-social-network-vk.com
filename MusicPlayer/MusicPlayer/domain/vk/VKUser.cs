using System;
using System.Xml;

namespace VKAudioPlayer.domain
{
    public class VkUser
    {
        public string Uid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Photo50 { get; set; }
        public string Photo100 { get; set; }
        public string Photo { get; set; }
        public string Status { get; set; }

        public int Online { get; set; }

        public VkUser(XmlNode xml)
        {
            LoadXml(xml);
        }

        public void LoadXml(XmlNode source)
        {
            var xmlElement = source["uid"];
            if (xmlElement != null) Uid = xmlElement.InnerText;
            xmlElement = source["first_name"];
            if (xmlElement != null) FirstName = xmlElement.InnerText;
            xmlElement = source["last_name"];
            if (xmlElement != null) LastName = xmlElement.InnerText;
            xmlElement = source["photo_50"];
            if (xmlElement != null) Photo50 = xmlElement.InnerText;
            xmlElement = source["photo_100"];
            if (xmlElement != null) Photo100 = xmlElement.InnerText;
            Photo = !string.IsNullOrEmpty(Photo100) ? Photo100 : Photo50;
            if (source["status"] != null)
                Status = source["status"].InnerText;
            xmlElement = source["online"];
            if (xmlElement != null) Online = Convert.ToInt32(xmlElement.InnerText);
        }
    }
}
