using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures
{
    public enum LibrarySelection
    {
        // Program Specific
        None,
        All,
        APISupported,

        // API Supported
        CurrentlyWatching,
        PlanToWatch,
        Completed,
        OnHold,
        Dropped,

        // Extra
        Favourites,
        Recent
    }
}
