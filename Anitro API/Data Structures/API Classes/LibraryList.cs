using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures.API_Classes
{
    public class LibraryList
    {
        public List<LibraryObject> library { get; set; }
        public List<LibraryObject> favourites { get; set; }
        public List<LibraryObject> recent { get; set; }
    }
}
