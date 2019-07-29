using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using scribercontext.Model;

namespace scribercontext.Helper
{
    public class YouTubeHelper
    {

        public static String GetVideoIdFromURL(String videoURL)
        {
            int indexOfFirstId = videoURL.IndexOf("=") + 1;
            String videoId = videoURL.Substring(indexOfFirstId);
            return videoId;
        }

        public static Video GetVideoInfo(String videoId)
        {
            String APIKey = "AIzaSyDZUO9JXXlHIASlW_SGBXLHN1ME6mmfmUg";
            String YoutubeAPIURL = "https://www.googleapis.com/youtube/v3/videos?id="+ videoId +"&key="+ APIKey +"&part=snippet,contentDetails";
            String videoInfoJSON = new WebClient().DownloadString(YoutubeAPIURL);
            dynamic jsonObj = JsonConvert.DeserializeObject<dynamic>(videoInfoJSON);

            String title = jsonObj["items"][0]["snippet"]["title"];
            String thumbnailURL = jsonObj["items"][0]["snippet"]["thumbnails"]["medium"]["url"];
            String durationString = jsonObj["items"][0]["contentDetails"]["duration"];
            String videoURL = "https://www.youtube.com/watch?v=" + videoId;

            TimeSpan videoDuration = XmlConvert.ToTimeSpan(durationString);
            int duration = (int)videoDuration.TotalSeconds;

            Video video = new Video
            {
                VideoTitle = title,
                WebUrl = videoURL,
                VideoLength = duration,
                IsFavourite = false,
                ThunbnailUrl = thumbnailURL
            };
            return video;
        }

        private static String GetTranscriptionLink(String videoId)
        {
            String YouTubeVideoURL = "https://www.youtube.com/watch?v=" + videoId;
            String HTMLSource = new WebClient().DownloadString(YouTubeVideoURL);

            String pattern = "timedtext.+?lang=";
            Match match = Regex.Match(HTMLSource, pattern);
            if (match.ToString() != "")
            {
                String subtitleLink = "https://www.youtube.com/api/" + match + "en";
                subtitleLink = CleanLink(subtitleLink);
                return subtitleLink;
            }
            else
            {
                return null;
            }
        }

        public static List<Transcription> GetTranscriptions(String videoId)
        {
            String subtitleLink = GetTranscriptionLink(videoId);

            XmlDocument doc = new XmlDocument();
            doc.Load(subtitleLink);
            XmlNode root = doc.ChildNodes[1];

            List<Transcription> transcriptions = new List<Transcription>();
            if(root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    String phrase = root.ChildNodes[i].InnerText;
                    phrase = HttpUtility.HtmlDecode(phrase);
                    Transcription transcription = new Transcription
                    {
                        StartTime = (int)Convert.ToDouble(root.ChildNodes[i].Attributes["start"].Value),
                        Phrase = phrase
                    };
                    transcription.Add(transcription);
                }
            }
            return transcriptions;
        }

        private static string CleanLink(string subtitleURL)
        {
            subtitleURL = subtitleURL.Replace("\\\\u0026", "&");
            subtitleURL = subtitleURL.Replace("\\", "");
            return (subtitleURL);
        }
    }
}
