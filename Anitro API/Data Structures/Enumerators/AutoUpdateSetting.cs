using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anitro_API.Data_Structures.Enumerators
{
    public enum AutoUpdateSetting
    {
        Off                 = 0,
        EveryActivation     = 1,
        EveryHour           = 2,
        EveryFourHours      = 3,
        OnceADay            = 4,
        TwiceADay           = 5,
        EveryOtherDay       = 6
    }
}
