using System;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;

namespace VKAudioPlayer.domain
{
    class LastFmHelper
    {
        private const string ApiKey = "da0997160b0bd280c7fa83e132312321";

        private static XmlDocument ExecuteCommand(string name, NameValueCollection qs)
        {
            try
            {
                var result = new XmlDocument();
                result.Load(String.Format("http://ws.audioscrobbler.com/2.0/?method={0}&{2}&api_key={1}", name, ApiKey, String.Join("&", from item in qs.AllKeys select item + "=" + qs[item])));
                return result;
            }
            catch (Exception e)
            {
                e.GetType();
                return null;
            }
        }

        public LastFmTrack TrackSearch(string name,string artist)
        {
            LastFmTrack track = null;

            var qs = new NameValueCollection();
            qs["track"] = name;
            qs["artist"] = artist;
            qs["limit"] = "1";
            var result = ExecuteCommand("track.search", qs);
            if (result == null) return null;
            if (result.DocumentElement != null)
            {
                var node = result.DocumentElement.ChildNodes;
                if (node[0] != null && node[0].ChildNodes[4] != null && node[0].ChildNodes[4].ChildNodes[0] != null)
                    track = new LastFmTrack(node[0].ChildNodes[4].ChildNodes[0]);
                else
                {
                    qs = new NameValueCollection();
                    qs["track"] = name;
                    result = ExecuteCommand("track.search", qs);
                    if (result.DocumentElement != null) node = result.DocumentElement.ChildNodes;
                    if (node[0] != null && node[0].ChildNodes[4] != null && node[0].ChildNodes[4].ChildNodes[0] != null)
                        track = new LastFmTrack(node[0].ChildNodes[4].ChildNodes[0]);
                }
            }
            return track;
        }
    }
}
