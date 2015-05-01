using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Anitro.APIs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Anitro.Data_Structures
{
    public enum UserSaveSettings
    {
        All,
        JustUser
    }

    public class User
    {
        public bool IsLoggedIn
        {
            get
            {
                bool iLI = (authToken != "") && (Username != "");

                Debug.WriteLine("User Logged In: " + iLI.ToString());
                return iLI;
            }
        }

        public string AuthToken
        {
            get
            {
                if (IsLoggedIn) {
                    Debug.WriteLine("Grabbing users Auth Token: "+ authToken);
                    return authToken;
                }
                else { 
                    Debug.WriteLine("Grabbing valid-placeholder AuthToken");
                    return "swmRwwrimshWG8EtjKZK"; 
                }
            }
            set { authToken = value; }
        }
        private string authToken;

        public string Username; // { get; set; }
        public string AvatarURL;
        public string EmailAddress; // { get; set; }
        private string Password; // { get; set; }

        public API_Classes.UserInfo UserInfo;

        public Library animeLibrary;
        public ObservableCollection<API_Classes.V1.ActivityFeedObject> activityFeed;


        public User()
        {
            Username = "";
            Password = "";
            EmailAddress = "";
            AuthToken = "";
            AvatarURL = "";

            UserInfo = new API_Classes.UserInfo();

            animeLibrary = new Library("");
            activityFeed = new ObservableCollection<API_Classes.V1.ActivityFeedObject>();
        }
        public User(string _username) : this()
        {
            Username        = _username;
            animeLibrary.Owner   = _username;
        }
        public User(User storageUser)
        {
            SetFromOtherUser(storageUser);
        }

        private void SetFromOtherUser(User storageUser)
        {
            Username = storageUser.Username;
            Password = storageUser.Password;
            EmailAddress = storageUser.EmailAddress;
            AuthToken = storageUser.AuthToken;
            AvatarURL = storageUser.AvatarURL;

            // Create activity feed to be populated later
            activityFeed = new ObservableCollection<API_Classes.V1.ActivityFeedObject>();

            //-- Load UserInfo from actual storage
            if (storageUser.UserInfo == null)
                UserInfo = new API_Classes.UserInfo();
            else
                UserInfo = storageUser.UserInfo;

            //-- Load Library from actual storage
            if (storageUser.animeLibrary == null)
                animeLibrary = new Library(storageUser.Username);
            else
                animeLibrary = storageUser.animeLibrary;
        }
        public User CreateStorageObject()
        {
            User storageUser = new User(Username);
            storageUser.Password = Password;
            storageUser.EmailAddress = EmailAddress;
            storageUser.AuthToken = AuthToken;
            storageUser.AvatarURL = AvatarURL;

            storageUser.UserInfo = UserInfo;

            storageUser.animeLibrary = null;
            storageUser.activityFeed = null;

            return storageUser;
        }
        

        public void Login(string _user, string _auth, string pass, string email = "")
        {
            Username = _user;
            animeLibrary.Owner = _user;

            EmailAddress = email;

            AuthToken = _auth;
            Password = pass;

            animeLibrary.ClearLibrary(LibrarySelection.All);

            Consts.justSignedIn = true;
        }
        public void Logout()
        {
            Username = "";
            animeLibrary.Owner = "";
            EmailAddress = "";
            
            AuthToken = "";
            Password = "";

            AvatarURL = "";

            UserInfo = new API_Classes.UserInfo();

            animeLibrary.ClearLibrary(LibrarySelection.All);
            activityFeed = new ObservableCollection<API_Classes.V1.ActivityFeedObject>();
        }

        #region Storage Tools
        #region Json Tools
        public string ThisToJson(UserSaveSettings saveSettings = UserSaveSettings.All)
        {
            Debug.WriteLine("ThisToJson(): Converting to JSON");
            switch (saveSettings)
            {
                case UserSaveSettings.All:
                    return JsonConvert.SerializeObject(this);
                case UserSaveSettings.JustUser:
                    return JsonConvert.SerializeObject(CreateStorageObject());
                default:
                    return "";
            }
        }

        public void JsonToThis(string json)
        {
            JObject jObject = JObject.Parse(json);
            User storageUser = JsonConvert.DeserializeObject<User>(jObject.ToString());

            SetFromOtherUser(storageUser);
        }
        #endregion

        public static async System.Threading.Tasks.Task<User> Load()
        {
            User user = new User();

            if (await StorageTools.DoesFileExist(StorageTools.StorageConsts.UserFile))
            {
                try
                {
                    string json = await StorageTools.LoadFileFromStorage(StorageTools.StorageConsts.UserFile);
                    user.JsonToThis(json);
                }
                catch (Exception)
                {
                    Consts.LoggedInUser.DeleteFile();
                    user = new User();
                }
            }

            return user;
        }
        public async System.Threading.Tasks.Task<bool> Save(UserSaveSettings saveSettings = UserSaveSettings.All)
        {
            try {
                Debug.WriteLine("Save(): Begun");
                StorageTools.isSavingComplete = false;

                string json = ThisToJson(saveSettings);
                bool result = await StorageTools.SaveToStorage(StorageTools.StorageConsts.UserFile, json);

                Debug.WriteLine("Save(): Success!");
                return result;
            }
            catch(Exception) {
                Debug.WriteLine("Save(): Failed");
                StorageTools.isSavingComplete = true;
                return false;
            }
        }
        public async System.Threading.Tasks.Task DeleteFile()
        {
            try
            {
                await StorageTools.DeleteFile(StorageTools.StorageConsts.UserFile, Windows.Storage.StorageDeleteOption.PermanentDelete);
            }
            catch (Exception) { }
        }
        #endregion
    }
}
