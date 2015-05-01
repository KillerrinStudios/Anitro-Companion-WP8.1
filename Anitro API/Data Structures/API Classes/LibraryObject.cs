using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures.API_Classes
{
    public class LibraryObject
    {
        public string episodes_watched { get; set; } // int
        public string last_watched { get; set; }
        public string rewatched_times { get; set; } // int
        public object notes { get; set; } //string
        public object notes_present { get; set; } //bool
        public string status { get; set; }
        public string id { get; set; }
        public bool @private { get; set; }
        public object rewatching { get; set; } //bool
        public Anime anime { get; set; }
        public Rating rating { get; set; }
    }
}
