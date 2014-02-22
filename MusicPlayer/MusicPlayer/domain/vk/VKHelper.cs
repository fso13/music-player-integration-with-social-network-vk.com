using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Xml;
using VKAudioPlayer.domain;

namespace MusicPlayer.domain.vk
{
    internal class VkHelper
    {
        public string AccessToken = "";

        private XmlDocument ExecuteCommand(string name, NameValueCollection qs)
        {
            string param = String.Empty;

            for (int i = 0; i < qs.Count; i++)
            {
                param += "&" + qs.Keys[i] + "=" + qs[i];
            }
            var result = new XmlDocument();
            result.Load(String.Format("https://api.vkontakte.ru/method/{0}.xml?access_token={1}{2}", name, AccessToken, param));
            return result;
        }

        public List<VkAlbom> GetAlbom()
        {
            var qs = new NameValueCollection();
            var result = ExecuteCommand("audio.getAlbums", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            var list = new List<VkAlbom>();
            if (node[0].Name.Equals("error_code")) return list;
            for (var i = 1; i < node.Count; i++)
            {
                list.Add(new VkAlbom(node[i]));
            }
            return list;
        }

        public List<VkAudio> GetAudio(string ownerId, string albumId)
        {
            var qs = new NameValueCollection();
            qs["count"] = "6000";

            qs["owner_id"] = ownerId;
            if (albumId != "")
                qs["album_id"] = albumId;

            var result = ExecuteCommand("audio.get", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            var list = new List<VkAudio>();
            if (node.Count <= 0) return list;
            if (node[0].Name.Equals("error_code")) return list;
            for (var i = 1; i < node.Count; i++)
            {
                list.Add(new VkAudio(node[i]));
            }
            return list;
        }

        public List<VkAudio> GetRecommendations()
        {
            var qs = new NameValueCollection();
            qs["count"] = "6000";
            var result = ExecuteCommand("audio.getRecommendations", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            var list = new List<VkAudio>();
            if (node.Count <= 0) return list;
            if (node[0].Name.Equals("error_code")) return list;
            for (var i = 0; i < node.Count; i++)
            {
                list.Add(new VkAudio(node[i]));
            }
            return list;
        }

        public List<VkAudio> GetPopular(string genreId )
        {
            var qs = new NameValueCollection();
            qs["count"] = "6000";
            qs["genre_id"] = genreId;
            var result = ExecuteCommand("audio.getPopular", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            var list = new List<VkAudio>();
            if (node.Count <= 0) return list;
            if (node[0].Name.Equals("error_code")) return list;
            for (var i = 0; i < node.Count; i++)
            {
                list.Add(new VkAudio(node[i]));
            }
            return list;
        }

        public List<VkAudio> FindAudio(string find)
        {
            var qs = new NameValueCollection();
            qs["count"] = "6000";
            qs["q"] = find;
            var result = ExecuteCommand("audio.search", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            if (node[0].InnerText.Equals("0"))
            {
                MessageBox.Show("Не найдено.");
                return null;
            }
            var list = new List<VkAudio>();
            for (var i = 1; i < node.Count; i++)
            {
                list.Add(new VkAudio(node[i]));
            }
            return list;
        }

        public List<VkGroup> GetGroups(string userId)
        {
            var qs = new NameValueCollection();
            qs["user_id"] = userId;
            qs["extended"] = "1";
            var result = ExecuteCommand("groups.get", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            if (node[0].InnerText.Equals("0"))
            {
                MessageBox.Show("Не найдено.");
                return null;
            }
            var list = new List<VkGroup>();
            for (var i = 1; i < node.Count; i++)
            {
                list.Add(new VkGroup(node[i]));
            }
            return list;
        }

        public VkUser GetUser()
        {
            var qs = new NameValueCollection();
            qs["fields"] = "photo_50,photo_100,status,online";
            var result = ExecuteCommand("users.get", qs);
            if (result.DocumentElement == null) return null;
            var node = result.DocumentElement.ChildNodes;
            if (!node[0].Name.Equals("error_code")) return new VkUser(node[0]);
            MessageBox.Show("Ошибка авторизации.");
            return null;
        }

        public List<VkUser> GetFriends(string userId)
        {
            var listFriends = new List<VkUser>();
            var qs = new NameValueCollection();
            qs["user_id"] = userId;
            qs["order"] = "name";
            qs["fields"] =
                "photo_50,status,online,nickname,domain,sex,bdate,city,country,timezone,photo_50,photo_100,photo_200_orig,has_mobile,contacts,education,online,relation,last_seen,status,can_write_private_message,can_see_all_posts,can_post,universities";

            var result = ExecuteCommand("friends.get", qs);
            if (result.DocumentElement == null) return listFriends;
            var node = result.DocumentElement.ChildNodes;
            for (var i = 0; i < node.Count; i++)
            {
                listFriends.Add(new VkUser(node[i]));
            }
            return listFriends;
        }

        public void AddAudio(string ownerId, string audioId)
        {
            var qs = new NameValueCollection();
            qs["owner_id"] = ownerId;
            qs["audio_id"] = audioId;
            ExecuteCommand("audio.add", qs);
        }

        public void SetStatus(string text)
        {
            var qs = new NameValueCollection();
            qs["text"] = text;
            ExecuteCommand("status.set", qs);
        }

        public string GetStatus()
        {
            var qs = new NameValueCollection();
            var result = ExecuteCommand("status.get", qs);
            if (result.DocumentElement == null) return "";
            var node = result.DocumentElement.ChildNodes;
            return node[0].InnerText;
        }

        public int GetCount(string ownerId)
        {
            var qs = new NameValueCollection();
            qs["owner_id"] = ownerId;
            var result = ExecuteCommand("audio.getCount", qs);
            if (result.DocumentElement == null) return 0;
            var node = result.DocumentElement.ChildNodes;
            return Convert.ToInt32(node[0].InnerText);
        }

        public void SetOnline()
        {
            var qs = new NameValueCollection();
            ExecuteCommand("account.setOnline", qs);
        }
    }
}
