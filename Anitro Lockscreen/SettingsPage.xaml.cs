using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Anitro;
using Anitro.APIs;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;
using Anitro.Data_Structures;
using Anitro_API.Data_Structures.Enumerators;

namespace Anitro_Lockscreen
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        bool libraryLoaded = false;
        bool initalSet = false;

        bool listPickerLoaded = false;
        private string[] backgroundTask_listPickerOptions =
        {
            "off",
            "every activation",
            "every hour",
            "every four hours",
            "once a day",
            "twice a day",
            "every other day",
        };

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Consts.HasAccessForLockscreen = LockScreenManager.IsProvidedByCurrentApplication;
            lockscreenSwitch.IsChecked = Consts.HasAccessForLockscreen;


            string navParamKey = "fromLockscreen";//anitrols";
            string navParamValue = "";
            bool navigationParamExists = NavigationContext.QueryString.TryGetValue(navParamKey, out navParamValue);
            if (navigationParamExists)
            {
                MainPivot.SelectedIndex = 1;
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

        private void ChangeProgressBar(bool isEnabled)
        {
            if (isEnabled) { ApplicationProgressBar.Visibility = Visibility.Visible; }
            else { ApplicationProgressBar.Visibility = Visibility.Collapsed; }
        }

        private void LoginView()
        {
            scheduledTaskRunner.Text = "scheduled task last run: " + StorageTools.Settings.ScheduledTaskLastRun.Value.ToString();
            logoutUsernameText.Text = Consts.LoggedInUser.Username;

            LoggedIn.Visibility = System.Windows.Visibility.Visible;
            LoggedOut.Visibility = System.Windows.Visibility.Collapsed;

            LoadLibrary();
        }
        private void LogoutView()
        {
            scheduledTaskRunner.Text = "";
            logoutUsernameText.Text = "";

            LoggedIn.Visibility = System.Windows.Visibility.Collapsed;
            LoggedOut.Visibility = System.Windows.Visibility.Visible;
        }

        private void SetBackgroundTaskItem()
        {
            try
            {
                backgroundTaskSelector.ItemsSource = backgroundTask_listPickerOptions;

                // Set the value
                AutoUpdateSetting currentSetting = StorageTools.Settings.BackgroundTaskSettings.Value;

                switch (currentSetting)
                {
                    case AutoUpdateSetting.EveryActivation:
                        backgroundTaskSelector.SelectedIndex = 1;
                        break;
                    case AutoUpdateSetting.EveryHour:
                        backgroundTaskSelector.SelectedIndex = 2;
                        break;
                    case AutoUpdateSetting.EveryFourHours:
                        backgroundTaskSelector.SelectedIndex = 3;
                        break;
                    case AutoUpdateSetting.OnceADay:
                        backgroundTaskSelector.SelectedIndex = 4;
                        break;
                    case AutoUpdateSetting.TwiceADay:
                        backgroundTaskSelector.SelectedIndex = 5;
                        break;
                    case AutoUpdateSetting.EveryOtherDay:
                        backgroundTaskSelector.SelectedIndex = 6;
                        break;
                    case AutoUpdateSetting.Off:
                    default:
                        backgroundTaskSelector.SelectedIndex = 0;
                        break;
                }
            }
            catch (Exception) { }
        }

        #region Navigation
        private async void lockscreenButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
            }
            catch (Exception) { }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }
        #endregion

        #region MessageBoxes
        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Logout?",
                Message = "Are you sure you want to log out of " + Consts.LoggedInUser.Username,//Consts.settings.userName,
                LeftButtonContent = "yes",
                RightButtonContent = "no",
                IsFullScreen = false
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        Consts.LoggedInUser.DeleteFile();
                        LogoutView();
                        break;
                    case CustomMessageBoxResult.RightButton:
                    case CustomMessageBoxResult.None:
                    default:
                        // Do nothing.
                        break;
                }
            };


            messageBox.Show();
        }
        private void lockscreenSwitch_UnChecked(object sender, RoutedEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = "Disable lockscreen?",
                Message = "To disable lock screen you need to change your lockscreen provider, would you like to go to that settings screen now?",
                LeftButtonContent = "yes",
                RightButtonContent = "no",
                IsFullScreen = false
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        lockscreenButton_Clicked(sender, e);
                        break;
                    case CustomMessageBoxResult.RightButton:
                    case CustomMessageBoxResult.None:
                    default:
                        break;
                }
            };

            messageBox.Show();
        }
        #endregion

        #region Lockscreen
        private async void lockscreenSwitch_Checked(object sender, RoutedEventArgs e)
        {
            //if (!initalSet) { initalSet = true; return; }
            //if (!libraryLoaded) return;

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
                        await UpdateLockscreenImage(new Uri("Assets/defaultLockscreenBackground.jpg", UriKind.Relative));
                    }
                }
            }
            catch (Exception) { }
        }

        private async void randomUpdate_Button_Clicked(object sender, RoutedEventArgs e)
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
            try
            {
                if (Consts.IsConnectedToInternet())
                {
                    if (!Consts.HasAccessForLockscreen) { return; }
                    else
                    {
                        //Lockscreen_Helper.DeleteLockscreenImage();
                        if (uri.OriginalString.Contains("defaultLockscreenBackground.jpg")) { await Lockscreen_Helper.SetLockScreen(0); }
                        else if (uri == null)
                        {
                            await Lockscreen_Helper.SetRandomImageFromLibrary();
                        }
                        else
                        {
                            await Lockscreen_Helper.SetImage(uri);
                        }
                        MessageBox.Show("Locksceen has been updated");

                        Debug.WriteLine("Setting Current Image");
                        if (!(StorageTools.Settings.CurrentLockscreenImageUri.IsDefault()))
                        {
                            Uri imgUri = StorageTools.Settings.CurrentLockscreenImageUri.Value;
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Custom Events
        private async void LoadLibrary()
        {
            if (Consts.LoggedInUser.animeLibrary.IsEveryLibraryEmpty(LibrarySelection.All))
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
        #endregion

        private void BackgroundTaskSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!listPickerLoaded) return;

            try
            {
                switch (backgroundTaskSelector.SelectedIndex)
                {
                    case 1:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.EveryActivation;
                        break;
                    case 2:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.EveryHour;
                        break;
                    case 3:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.EveryFourHours;
                        break;
                    case 4:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.OnceADay;
                        break;
                    case 5:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.TwiceADay;
                        break;
                    case 6:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.EveryOtherDay;
                        break;
                    case 0:
                    default:
                        StorageTools.Settings.BackgroundTaskSettings.Value = AutoUpdateSetting.Off;
                        break;
                }
            }
            catch (Exception) { }
        }

        private void backgroundTaskSelector_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackgroundTaskItem();
            listPickerLoaded = true;
        }
    }
}