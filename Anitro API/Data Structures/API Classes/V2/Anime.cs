using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures.API_Classes.V2
{
    public class Anime
    {
        public string id { get; set; }
        public string canonical_title { get; set; }
        public object english_title { get; set; }
        public string romaji_title { get; set; }
        public string synopsis { get; set; }
        public string poster_image { get; set; }
        public List<string> genres { get; set; }
        public string type { get; set; }
        public string started_airing { get; set; }
        public string finished_airing { get; set; }
        public List<string> screencaps { get; set; }
        public string youtube_trailer_id { get; set; }
        public double community_rating { get; set; }
    }
}
