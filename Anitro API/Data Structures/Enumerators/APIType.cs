using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures
{
    public enum APIType
    {
        None,

        // One-Off
        Search,
        Login,
        UserInfo,

        // Anime
        GetAnime,
        GetFavourites,

        // Library
        LoadLibrary,
        LibraryUpdate,

        GetLibrary,
        PostLibraryUpdate,
        PostLibraryRemove,

        // Activity Feed
        GetActivityFeed,
        PostActivityFeed
    }
}
