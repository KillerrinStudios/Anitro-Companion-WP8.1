using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Anitro;
using Anitro.Data_Structures.API_Classes;
using System.Threading.Tasks;
using Anitro.APIs;
using System.Windows.Media;

namespace Anitro_Lockscreen
{
    public partial class SelectionPage : PhoneApplicationPage
    {
        private bool pageLoaded = false;
        public SelectionPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!pageLoaded)
            {
                AddAllLibrary();
            }
        }

        private void AddAllLibrary()
        {
            ObservableCollection<LibraryObject> library = Consts.LoggedInUser.animeLibrary.GetLibraryInSingleCollection(Anitro.Data_Structures.LibrarySelection.APISupported, true);

            AddItem(new Uri("Assets/defaultLockscreenBackground.jpg", UriKind.Relative)); //Lockscreen_Helper.DefaultLockscreenImage);
            AddItem(new Uri("Assets/defaultLockscreenBackground2.jpg", UriKind.Relative)); //Lockscreen_Helper.DefaultLockscreenImage2);
            AddItem(new Uri("/", UriKind.Relative)); // Just to preserve formatting

            foreach (LibraryObject lO in library)
            {
                AddItem(lO.anime.cover_image_uri);
            }

            pageLoaded = true;
        }

        private void AddItem(Uri _source)
        {
            Image image = new Image()
            {
                Width = 130,
                Height = 230,
                Margin = new Thickness(2),
            };

            image.Source = new BitmapImage(_source);

            if (_source.OriginalString[0] != '/')
                image.Tap += OnTapped;

            animeCover_wrapPanel.Children.Add(image);
        }

        private async void OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image image = sender as Image;
            if (null != image)
            {
                BitmapImage source = image.Source as BitmapImage;
                Uri uri = source.UriSource;

                if (!string.IsNullOrWhiteSpace(uri.OriginalString))
                {
                    Debug.WriteLine(uri.OriginalString);
                    await UpdateLockscreenImage(uri);
                }
            }
        }

        private async Task UpdateLockscreenImage(Uri uri = null)
        {
            if (uri == null) return;

            try
            {
                if (Consts.IsConnectedToInternet())
                {
                    if (!Consts.HasAccessForLockscreen) { return; }
                    else
                    {

                        if (uri.OriginalString.Contains("defaultLockscreenBackground.jpg")) { Debug.WriteLine("Default 1"); await Lockscreen_Helper.SetLockScreen(0); }
                        else if (uri.OriginalString.Contains("defaultLockscreenBackground2.jpg")) { Debug.WriteLine("Default 2"); await Lockscreen_Helper.SetLockScreen(1); }
                        else { Debug.WriteLine("Not a Default"); await Lockscreen_Helper.SetImage(uri); }
                        
                        MessageBox.Show("Lockscreen Image Updated!");

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
    }
}