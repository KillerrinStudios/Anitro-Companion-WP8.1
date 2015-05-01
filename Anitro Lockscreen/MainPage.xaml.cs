using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Anitro_Lockscreen.Resources;
using Anitro;
using Anitro.APIs;
using System.Diagnostics;
using Windows.Phone.System.UserProfile;
using Anitro.Data_Structures;
using System.Windows.Media.Imaging;

using Microsoft.Phone.Scheduler;
using Anitro.Data_Structures.API_Classes;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;

namespace Anitro_Lockscreen
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool libraryLoaded = false;
        public static bool openedThroughLockscreen = false;
        public static bool openedThroughProtocol = false;
        public static Uri nextUri;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
 	        base.OnNavigatedTo(e);

            Consts.HasAccessForLockscreen = LockScreenManager.IsProvidedByCurrentApplication;

            if (!Consts.LoggedInUser.IsLoggedIn)
            {
                Consts.LoggedInUser = await Anitro.Data_Structures.User.Load();
            }

            if (!Settings.Loaded)
            {
                Consts.AppSettings = await Anitro.Data_Structures.Settings.Load();
                Debug.WriteLine(Consts.AppSettings.ToString());
            }

            if (!openedThroughLockscreen)
            {
                string lockscreenKey = "WallpaperSettings";
                string lockscreenValue = "0";
                bool lockscreenValueExists = NavigationContext.QueryString.TryGetValue(lockscreenKey, out lockscreenValue);

                if (lockscreenValueExists)
                {
                    openedThroughLockscreen = true;
                    string sendData = "fromLockscreen=" + true;
                    NavigationService.Navigate(new Uri("/SettingsPage.xaml?" + sendData, UriKind.Relative));
                    return;
                }
            }

            if (!openedThroughProtocol)
            {
                string navParamKey = "args";
                string navParamValue = "";
                bool navigationParamExists = NavigationContext.QueryString.TryGetValue(navParamKey, out navParamValue);
                if (navigationParamExists)
                {
                    openedThroughProtocol = true;
                    await LaunchedThroughParameter(navParamValue);
                }
            }

            
            if (Consts.LoggedInUser.IsLoggedIn)
            {
                LoginView();
            }
            else
            {
                LogoutView();
            }
        }

        void LoginView()
        {
            LoggedOut.Visibility = System.Windows.Visibility.Collapsed;
            LoggedIn.Visibility = System.Windows.Visibility.Visible;

            // Enable the buttons
            randomUpdate_Button.IsEnabled = Consts.HasAccessForLockscreen;
            setLockscreen_Button.IsEnabled = Consts.HasAccessForLockscreen;
            selectImage_Button.IsEnabled = Consts.HasAccessForLockscreen;

            Debug.WriteLine("Enabling Library Switches");
            randomize_Favourites.IsEnabled =        Consts.HasAccessForLockscreen;
            randomize_CurrentlyWatching.IsEnabled = Consts.HasAccessForLockscreen;
            randomize_Completed.IsEnabled =         Consts.HasAccessForLockscreen;
            randomize_PlanToWatch.IsEnabled =       Consts.HasAccessForLockscreen;
            randomize_OnHold.IsEnabled =            Consts.HasAccessForLockscreen;
            randomize_Dropped.IsEnabled =           Consts.HasAccessForLockscreen;

            Debug.WriteLine("Checking Library Switches");
            randomize_Favourites.IsChecked =        Anitro.APIs.StorageTools.Settings.Lockscreen.randomizeFavourites.Value;
            randomize_CurrentlyWatching.IsChecked = Anitro.APIs.StorageTools.Settings.Lockscreen.randomizeCurrentlyWatching.Value;
            randomize_Completed.IsChecked =         Anitro.APIs.StorageTools.Settings.Lockscreen.randomizeCompleted.Value;
            randomize_PlanToWatch.IsChecked =       Anitro.APIs.StorageTools.Settings.Lockscreen.randomizePlanToWatch.Value;
            randomize_OnHold.IsChecked =            Anitro.APIs.StorageTools.Settings.Lockscreen.randomizeOnHold.Value;
            randomize_Dropped.IsChecked =           Anitro.APIs.StorageTools.Settings.Lockscreen.randomizeDropped.Value;

            Debug.WriteLine("Setting Current Image");
            if (!(StorageTools.Settings.CurrentLockscreenImageUri.IsDefault()))
            {
                nextUri = StorageTools.Settings.CurrentLockscreenImageUri.Value;
                CurrentImage.Source = new BitmapImage(nextUri);
            }

            // Force the server to pull newest
            Consts.justSignedIn = true;

            LoadLibrary();
        }
        void LogoutView()
        {
            LoggedOut.Visibility = System.Windows.Visibility.Visible;
            LoggedIn.Visibility = System.Windows.Visibility.Collapsed;
            
            libraryLoaded = true;
        }

        private async Task LaunchedThroughParameter(string query)
        {
            Debug.WriteLine("Launched through Query: " + query); // navParamValue);

            //anitParam.Text = query;

            if (Consts.LoggedInUser.IsLoggedIn)
            {
                //bool isInLibrary = Consts.LoggedInUser.animeLibrary.DoesExistInLibrary(LibrarySelection.All, navParamValue);
                LibrarySelection libSel = Consts.LoggedInUser.animeLibrary.FindWhereExistsInLibrary(query);
                switch (libSel)
                {
                    case LibrarySelection.None:
                        break;
                    default:
                        LibraryObject obj = Consts.LoggedInUser.animeLibrary.GetObjectInLibrary(libSel, query);
                        await UpdateLockscreenImage(obj.anime.cover_image_uri, true);
                        return;
                }
            }

            
            //If not, default to Grabbing it off the web
            if (Consts.IsConnectedToInternet())
            {
                ChangeProgressBar(true);
                HummingbirdAPI.APICompletedEventHandler += AnimeGrabbed_APICompletedEventHandler;
                HummingbirdAPI.V1API.Get.Anime(query);
            }
        }

        private void AppBar_Refresh_Click(object sender, EventArgs e)
        {
            if (!Consts.LoggedInUser.IsLoggedIn) return;
            if (!libraryLoaded) return;

            // Set the variable to true to force a grab
            libraryLoaded = false;
            Consts.LoggedInUser.animeLibrary.ClearLibrary(LibrarySelection.All);
            Consts.justSignedIn = true;
            LoadLibrary(true);
        }

        private void ChangeProgressBar(bool isEnabled)
        {
            if (isEnabled) { ApplicationProgressBar.Visibility = Visibility.Visible; }
            else { ApplicationProgressBar.Visibility = Visibility.Collapsed; }
        }

        #region Navigation
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }
        private void AppBar_Settings_Click(object sender, EventArgs e)
        {
            if (!libraryLoaded) return;
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
        private void SelectImage_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (!libraryLoaded) return;
            NavigationService.Navigate(new Uri("/SelectionPage.xaml", UriKind.Relative));
        }

        private async void AppBar_Review_Click(object sender, EventArgs e)
        {
            if (!libraryLoaded) return;

            //await Consts.LaunchReview();
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
        
        #endregion

        #region Lockscreen
        private async void NextImage_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (!libraryLoaded) return;

            nextUri = await Lockscreen_Helper.SetRandomImageFromLibrary(false);
            CurrentImage.Source = new BitmapImage(nextUri);
        }

        private async void UpdateImage_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (!libraryLoaded) return;
            if (nextUri == null) return;

            await UpdateLockscreenImage(nextUri);
        }

        private async void RandomUpdate_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (!libraryLoaded) return;

            try
            {
                Consts.HasAccessForLockscreen = LockScreenManager.IsProvidedByCurrentApplication;
                if (!Consts.HasAccessForLockscreen)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    var accessRequested = await LockScreenManager.RequestAccessAsync();

                    Consts.HasAccessForLockscreen = (accessRequested == LockScreenRequestResult.Granted);
                    if (Consts.HasAccessForLockscreen)
                    {
                        await UpdateLockscreenImage();
                    }
                }
                else
                {
                    await UpdateLockscreenImage();
                }
            }
            catch (Exception) { }
        }

        private async Task UpdateLockscreenImage(Uri uri = null, bool ignoreEarlyCheck = false)
        {
            if (!ignoreEarlyCheck) { if (!libraryLoaded) return; }

            try
            {
                if (Consts.IsConnectedToInternet())
                {
                    if (!Consts.HasAccessForLockscreen) {
                        //var accessRequested = await LockScreenManager.RequestAccessAsync();

                        //Consts.HasAccessForLockscreen = (accessRequested == LockScreenRequestResult.Granted);
                        //if (Consts.HasAccessForLockscreen)
                        //{
                        //    await UpdateLockscreenImage(uri, ignoreEarlyCheck);
                        //}
    
                        return;
                    }
                    else
                    {
                        //Lockscreen_Helper.DeleteLockscreenImage();
                        if (uri == null)
                        {
                            nextUri = await Lockscreen_Helper.SetRandomImageFromLibrary();
                        }
                        else
                        {
                            await Lockscreen_Helper.SetImage(uri);
                        }
                        MessageBox.Show("Lockscreen Image Updated!");

                        Debug.WriteLine("Setting Current Image");
                        if (!(StorageTools.Settings.CurrentLockscreenImageUri.IsDefault()))
                        {
                            Uri imgUri = StorageTools.Settings.CurrentLockscreenImageUri.Value;
                            CurrentImage.Source = new BitmapImage(imgUri);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        #region Randomize Lockscreen Switches
        #region UnChecked
        private void randomize_Dropped_UnChecked(object sender, RoutedEventArgs e)
        {

            StorageTools.Settings.Lockscreen.randomizeDropped.Value = false;
        }

        private void randomize_OnHold_UnChecked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeOnHold.Value = false;

        }

        private void randomize_Completed_UnChecked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeCompleted.Value = false;

        }

        private void randomize_PlanToWatch_UnChecked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizePlanToWatch.Value = false;

        }

        private void randomize_CurrentlyWatching_UnChecked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeCurrentlyWatching.Value = false;

        }

        private void randomize_Favourites_UnChecked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeFavourites.Value = false;
        }
        #endregion
        #region Checked
        private void randomize_Favourites_Checked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeFavourites.Value = true;
        }

        private void randomize_CurrentlyWatching_Checked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeCurrentlyWatching.Value = true;
        }

        private void randomize_PlanToWatch_Checked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizePlanToWatch.Value = true;
        }

        private void randomize_Completed_Checked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeCompleted.Value = true;
        }

        private void randomize_OnHold_Checked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeOnHold.Value = true;
        }

        private void randomize_Dropped_Checked(object sender, RoutedEventArgs e)
        {
            StorageTools.Settings.Lockscreen.randomizeDropped.Value = true;
        }
        #endregion
        #endregion
        #endregion

        #region Custom Events
        private async void LoadLibrary(bool forcePull = false)
        {
            if (Consts.LoggedInUser.animeLibrary.IsEveryLibraryEmpty(LibrarySelection.All) || forcePull)
            {
                if (Consts.IsConnectedToInternet())
                {
                    Debug.WriteLine("LoadLibrary(): Loading Library");

                    ChangeProgressBar(true);
                    try
                    {
                        Consts.LoggedInUser.animeLibrary.LibraryLoadedEventHandler += LibraryLoaded;

                        //this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        //{
                            Consts.LoggedInUser.animeLibrary.LoadFromStorageOrServer(Consts.justSignedIn);
                        //});
                    }
                    catch (Exception)
                    {
                        Consts.LoggedInUser.animeLibrary.LibraryLoadedEventHandler -= LibraryLoaded;
                        ChangeProgressBar(false);
                        libraryLoaded = true;
                    }
                }
                else
                {
                    libraryLoaded = true;
                }
            }
            else
            {
                if (Consts.justSignedIn)
                libraryLoaded = true;
            }
        }

        private void LibraryLoaded(object sender, Anitro.APIs.Events.LibraryLoadedEventArgs e)
        {
            if (e.Type != Anitro.Data_Structures.APIType.LoadLibrary) return;
            Debug.WriteLine("LibraryLoaded(): MainPage Loaded");

            switch (e.Result)
            {
                case Anitro.Data_Structures.APIResponse.Successful:
                    Debug.WriteLine("LibraryLoaded(): Successful!");
                    break;
                case Anitro.Data_Structures.APIResponse.Failed:
                    Debug.WriteLine("LibraryLoaded(): Failed");
                    break;
            }

            //for (int i = 0; i < 9; i++)
            //{
            //    bool s = false;
            //    if (i == 8) s = true;
            //    Consts.LoggedInUser.animeLibrary.AddToRecent(Consts.LoggedInUser.animeLibrary.CurrentlyWatching[i], s);
            //}

            // Remove the Event Handler
            Consts.LoggedInUser.animeLibrary.LibraryLoadedEventHandler -= LibraryLoaded;
            ChangeProgressBar(false);
            libraryLoaded = true;
        }

        async void AnimeGrabbed_APICompletedEventHandler(object sender, Anitro.APIs.Events.APICompletedEventArgs e)
        {
            switch (e.Result)
            {
                case APIResponse.Successful:
                    Anime animeObject = (sender as Anime);
                    await UpdateLockscreenImage(animeObject.cover_image_uri, true);
                    break;
                case APIResponse.Failed:
                    break;
            }

            ChangeProgressBar(false);
            HummingbirdAPI.APICompletedEventHandler -= AnimeGrabbed_APICompletedEventHandler;
        }
        #endregion
    }
}