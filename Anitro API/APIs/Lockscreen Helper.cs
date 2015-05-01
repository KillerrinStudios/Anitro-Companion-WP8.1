using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Navigation;
using Windows.Phone.System.UserProfile;
using Anitro.APIs;
using Anitro.Data_Structures.API_Classes;
using Anitro;
using Anitro.Data_Structures;

namespace Anitro.APIs
{
    public static class Lockscreen_Helper
    {
        public static string BackgroundRoot = ""; 
        public static string LOCKSCREEN_IMAGE = "lockscreen.jpg";
        public static string LOCKSCREEN_IMAGE_1 = "lockscreen1.jpg";

        public static Uri DefaultLockscreenImage
        {
            get
            {
                bool isAppResource = true;
                string filePathOfTheImage = "Assets/defaultLockscreenBackground.jpg";
                var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                var uri = new Uri("ms-appx:///" + filePathOfTheImage, UriKind.Absolute);
                return uri;
            }
        }

        public static Uri DefaultLockscreenImage2
        {
            get
            {
                bool isAppResource = true;
                string filePathOfTheImage = "Assets/defaultLockscreenBackground2.jpg";
                var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                var uri = new Uri("ms-appx:///" + filePathOfTheImage, UriKind.Absolute);
                return uri;
            }
        }

        public static int count = -1;
        public static Random random = new Random();

        public static bool DeleteLockscreenImage()
        {
            StorageTools.DeleteFile(LOCKSCREEN_IMAGE);
            StorageTools.DeleteFile(LOCKSCREEN_IMAGE_1);
            return true;
        }

        public static async Task<Uri> SetRandomImageFromLibrary(bool setToLockscreen = true)
        {
            if (!LockScreenManager.IsProvidedByCurrentApplication) { return DefaultLockscreenImage; }
            if ((setToLockscreen &&
                 !StorageTools.Settings.Lockscreen.randomizeCurrentlyWatching.Value &&
                 !StorageTools.Settings.Lockscreen.randomizePlanToWatch.Value &&
                 !StorageTools.Settings.Lockscreen.randomizeCompleted.Value &&
                 !StorageTools.Settings.Lockscreen.randomizeOnHold.Value &&
                 !StorageTools.Settings.Lockscreen.randomizeDropped.Value &&
                 !StorageTools.Settings.Lockscreen.randomizeFavourites.Value)) 
            { 
                Debug.WriteLine("All Libraries false, setting to default and exiting early");
                await SetLockScreen(0);
                return DefaultLockscreenImage; 
            }

            // Attempt to sign in
            if (!Consts.LoggedInUser.IsLoggedIn)
                Consts.LoggedInUser = await Anitro.Data_Structures.User.Load();

            // Still not signed in
            if (!Consts.LoggedInUser.IsLoggedIn && setToLockscreen)
            {
                Debug.WriteLine("Still not signed in, setting to default and exiting early");
                await SetLockScreen(0);
                return DefaultLockscreenImage;
            }
            //===========================================================================================================\\
            if (!Consts.LoggedInUser.animeLibrary.IsEveryLibraryEmpty(Anitro.Data_Structures.LibrarySelection.All)) 
            {
                if (StorageTools.Settings.Lockscreen.randomizeCurrentlyWatching.Value ||
                    StorageTools.Settings.Lockscreen.randomizePlanToWatch.Value ||
                    StorageTools.Settings.Lockscreen.randomizeCompleted.Value ||
                    StorageTools.Settings.Lockscreen.randomizeOnHold.Value ||
                    StorageTools.Settings.Lockscreen.randomizeDropped.Value ||
                    StorageTools.Settings.Lockscreen.randomizeFavourites.Value)
                {
                    // Maybe decide to refresh the library here
                    //Consts.LoggedInUser = await Anitro.Data_Structures.User.Load();
                }
            }

            List<LibraryObject> tempList = new List<LibraryObject>();
            if (StorageTools.Settings.Lockscreen.randomizeCurrentlyWatching.Value)
            {
                foreach (LibraryObject lO in Consts.LoggedInUser.animeLibrary.CurrentlyWatching)
                {
                    tempList.Add(lO);
                }
            }
            if (StorageTools.Settings.Lockscreen.randomizePlanToWatch.Value)
            {
                foreach (LibraryObject lO in Consts.LoggedInUser.animeLibrary.PlanToWatch)
                {
                    tempList.Add(lO);
                }
            }
            if (StorageTools.Settings.Lockscreen.randomizeCompleted.Value)
            {
                foreach (LibraryObject lO in Consts.LoggedInUser.animeLibrary.Completed)
                {
                    tempList.Add(lO);
                }
            }
            if (StorageTools.Settings.Lockscreen.randomizeOnHold.Value)
            {
                foreach (LibraryObject lO in Consts.LoggedInUser.animeLibrary.OnHold)
                {
                    tempList.Add(lO);
                }
            }
            if (StorageTools.Settings.Lockscreen.randomizeDropped.Value)
            {
                foreach (LibraryObject lO in Consts.LoggedInUser.animeLibrary.Dropped)
                {
                    tempList.Add(lO);
                }
            }
            if (StorageTools.Settings.Lockscreen.randomizeFavourites.Value)
            {
                //if (!Consts.LoggedInUser.animeLibrary.IsEveryLibraryEmpty(LibrarySelection.Favourites) { await Storage.LoadAnimeLibrary("favourites"); }
                foreach (LibraryObject lO in Consts.LoggedInUser.animeLibrary.Favourites)
                {
                    tempList.Add(lO);
                }
            }

            if (tempList == null || tempList.Count == 0) { Debug.WriteLine("List is empty. Returning."); return DefaultLockscreenImage; }
            else
            {
                LibraryObject tempLO = tempList[random.Next(tempList.Count)];

                Debug.WriteLine("Anime Selected: " + tempLO.anime.slug);

                if (setToLockscreen)
                {
                    await SetImage(tempLO.anime.cover_image_uri);
                }

                return tempLO.anime.cover_image_uri;
            }
        }

        public static async Task SetImage(Uri _uri)
        {
            if (_uri == null) return;

            //First Delete Old image
            //DeleteLockscreenImage();

            // If the uri contains the default background, set it to the default
            Uri uri;
            if (_uri.OriginalString.Contains("defaultLockscreenBackground.jpg")) { Debug.WriteLine("Default 1"); uri = DefaultLockscreenImage; }
            else if (_uri.OriginalString.Contains("defaultLockscreenBackground2.jpg")) { Debug.WriteLine("Default 2"); uri = DefaultLockscreenImage2; }
            else { Debug.WriteLine("Not a Default"); uri = _uri; }

            Debug.WriteLine("Image Path: " + uri.OriginalString);

            string fileName = uri.Segments[uri.Segments.Length - 1];
            string imageName = BackgroundRoot + fileName;

            if (StorageTools.Settings.LockscreenImageFile.Value)
            {
                await StorageTools.SaveFileFromServer(uri, LOCKSCREEN_IMAGE);
                StorageTools.Settings.LockscreenImageFile.Value = false;
            }
            else
            {
                await StorageTools.SaveFileFromServer(uri, LOCKSCREEN_IMAGE_1);
                StorageTools.Settings.LockscreenImageFile.Value = true;
            }

            StorageTools.Settings.CurrentLockscreenImageUri.Value = uri;

            await SetLockScreen();
        }

        public static async Task SetLockScreen(int defaultNum = -1)
        {
            bool hasAccessForLockScreen = LockScreenManager.IsProvidedByCurrentApplication;

            if (!hasAccessForLockScreen)
            {
                var accessRequested = await LockScreenManager.RequestAccessAsync();
                hasAccessForLockScreen = (accessRequested == LockScreenRequestResult.Granted);

                Consts.HasAccessForLockscreen = hasAccessForLockScreen;
            }

            if (hasAccessForLockScreen)
            {
                if (defaultNum != -1)
                {
                    if (defaultNum == 0)
                        LockScreen.SetImageUri(DefaultLockscreenImage);
                    else if (defaultNum == 1)
                        LockScreen.SetImageUri(DefaultLockscreenImage2);
                    //Thread.Sleep(TimeSpan.FromSeconds(2));
                }
                else
                {
                    Uri imgUri;
                    if (StorageTools.Settings.LockscreenImageFile.Value)
                    {
                        imgUri = new Uri("ms-appdata:///local/" + LOCKSCREEN_IMAGE_1, UriKind.Absolute);
                    }
                    else
                    {
                        imgUri = new Uri("ms-appdata:///local/" + LOCKSCREEN_IMAGE, UriKind.Absolute);
                    }

                    LockScreen.SetImageUri(imgUri);
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    LockScreen.SetImageUri(imgUri);
                    //}
                }

                Debug.WriteLine("Lockscreen Image Set");
            }
        }
    }
}
