using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures.API_Classes
{
    public class Anime
    {
        public string slug { get; set; }
        public string status { get; set; }
        public string url { get; set; }
        public string title { get; set; }

        private string _altTitle;
        public string alternate_title 
        {
            get { return _altTitle; }
            set 
            {
                if (value != null)
                    _altTitle = value;
            }
        } //string

        private string episodeCountFixed;
        public string episode_count {
            get { return episodeCountFixed; }
            set
            {
                if (value == "0") { episodeCountFixed = "?"; }
                else { episodeCountFixed = value; }
            }
        } // int

        public Uri cover_image_uri;
        private string _coverimage;
        public string cover_image 
        {
            get { return _coverimage; }
            set 
            {
                if (value != null)
                {
                    _coverimage = value;
                    cover_image_uri = new Uri(_coverimage, UriKind.Absolute);
                }
            }
        }

        public string synopsis { get; set; }
        public string show_type { get; set; }
        public int fav_rank { get; set; }
        public int fav_id { get; set; }
        public List<Genre> genres { get; set; }
    }

    public class AnimeList
    {
        public List<Anime> anime { get; set; }
    }
}
