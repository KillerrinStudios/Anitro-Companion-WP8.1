using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Diagnostics;

using Anitro.Data_Structures;
using System.Threading.Tasks;

namespace Anitro
{
    public static class Consts
    {
        // Testing Code
        public static bool DebugMode = Debugger.IsAttached;

        public static string testAccountUsername = "killerrin";
        public static string testAccountPassword = "";

        // Application Information
        public static KillerrinApplicationData appData = new KillerrinApplicationData();

        // Logged-In Settings
        public static User LoggedInUser = new User();
        public static Settings AppSettings = new Settings();

        // Persisting Application Variables
        public static bool HasAccessForLockscreen = false;
        public static bool isApplicationClosing = false;
        public static bool justSignedIn = false;
        public static bool forceLibrarySave = false;
        public static bool forceLibraryRemoveal = false;
        
        public static bool openedFromProtocolOrTile = false;
        public static bool uriAssociationHandled = false;

        #region Helper Methods
        public static async Task<bool> LaunchReview()
        {

            bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid="+Consts.appData.AnitroPackageName));

            return result;
        }

        public static bool IsConnectedToInternet() { return NetworkInterface.GetIsNetworkAvailable(); }
        #endregion
    }
}
