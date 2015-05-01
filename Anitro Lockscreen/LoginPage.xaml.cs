using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Anitro.APIs;
using System.Diagnostics;
using Anitro;
using Anitro.Data_Structures;
using Anitro.APIs.Events;

namespace Anitro_Lockscreen
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public bool postingLogin = false;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Subscribe to the back button event as to not close the page
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            if (Consts.DebugMode)
            {
                usernameBox.Text = Consts.testAccountUsername;
                passwordBox.Text = Consts.testAccountPassword;
            }
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            bool loop = true;
            while (loop)
            {
                if (Anitro.APIs.StorageTools.isSavingComplete &&
                    !postingLogin)
                {
                    loop = false;
                }
            }

            e.Handled = true;

            GoBack();
        }
        private void GoBack()
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            NavigationService.GoBack();
        }

        private void ChangeProgressBar(bool isEnabled)
        {
            if (isEnabled) { ApplicationProgressBar.Visibility = Visibility.Visible ; }
            else { ApplicationProgressBar.Visibility = Visibility.Collapsed; }
            //ApplicationProgressBar.IsActive = isEnabled;
        }

        private void LoginEnterEvent(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Debug.WriteLine("Enter Pressed");
                this.Focus();// Frame.Focus(Windows.UI.Xaml.FocusState.Programmatic);

                Login();
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private async void Login()
        {
            ChangeProgressBar(true);
            Debug.WriteLine("Clicking Login Button");

            APIResponse response = APIResponse.None;

            if (usernameBox.Text == "" || passwordBox.Text == "") { response = APIResponse.InfoNotEntered; }
            else if (!Consts.IsConnectedToInternet()) { response = APIResponse.NetworkError; }
            else
            {
                try
                {
                    Debug.WriteLine("Getting Login Result");
                    loginErrors.Text = "Attempting to Login..";
                    postingLogin = true;

                    HummingbirdAPI.APICompletedEventHandler += LoginCompleted;

                    //this.Dispatcher.BeginInvoke(HummingbirdAPI.V1API.Post.Login //Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    //{
                    HummingbirdAPI.V1API.Post.Login(usernameBox.Text, passwordBox.Text);
                    //});

                    //await Task.Run(() => HummingbirdAPI.V1API.Post.Login(usernameBox.Text, passwordBox.Text));
                    //Task responseTask = HummingbirdAPI.V1API.Post.Login(usernameBox.Text, passwordBox.Text);

                }
                catch (Exception) { }
            }

            switch (response)
            {
                case APIResponse.InfoNotEntered:
                    Debug.WriteLine("Username of Password not Entered");
                    loginErrors.Text = "Please type in your username/password";
                    ChangeProgressBar(false);
                    break;
                case APIResponse.NetworkError:
                    Debug.WriteLine("Network Error");
                    loginErrors.Text = "Error Connecting to internet";
                    ChangeProgressBar(false);
                    break;
            }
        }

        async void LoginCompleted(object sender, APICompletedEventArgs e)
        {
            if (e.Type != APIType.Login) { return; }

            Debug.WriteLine("Login Completed");

            bool goBackToMainPage = false;

            switch (e.Result)
            {
                case APIResponse.Successful:
                    Debug.WriteLine(Consts.LoggedInUser.AuthToken + " | " + Consts.LoggedInUser.Username);

                    // Reset the textboxes
                    usernameBox.Text = ""; //UsernameTB.Text = "username     ";
                    passwordBox.Text = ""; //PasswordTB.Text = "password     ";

                    Debug.WriteLine("Success!");
                    loginErrors.Text = "Success!";

                    goBackToMainPage = true;
                    break;
                case APIResponse.NotSupported:
                    Debug.WriteLine("Email not Supported");
                    loginErrors.Text = "Logging in through Email is not supported yet. Please use your Username";
                    break;
                case APIResponse.InfoNotEntered:
                    Debug.WriteLine("Username of Password not Entered");
                    loginErrors.Text = "Please type in your username/password";
                    break;

                case APIResponse.InvalidCredentials:
                    Debug.WriteLine("Invalid Login Credidentials");
                    loginErrors.Text = "Invalid Login Credidentials";
                    break;
                case APIResponse.ServerError:
                    Debug.WriteLine("Error connecting to hummingbird.me");
                    loginErrors.Text = "Error connecting to hummingbird.me";
                    break;
                case APIResponse.NetworkError:
                    Debug.WriteLine("Network Error");
                    loginErrors.Text = "Error Connecting to internet";
                    break;
                case APIResponse.UnknownError:
                default:
                    Debug.WriteLine("An Unknown Error has Occured");
                    loginErrors.Text = "An Unknown Error has Occured";
                    break;
            }

            // Remove the Event Handler
            HummingbirdAPI.APICompletedEventHandler -= LoginCompleted;

            ChangeProgressBar(false);
            postingLogin = false;
            Debug.WriteLine("Login Result Posted\n\n");

            if (goBackToMainPage)
            {
                await Consts.LoggedInUser.Save();

                GoBack();
            }
        }
    }
}