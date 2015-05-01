using System;
using System.Collections.Generic;
using System.Text;

using Anitro.Data_Structures;
using Anitro.Data_Structures.API_Classes;

using Anitro.APIs.Events;

using System.Threading;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.ApplicationModel.Core;
using Anitro.Data_Structures.Enumerators;

namespace Anitro.APIs
{
    public static class HummingbirdAPI
    {
        public static event APICompletedEventHandler APICompletedEventHandler;

        public static class V1API
        {
            public static class Get
            {
                public static class Parsers
                {
                    public static bool ParseActivityFeed(Anitro.Data_Structures.API_Classes.V1.ActivityFeed aF)
                    {
                        Debug.WriteLine("ParseActivityFeed(): Entering");
                        try
                        {
                            //StorageTools.SaveFileFromServer(new Uri(aF.status_feed[0].user.avatar, UriKind.Absolute), StorageTools.StorageConsts.AvatarImage);

                            foreach (Anitro.Data_Structures.API_Classes.V1.StatusFeedObject sFO in aF.status_feed)
                            {
                                Anitro.Data_Structures.API_Classes.V1.ActivityFeedObject temp = new Anitro.Data_Structures.API_Classes.V1.ActivityFeedObject { };
                                string contentString;

                                if (string.IsNullOrEmpty(Consts.LoggedInUser.AvatarURL))//Consts.settings.userAvatar))
                                {
                                    Debug.WriteLine("Saving avatar");
                                    //Storage.SaveFileFromServer(new Uri(sFO.user.avatar, UriKind.Absolute), "avatar.jpg");
                                    //    Debug.WriteLine("Avatar done saving");

                                    Consts.LoggedInUser.AvatarURL = sFO.user.avatar; //Storage.Settings.User.userAvatar.Value = sFO.user.avatar;
                                        //Consts.settings.userAvatar = sFO.user.avatar;
                                        //Storage.SaveSettingsInfo();
                                }

                                switch (sFO.story_type)
                                {
                                    case "media_story":
                                        string tts = sFO.updated_at.Substring(0, sFO.updated_at.Length - 1);
                                        string[] tS = tts.Split('T');

                                        if (sFO.substories[0].substory_type == "watchlist_status_update")
                                        {
                                            switch (sFO.substories[0].new_status)
                                            {
                                                case "currently_watching":
                                                    contentString = sFO.user.name + " is currently watching";
                                                    break;
                                                case "plan_to_watch":
                                                    contentString = sFO.user.name + " plans to watch";
                                                    break;
                                                case "completed":
                                                    contentString = sFO.user.name + " has completed";
                                                    break;
                                                case "on_hold":
                                                    contentString = sFO.user.name + " has placed on hold";
                                                    break;
                                                case "dropped":
                                                    contentString = sFO.user.name + " has dropped";
                                                    break;
                                                default:
                                                    contentString = "";
                                                    break;
                                            }
                                        }
                                        else if (sFO.substories[0].substory_type == "watched_episode")
                                        {
                                            contentString = sFO.user.name + " watched episode " + sFO.substories[0].episode_number;
                                        }
                                        else { contentString = ""; }

                                        string storyImageString = "";

                                        temp = new Anitro.Data_Structures.API_Classes.V1.ActivityFeedObject
                                        {
                                            slug = sFO.media.slug,
                                            storyImage = sFO.media.cover_image,
                                            header = sFO.media.title,
                                            content = contentString,
                                            timeStamp = tS[0] + " at " + tS[1],
                                        };
                                        break;
                                    case "comment":
                                        string tts2 = sFO.updated_at.Substring(0, sFO.updated_at.Length - 1);
                                        string[] tS2 = tts2.Split('T');

                                        string commentCut = sFO.substories[0].comment;

                                        var schema = "ms-appdata:///Local/";
                                        string storyimage = sFO.user.avatar; //schema + Storage.AVATARIMAGE;


                                        temp = new Anitro.Data_Structures.API_Classes.V1.ActivityFeedObject
                                        {
                                            storyImage = storyimage, //sFO.user.avatar,
                                            header = sFO.user.name,
                                            content = sFO.substories[0].comment,
                                            timeStamp = tS2[0] + " at " + tS2[1],
                                        };
                                        break;
                                }

                                //activityFeedObject.Add(temp);
                                Consts.LoggedInUser.activityFeed.Add(temp);
                            }

                            Debug.WriteLine("ParseActivityFeed(): Exiting Success!");
                            return true;
                        }
                        catch (Exception) { Debug.WriteLine("ParseActivityFeed(): Exiting Failed"); return false; }
                    }
                    public static List<Anitro.Data_Structures.API_Classes.V1.Anime> ParseSearchResult(string searchResponseAsString)
                    {
                        ///
                        /// If Errors arrise, convert back to an async which returns a Task<List<Anime>>
                        /// 

                        Debug.WriteLine("ParseSearchResult(): Entering");
                        //Debug.WriteLine(searchResponseAsString);

                        if (String.IsNullOrWhiteSpace(searchResponseAsString)) { Debug.WriteLine("ParseSearchResult(): String is null or empty"); return new List<Anitro.Data_Structures.API_Classes.V1.Anime>(); }
                        else
                        {
                            Debug.WriteLine("ParseSearchResult(): String is Populated");
                            string response = "{\"anime\":" + searchResponseAsString + "}";

                            JObject o = JObject.Parse(response); // This would be the string you defined above
                            //Debug.WriteLine("Parsed");
                            Anitro.Data_Structures.API_Classes.V1.AnimeList ani = JsonConvert.DeserializeObject<Anitro.Data_Structures.API_Classes.V1.AnimeList>(o.ToString());
                            //Debug.WriteLine("Parsing Lib parsed");

                            foreach (Anitro.Data_Structures.API_Classes.V1.Anime a in ani.anime)
                            {
                                a.cover_image_uri = new Uri(a.cover_image, UriKind.Absolute);
                            }

                            Debug.WriteLine("ParseSearchResult(): Exiting");
                            return ani.anime;
                        }
                    }
                    public static List<Anitro.Data_Structures.API_Classes.Anime> ParseFavouritesResult(string searchResponseAsString)
                    {
                        ///
                        /// If Errors arrise, convert back to an async which returns a Task<List<Anime>>
                        /// 

                        Debug.WriteLine("ParseFavouritesResult(): Entering");
                        //Debug.WriteLine(searchResponseAsString);

                        if (String.IsNullOrWhiteSpace(searchResponseAsString)) { Debug.WriteLine("ParseFavouritesResult(): String is null or empty"); return new List<Anitro.Data_Structures.API_Classes.Anime>(); }
                        else
                        {
                            Debug.WriteLine("ParseFavouritesResult(): String is Populated");
                            string response = "{\"anime\":" + searchResponseAsString + "}";

                            JObject o = JObject.Parse(response); // This would be the string you defined above
                            //Debug.WriteLine("Parsed");
                            Anitro.Data_Structures.API_Classes.AnimeList ani = JsonConvert.DeserializeObject<Anitro.Data_Structures.API_Classes.AnimeList>(o.ToString());
                            //Debug.WriteLine("Parsing Lib parsed");

                            foreach (Anitro.Data_Structures.API_Classes.Anime a in ani.anime)
                            {
                                a.cover_image_uri = new Uri(a.cover_image, UriKind.Absolute);
                            }

                            Debug.WriteLine("ParseFavouritesResult(): Exiting");
                            return ani.anime;
                        }
                    }
                    
                    public static async Task<bool> ParseLibrary(string responseAsString, LibrarySelection status) //async Task<List<LibraryObject>>
                    {
                        Debug.WriteLine("ParseLibrary(" + status.ToString() + "): Entering");

                        //Debug.WriteLine(responseAsString);

                        if (String.IsNullOrWhiteSpace(responseAsString))
                        {
                            Debug.WriteLine("ParseLibrary(): Exiting");
                            return false;
                        }
                        else
                        {
                            //Debug.WriteLine("Parsing Library");
                            string response = "{\"library\":" + responseAsString + "}";

                            //Debug.WriteLine(response);

                            JObject o = JObject.Parse(response); // This would be the string you defined above
                            //Debug.WriteLine("Parsed");
                            LibraryList lib = JsonConvert.DeserializeObject<LibraryList>(o.ToString()); ;
                            //Debug.WriteLine("Parsing Lib parsed");

                            foreach (LibraryObject lO in lib.library)
                            {
                                Debug.WriteLine("Parsed: " + lO.anime.title + " | " + lO.status);
                                LibraryObject tempAnimeObject = lO;

                                //Set Genres and URI
                                //-- Set the URIs
                                tempAnimeObject.anime.cover_image_uri = new Uri(tempAnimeObject.anime.cover_image, UriKind.Absolute);

                                //-- Get Genres
                                //Task<Anime> temp = GetAnime(tempAnimeObject.anime.slug);
                                //await temp;
                                //tempAnimeObject.anime.genres = temp.Result.genres;
                                tempAnimeObject.anime.genres = new List<Genre> { new Genre { name = "" } };

                                if (!String.IsNullOrEmpty(tempAnimeObject.rating.value)) { tempAnimeObject.rating.valueAsDouble = System.Convert.ToDouble(tempAnimeObject.rating.value); }
                                else { tempAnimeObject.rating.valueAsDouble = 0.0; }

                                //library.Add(tempAnimeObject);


                                switch (status)
                                {
                                    case LibrarySelection.None:
                                        break;
                                    case LibrarySelection.All:
                                        break;
                                    case LibrarySelection.CurrentlyWatching:
                                        break;
                                    case LibrarySelection.PlanToWatch:
                                        break;
                                    case LibrarySelection.Completed:
                                        break;
                                    case LibrarySelection.OnHold:
                                        break;
                                    case LibrarySelection.Dropped:
                                        break;
                                    default:
                                        break;
                                }

                                switch (status)
                                {
                                    case LibrarySelection.All:
                                    case LibrarySelection.None:
                                        Consts.LoggedInUser.animeLibrary.AddToLibrary(Anitro.Data_Structures.Library.GetLibrarySelectionFromStatus(tempAnimeObject.status),
                                                            tempAnimeObject,
                                                            false,
                                                            false);
                                        break;
                                    case LibrarySelection.CurrentlyWatching:
                                        Consts.LoggedInUser.animeLibrary.AddToLibrary(LibrarySelection.CurrentlyWatching, tempAnimeObject, false, false);
                                        //Consts.currentlyWatching.Add(tempAnimeObject);
                                        break;
                                    case LibrarySelection.PlanToWatch:
                                        Consts.LoggedInUser.animeLibrary.AddToLibrary(LibrarySelection.PlanToWatch, tempAnimeObject, false, false);
                                        //Consts.planToWatch.Add(tempAnimeObject);
                                        break;
                                    case LibrarySelection.Completed:
                                        Consts.LoggedInUser.animeLibrary.AddToLibrary(LibrarySelection.Completed, tempAnimeObject, false, false);
                                        //Consts.completed.Add(tempAnimeObject);
                                        break;
                                    case LibrarySelection.OnHold:
                                        Consts.LoggedInUser.animeLibrary.AddToLibrary(LibrarySelection.OnHold, tempAnimeObject, false, false);
                                        //Consts.onHold.Add(tempAnimeObject);
                                        break;
                                    case LibrarySelection.Dropped:
                                        Consts.LoggedInUser.animeLibrary.AddToLibrary(LibrarySelection.Dropped, tempAnimeObject, false, false);
                                        //Consts.dropped.Add(tempAnimeObject);
                                        break;
                                }
                            }

                            switch (status)
                            {
                                case LibrarySelection.None:
                                    break;
                                case LibrarySelection.All:
                                    break;
                                case LibrarySelection.CurrentlyWatching:
                                    break;
                                case LibrarySelection.PlanToWatch:
                                    break;
                                case LibrarySelection.Completed:
                                    break;
                                case LibrarySelection.OnHold:
                                    break;
                                case LibrarySelection.Dropped:
                                    break;
                                default:
                                    break;
                            }

                            Debug.WriteLine("ParseLibrary(" + status.ToString() + "): Exiting");

                            return true;
                        }
                    }
                }

                public static class Streamlining
                {
                    public static async Task AllLibraries(bool forceServerGrab = false)
                    {
                        APICompletedEventArgs args;

                        Debug.WriteLine("GetAllLibraries(): Entering");
                        bool testingLoading = false;

                        #region Regenerate Library
                        bool regenerateLibrary;
                        if (Consts.IsConnectedToInternet())
                        {
                            Debug.WriteLine(DateTime.Now.ToString());
                            if (Debugger.IsAttached)
                            {
                                double time = 1.0;
                                Debug.WriteLine(Consts.LoggedInUser.animeLibrary.lastPulled.AddMinutes(time).ToString());

                                if (DateTime.Now >= Consts.LoggedInUser.animeLibrary.lastPulled.AddMinutes(time))//Consts.settings.libraryLastPulled <= Consts.settings.libraryLastPulled.AddMinutes(1.0))
                                {
                                    if (testingLoading)
                                    {
                                        Debug.WriteLine("Regenerating Library");
                                        await GenerateAllLibraries(LibrarySelection.All);
                                        regenerateLibrary = true;
                                    }
                                    else if (forceServerGrab)
                                    {
                                        Debug.WriteLine("Forcing: Regenerating Library");
                                        await GenerateAllLibraries(LibrarySelection.All);
                                        regenerateLibrary = true;
                                    }
                                    else { Debug.WriteLine("Testing Library Load: Do Not Regen"); regenerateLibrary = false; }
                                }
                                else { Debug.WriteLine("Not enough time passed to auto regen animeLibrary"); regenerateLibrary = false; }
                            }
                            else
                            {
                                Debug.WriteLine(Consts.LoggedInUser.animeLibrary.lastPulled.AddDays(Consts.AppSettings.AutoGenerateLibraryAfterXDays).ToString());
                                if (DateTime.Now >= Consts.LoggedInUser.animeLibrary.lastPulled.AddDays(Consts.AppSettings.AutoGenerateLibraryAfterXDays))//Consts.settings.libraryLastPulled <= Consts.settings.libraryLastPulled.AddDays(Consts.settings.autoGenerateLibraryAfterXDays))
                                {
                                    Debug.WriteLine("Regenerating Library");
                                    await GenerateAllLibraries(LibrarySelection.All);
                                    regenerateLibrary = true;
                                }
                                else if (forceServerGrab)
                                {
                                    Debug.WriteLine("Forcing: Regenerating Library");
                                    await GenerateAllLibraries(LibrarySelection.All);
                                    regenerateLibrary = true;
                                }
                                else { Debug.WriteLine("Not enough time passed to auto regen animeLibrary"); regenerateLibrary = false; }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("No network Connected. Can not attempt to regenerate animeLibrary");
                            regenerateLibrary = false;

                            // Return early with the error
                            args = new APICompletedEventArgs(APIResponse.NetworkError, APIType.LoadLibrary);
                            APICompletedEventHandler(args.UserState, args);
                            return;
                        }
                        #endregion

                        if (!regenerateLibrary)
                        {
                            //Debug.WriteLine("LoadLibrary(): Loading Library: all");
                            //Task<bool> allLibraries = Storage.LoadAnimeLibrary("");
                            //if (!await allLibraries)
                            //{
                            //    GenerateAllLibraries(LibrarySelection.All);
                            //}
                            Debug.WriteLine("LoadLibrary(): Library Loaded");

                            args = new APICompletedEventArgs(APIResponse.Successful, APIType.LoadLibrary);
                            APICompletedEventHandler(args.UserState, args);
                            return;
                        }


                        Debug.WriteLine("GetAllLibraries(): Exiting");
                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.LoadLibrary);
                        APICompletedEventHandler(args.UserState, args);
                        return;
                    }
                    public static async Task<bool> GenerateAllLibraries(LibrarySelection library)
                    {
                        if (!Consts.IsConnectedToInternet())
                        {
                            return false;
                        }

                        switch (library)
                        {
                            case LibrarySelection.CurrentlyWatching:
                                Debug.WriteLine("Generating Library: currently-watching");
                                Task<string> cw = HummingbirdAPI.V1API.Get.Library(LibrarySelection.CurrentlyWatching, true);
                                await cw;
                                break;
                            case LibrarySelection.PlanToWatch:
                                Debug.WriteLine("Generating Library: plan-to-watch");
                                Task<string> ptw = HummingbirdAPI.V1API.Get.Library(LibrarySelection.PlanToWatch, true);
                                await ptw;
                                break;
                            case LibrarySelection.Completed:
                                Debug.WriteLine("Generating Library: completed");
                                Task<string> c = HummingbirdAPI.V1API.Get.Library(LibrarySelection.Completed, true);
                                await c;
                                break;
                            case LibrarySelection.OnHold:
                                Debug.WriteLine("Generating Library: on-hold");
                                Task<string> oh = HummingbirdAPI.V1API.Get.Library(LibrarySelection.OnHold, true);
                                await oh;
                                break;
                            case LibrarySelection.Dropped:
                                Debug.WriteLine("Generating Library: dropped");
                                Task<string> d = HummingbirdAPI.V1API.Get.Library(LibrarySelection.Dropped, true);
                                await d;
                                break;
                            case LibrarySelection.Favourites:
                                Debug.WriteLine("Generating Library: favourites");
                                break;
                            default:
                                break;
                        }

                        if (Consts.isApplicationClosing) { return false; }

                        if (Consts.IsConnectedToInternet())
                        {
                            Debug.WriteLine("Generating Library: all");
                            Task<string> aniLibrary = HummingbirdAPI.V1API.Get.Library(LibrarySelection.All, true);
                            await aniLibrary;

                            APICompletedEventArgs favEvtArgs = await HummingbirdAPI.V1API.Get.Favourites(Consts.LoggedInUser.Username, false);
                            switch (favEvtArgs.Result)
                            {
                                case APIResponse.Successful:
                                    Debug.WriteLine("Generating Library(): Favourites Success!");

                                    foreach(Anime a in (List<Anime>)favEvtArgs.ResultObject)
                                    {
                                        LibraryObject libraryObject = new LibraryObject
                                        {
                                            episodes_watched = "0", //public string episodes_watched { get; set; } // int
                                            last_watched = "",
                                            rewatched_times = "0", //public string rewatched_times { get; set; } // int
                                            notes = "", //public object notes { get; set; } //string
                                            notes_present = false, //public object notes_present { get; set; } //bool
                                            status = "",
                                            id = "",
                                            @private = false,
                                            rewatching = false, //public object rewatching { get; set; } //bool
                                            anime = a,
                                            rating = new Rating
                                            {
                                                type = "",
                                                value = "0.0",
                                                valueAsDouble = 0.0,
                                            },
                                        };

                                        Consts.LoggedInUser.animeLibrary.Favourites.Add(libraryObject);
                                    }
                                    break;
                                case APIResponse.Failed:
                                    Debug.WriteLine("Generating Library(): Favourites Failed");
                                    break;
                            }
                        }

                        // Finally, Save Favorites
                        Debug.WriteLine("Generating Library: favourites");

                        Consts.LoggedInUser.animeLibrary.lastPulled = DateTime.Now;
                        bool savingResult = await Consts.LoggedInUser.Save();
                        return savingResult;
                    }

                    public static async Task<APICompletedEventArgs> AllUserInfo(string userName, bool fireEventOff = true)
                    {
                        APICompletedEventArgs args;
                        UserInfo userinfo = new UserInfo();

                        Debug.WriteLine("AllUserInfo(): Getting UserInfo");
                        APICompletedEventArgs userEventArgs = await HummingbirdAPI.V1API.Get.UserInfo(userName, false);
                        switch (userEventArgs.Result)
                        {
                            case APIResponse.Successful:
                                Debug.WriteLine("AllUserInfo(): UserInfo Recieved Successfully!");
                                userinfo = (userEventArgs.ResultObject as Data_Structures.API_Classes.UserInfo);
                                break;
                            default:
                                Debug.WriteLine("AllUserInfo(): UserInfo Failed");
                                break;
                        }

                        Debug.WriteLine("Getting Undocumented UserInfo");
                        APICompletedEventArgs undoc_userEventArgs = await HummingbirdAPI.V2API.Get.UserInfo(userName, false);
                        switch (userEventArgs.Result)
                        {
                            case APIResponse.Successful:
                                Debug.WriteLine("AllUserInfo(): Undocumented_UserInfo Recieved Successfully!");
                                Data_Structures.API_Classes.UserInfo undoc_UserInfo = (undoc_userEventArgs.ResultObject as Data_Structures.API_Classes.UserInfoRootObject).user_info;
                                userinfo.AddUnDocumentedToDocumented(undoc_UserInfo);
                                break;
                            default:
                                Debug.WriteLine("AllUserInfo(): Undocumented_UserInfo Failed");
                                break;
                        }

                        Debug.WriteLine("AllUserInfo(): Exiting");
                        if (userEventArgs.Result == APIResponse.Successful &&
                            undoc_userEventArgs.Result == APIResponse.Successful)
                        {
                            args = new APICompletedEventArgs(APIResponse.Successful, APIType.UserInfo, userinfo);
                            if (fireEventOff)
                            {
                                APICompletedEventHandler(args.UserState, args);
                            }
                            return args;
                        }
                        else
                        {
                            args = new APICompletedEventArgs(APIResponse.Failed, APIType.UserInfo);
                            if (fireEventOff)
                            {
                                APICompletedEventHandler(args.UserState, args);
                            }
                            return args;
                        }
                    }
                }

                public static async Task<APICompletedEventArgs> UserInfo(string username, bool fireEventOff = true)
                {
                    // Create the arguments for future usage
                    APICompletedEventArgs args;

                    /// --------------------------------------------------- ///
                    /// Once _anime string is API Compliant, begin the GET  ///
                    /// --------------------------------------------------- ///
                    Debug.WriteLine("UserInfo(): Entering");

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message                                                     
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hbrd-v1.p.mashape.com/users/" + username); //http://hummingbird.me/user_infos/killerrin

                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    try
                    {
                        // Send the request to the server
                        HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                        if (response.IsSuccessStatusCode)
                        {
                            // Just as an example I'm turning the response into a string here
                            string responseAsString = await response.Content.ReadAsStringAsync();

                            //Debug.WriteLine(responseAsString);

                            JObject o = JObject.Parse(responseAsString); // This would be the string you defined above
                            Data_Structures.API_Classes.UserInfo userInfo = JsonConvert.DeserializeObject<Data_Structures.API_Classes.UserInfo>(o.ToString()); ;

                            Debug.WriteLine("GetAnime(): Exiting");

                            args = new APICompletedEventArgs(APIResponse.Successful, APIType.UserInfo, userInfo);
                            if (fireEventOff)
                            {
                                APICompletedEventHandler(args.UserState, args);
                            }
                            return args;
                        }
                    }
                    catch (Exception)
                    {
                        args = new APICompletedEventArgs(APIResponse.Failed, APIType.UserInfo);
                        if (fireEventOff)
                        {
                            APICompletedEventHandler(args.UserState, args);
                        }
                        return args;
                    }

                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.UserInfo);
                    if (fireEventOff)
                    {
                        APICompletedEventHandler(args.UserState, args);
                    }
                    return args;
                }

                public static async Task Anime(string _animeSlug)
                {
                    // Create the arguments for future usage
                    APICompletedEventArgs args;

                    /// ------------------------------------------------ ///
                    /// Double Check if _anime string is API Compliant.  ///
                    /// ------------------------------------------------ ///
                    string animeSearchTerm = ConvertToAPIConpliantString(_animeSlug, '-');

                    /// --------------------------------------------------- ///
                    /// Once _anime string is API Compliant, begin the GET  ///
                    /// --------------------------------------------------- ///
                    Debug.WriteLine("GetAnime(): Entering: " + animeSearchTerm);

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, CreateHummingbirdUrl("anime/" + animeSearchTerm + "/", HummingbirdAPILevel.Version1)); //"https://hbrd-v1.p.mashape.com/anime/" + animeSearchTerm); //"http://hummingbird.me/search?query="+uri);//

                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        // Just as an example I'm turning the response into a string here
                        string responseAsString = await response.Content.ReadAsStringAsync();

                        //Debug.WriteLine(responseAsString);

                        JObject o = JObject.Parse(responseAsString); // This would be the string you defined above
                        Anime animeObject = JsonConvert.DeserializeObject<Anime>(o.ToString()); ;

                        Debug.WriteLine("GetAnime(): Exiting");

                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.GetAnime);
                        APICompletedEventHandler(animeObject, args);
                        return;
                    }
                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.GetAnime);
                    APICompletedEventHandler(new Anime(), args);
                    return;
                }
                public static async Task<APICompletedEventArgs> Favourites(string username, bool fireEvent = true)
                {
                    // Create the arguments for future usage
                    APICompletedEventArgs args;

                    /// --------------------------------------------------- ///
                    /// Once _anime string is API Compliant, begin the GET  ///
                    /// --------------------------------------------------- ///
                    Debug.WriteLine("GetFavourites(): Entering: ");

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, CreateHummingbirdUrl("users/" + username + "/favorite_anime", HummingbirdAPILevel.Version1, true)); //"https://hbrd-v1.p.mashape.com/users/" + username + "/favorite_anime"); //"http://hummingbird.me/search?query="+uri);//

                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        // Just as an example I'm turning the response into a string here
                        string responseAsString = await response.Content.ReadAsStringAsync();

                        //Debug.WriteLine(responseAsString);

                        List<Anitro.Data_Structures.API_Classes.Anime> animeList = Parsers.ParseFavouritesResult(responseAsString);

                        foreach (Anitro.Data_Structures.API_Classes.Anime a in animeList)
                        {
                            Debug.WriteLine("GetFavourites(): "+ a.title);
                        }

                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.GetFavourites, animeList);

                        if (fireEvent)
                        {
                            APICompletedEventHandler(animeList, args);
                        }
                        return args;
                    }

                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.GetFavourites);
                    if (fireEvent)
                    {
                        APICompletedEventHandler(args.UserState, args);
                    }
                     return args;
                }
                public static async Task SearchAnime(string searchTerm)
                {
                    Debug.WriteLine("Entering");
                    APICompletedEventArgs args;

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message https://hummingbirdv1.p.mashape.com/search/anime?query=search&auth_token=token
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, CreateHummingbirdUrl("search/anime?query=" + searchTerm, HummingbirdAPILevel.Version1, true)); //"https://hbrd-v1.p.mashape.com/search/anime?query=" + searchTerm);// + "&auth_token=" + Consts.LoggedInUser.AuthToken);
                    
                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        // Just as an example I'm turning the response into a string here
                        string responseAsString = await response.Content.ReadAsStringAsync();

                        //Debug.WriteLine(responseAsString + "\n\n");

                        List<Anitro.Data_Structures.API_Classes.V1.Anime> animeList = Parsers.ParseSearchResult(responseAsString);

                        foreach (Anitro.Data_Structures.API_Classes.V1.Anime a in animeList)
                        {
                            Debug.WriteLine(a.title);
                        }

                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.Search);
                        APICompletedEventHandler(animeList, args);
                        return;
                    }

                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.Search);
                    APICompletedEventHandler(args.UserState, args);
                    return;
                }

                public static async Task StatusFeed(string userName, int pageIndex = 1)
                {
                    Debug.WriteLine("GetStatusFeed(): Entering");

                    // Create the arguments for future usage
                    APICompletedEventArgs args;

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://hbrd-v1.p.mashape.com/users/" + userName + "/feed?page="+pageIndex); // http://hummingbird.me/api/v1/users/killerrin/feed?page=1
                    Debug.WriteLine("GetStatusFeed(): Getting: " + requestMessage.RequestUri.OriginalString);

                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("GetStatusFeed(): Response successful");
                        // Just as an example I'm turning the response into a string here
                        string responseAsString = await response.Content.ReadAsStringAsync();

                        //Debug.WriteLine(responseAsString);
                        //Console.WriteLine(responseAsString);

                        responseAsString = "{\"status_feed\":" + responseAsString + "}";

                        Debug.WriteLine("GetStatusFeed(): Parsing Library To List");
                        //Debug.WriteLine(responseAsString);
                        JObject o = JObject.Parse(responseAsString); // This would be the string you defined above
                        Anitro.Data_Structures.API_Classes.V1.ActivityFeed activityFeed = JsonConvert.DeserializeObject<Anitro.Data_Structures.API_Classes.V1.ActivityFeed>(o.ToString());


                        bool parseResult = Parsers.ParseActivityFeed(activityFeed);
                        //Consts.activityFeed = aFO;



                        //Debug.WriteLine("GetActivityFeed(): Exiting Successful");
                        //byte[] data = Encoding.UTF8.GetBytes(responseAsString);
                        //return Encoding.UTF8.GetString(data, 0, data.Length);
                        //return responseAsString;
                        if (parseResult)
                        {
                            Debug.WriteLine("GetActivityFeed(): Exiting Success!");
                            args = new APICompletedEventArgs(APIResponse.Successful, APIType.GetActivityFeed);
                            APICompletedEventHandler(args.UserState, args);
                            return;
                        }
                    }

                    Debug.WriteLine("GetActivityFeed(): Exiting Failed");
                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.GetActivityFeed);
                    APICompletedEventHandler(args.UserState, args);
                    return;
                }
                public static async Task<string> Library(LibrarySelection status, bool calledFromStreamline = false)
                {
                    APICompletedEventArgs args;

                    string statusString = Anitro.Data_Structures.Library.GetStatusFromLibrarySelection(status);

                    Debug.WriteLine("GetLibrary(): Entering");

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    //"https://hummingbirdv1.p.mashape.com/users/killerrindev/library?status=currently-watching&auth_token=swmRwwrimshWG8EtjKZK";//%3Cauth_token%3E";
                    //string url = "https://hbrd-v1.p.mashape.com/users/" + Consts.LoggedInUser.Username + "/library?status=" + statusString;// + "&auth_token=" + Consts.LoggedInUser.AuthToken;
                    //Debug.WriteLine(url);
                    
                    // Add a new Request Message
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, CreateHummingbirdUrl("users/" + Consts.LoggedInUser.Username + "/library?status=" + statusString, HummingbirdAPILevel.Version1, true));

                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    //Debug.WriteLine("Test");
                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    //string responseAsString

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("GetLibrary(): Response Successful");

                        // Turn the response into a string for parsing later
                        string responseAsString = await response.Content.ReadAsStringAsync();
                        //Debug.WriteLine(responseAsString);

                        // Due to json randomness, chop off the last two characters
                        //responseAsString = responseAsString.Substring(0, responseAsString.Length - 2);

                        bool libraryParse = await Parsers.ParseLibrary(responseAsString, status);
                        //await libraryParse;

                        Debug.WriteLine("GetLibrary(): Exiting");

                        if (!calledFromStreamline)
                        {
                            args = new APICompletedEventArgs(APIResponse.Successful, APIType.GetLibrary);
                            APICompletedEventHandler(args.UserState, args);
                        }

                        return responseAsString;
                    }
                    else
                    {
                        // Do Stuff
                        Debug.WriteLine("GetLibrary(): Response Failed");

                        if (!calledFromStreamline)
                        {
                            args = new APICompletedEventArgs(APIResponse.Failed, APIType.GetLibrary);
                            APICompletedEventHandler(args.UserState, args);
                        }

                        return "";
                    }
                }
            }
            public static class Post
            {
                public static async Task Login (string userMail, string password)
                {
                    APICompletedEventArgs args;
                    if (userMail == "" || password == "")
                    {
                        args = new APICompletedEventArgs(APIResponse.InfoNotEntered, APIType.Login);
                        APICompletedEventHandler(args.UserState, args);
                        return;
                    }
                    if (userMail.Contains("@"))
                    {
                        args = new APICompletedEventArgs(APIResponse.NotSupported, APIType.Login);
                        APICompletedEventHandler(args.UserState, args);
                        return;
                    }

                    Debug.WriteLine("Entering: Post.Login()");

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, CreateHummingbirdUrl("users/authenticate/", HummingbirdAPILevel.Version1, true)); //"https://hbrd-v1.p.mashape.com/users/authenticate"); //"http://httpbin.org/post");

                    // Add our custom headers
                    requestMessage.Headers.Add("accept", "application/json"); //"accept"
                    //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    bool emailLogin;

                    // Determine Username or Email
                    if (userMail.Contains("@"))
                    {
                        emailLogin = true;

                        requestMessage.Content = new FormUrlEncodedContent(new[]
                                                {
                                                    new KeyValuePair<string,string>("email", userMail),
                                                    new KeyValuePair<string,string>("password", password)
                                                });
                    }
                    else
                    {
                        emailLogin = false;

                        requestMessage.Content = new FormUrlEncodedContent(new[]
                                                {
                                                    new KeyValuePair<string,string>("username", userMail),
                                                    new KeyValuePair<string,string>("password", password)
                                                });
                    }


                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        // Grab the string and grab the content
                        string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                        //Parse the responseAsString to remove ""'s
                        string _authToken = "";
                        char[] txtarr = responseAsString.ToCharArray();
                        foreach (char c in txtarr)
                        {
                            switch (c)
                            {
                                case '"':
                                    break;
                                default:
                                    _authToken += c;
                                    break;
                            }
                        }

                        // Grab the Username and/or Email and then Login to the user
                        string userName = "";
                        string email = "";
                        if (emailLogin)
                        {
                            // Set the EmailAddress to the user
                            email = userMail;

                            // Send a request to get username
                            userName = "me"; //userName = await GetUsernameFromServer(_authToken);
                        }
                        else
                        {
                            // Set the username to the user
                            userName = userMail;
                        }

                        Consts.LoggedInUser.Login(userName, _authToken, password, email);


                        //APICompletedEventArgs userEventArgs = await HummingbirdAPI.V1API.Get.Streamlining.AllUserInfo(userName, false);
                        //switch (userEventArgs.Result)
                        //{
                        //    case APIResponse.Successful:
                        //        Debug.WriteLine("AllUserInfo(): UserInfo Recieved Successfully!");
                        //        Consts.LoggedInUser.UserInfo = (userEventArgs.ResultObject as Data_Structures.API_Classes.UserInfo);
                        //        break;
                        //    default:
                        //        Debug.WriteLine("AllUserInfo(): UserInfo Failed");
                        //        break;
                        //}
                                  

                        Debug.WriteLine("Exiting: Post.PostLogin()");

                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.Login);
                        APICompletedEventHandler(args.UserState, args);

                        return;
                    }
                    else
                    {
                        // Do Stuff
                        if (!Consts.IsConnectedToInternet())
                        {
                            Debug.WriteLine("Network Error: PostLogin()");

                            args = new APICompletedEventArgs(APIResponse.NetworkError, APIType.Login);
                            APICompletedEventHandler(args.UserState, args);
                            return;
                        }
                        else if (await response.Content.ReadAsStringAsync() == "{\"error\":\"Invalid credentials\"}")
                        {
                            Debug.WriteLine("Invalid Login Credidentials: PostLogin()");

                            args = new APICompletedEventArgs(APIResponse.InvalidCredentials, APIType.Login);
                            APICompletedEventHandler(args.UserState, args);
                            return;
                        }
                        else
                        {
                            Debug.WriteLine("Error connecting to server: PostLogin()");

                            args = new APICompletedEventArgs(APIResponse.ServerError, APIType.Login);
                            APICompletedEventHandler(args.UserState, args);
                            return;
                        }
                    }
                }
                public static async Task StatusUpdate(string _text)
                {
                    Debug.WriteLine("PostStatusUpdate(): Entering");

                    APICompletedEventArgs args;

                    /// ------------------------------------------------ ///
                    /// Double Check if _anime string is API Compliant.  ///
                    /// ------------------------------------------------ ///
                    string text = _text; // ConvertToAPIConpliantString(_text, '+');

                    /// --------------------------------------------------- ///
                    /// Once _text string is API Compliant, begin the POST  ///
                    /// --------------------------------------------------- ///

                    //Debug.WriteLine("Creating Client");
                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    //Debug.WriteLine("Making Custom Message");
                    // Add a new Request Message                                                                               Consts.settings.userName
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://hummingbird.me/users/" + Consts.LoggedInUser.Username + "/comment.json"); //"http://httpbin.org/post");

                    //Debug.WriteLine("Setting Headers");
                    // Add our custom headers
                    requestMessage.Headers.Add("accept", "application/json"); //"accept"
                    requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    //Debug.WriteLine("Setting Content");
                    // Add our Content
                    requestMessage.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("auth_token", Consts.LoggedInUser.AuthToken),//Consts.settings.auth_token),
                    new KeyValuePair<string,string>("comment", text),
                });

                    //Debug.WriteLine("Sending Message");
                    // Send the request to the server
                    HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                    //Debug.WriteLine("Parsing the Response");
                    if (response.IsSuccessStatusCode)
                    {
                        // Grab the string and grab the content
                        string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                        DateTime dT = DateTime.Now;
                        Anitro.Data_Structures.API_Classes.V1.ActivityFeedObject temp = new Anitro.Data_Structures.API_Classes.V1.ActivityFeedObject
                        {
                            storyImage = Consts.LoggedInUser.AvatarURL,//Consts.settings.userAvatar,
                            header = Consts.LoggedInUser.Username,//Consts.settings.userName,
                            content = _text,
                            timeStamp = dT.Date.Year + "-" + dT.Date.Month + "-" + dT.Date.Day + " at " +
                                        dT.TimeOfDay.Hours + ":" + dT.TimeOfDay.Minutes + ":" + dT.TimeOfDay.Seconds
                        };

                        Consts.LoggedInUser.activityFeed.Insert(0, temp);

                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.PostActivityFeed);
                        APICompletedEventHandler(args.UserState, args);
                        return;
                    }

                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.PostActivityFeed);
                    APICompletedEventHandler(args.UserState, args);
                    return;
                }

                public static async Task<APICompletedEventArgs> LibraryUpdate(LibraryObject libraryObject, bool fireEvent = true)
                {
                    Debug.WriteLine("PostLibraryUpdate(LibraryObject libraryObject): Entering");
                    APICompletedEventArgs args;

                    if (libraryObject.notes == null) { libraryObject.notes = ""; }

                    Debug.WriteLine("Posting: ");
                    Debug.WriteLine(libraryObject.anime.slug);
                    Debug.WriteLine(libraryObject.status);
                    Debug.WriteLine(libraryObject.@private.ToString());
                    Debug.WriteLine(libraryObject.rating.value);
                    Debug.WriteLine(Convert.ToInt32(libraryObject.rewatched_times));
                    Debug.WriteLine(libraryObject.notes.ToString());
                    Debug.WriteLine(Convert.ToInt32(libraryObject.episodes_watched));
                    Debug.WriteLine(false);

                    args = await LibraryUpdate(libraryObject.anime.slug,
                                                    libraryObject.status,
                                                    libraryObject.@private.ToString(),
                                                    libraryObject.rating.value,
                                                    Convert.ToInt32(libraryObject.rewatched_times),
                                                    libraryObject.notes.ToString(),
                                                    Convert.ToInt32(libraryObject.episodes_watched),
                                                    false,
                                                    false);

                    if (args.Result == APIResponse.Successful)
                    {
                        //args = new APICompletedEventArgs(APIResponse.Successful, APIType.PostLibraryUpdate);

                        //AnimePage.dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        //{
                        if (fireEvent) { APICompletedEventHandler(args.UserState, args); } 
                        //});
                    }
                    else
                    {
                        //args = new APICompletedEventArgs(APIResponse.Failed, APIType.PostLibraryUpdate);
                        if (fireEvent) { APICompletedEventHandler(args.UserState, args); }  
                    }
                    return args;
                }
                public static async Task<APICompletedEventArgs> LibraryUpdate(string _slug, string status, string privacy, string rating, int rewatchedTimes, string notes, int episodesWatched, bool incrimentEpisodes, bool fireEventWhenComplete = true)
                {
                    Debug.WriteLine("PostLibraryUpdate(): Entering");
                    APICompletedEventArgs args;

                    /// ------------------------------------------------ ///
                    /// Double Check if _anime string is API Compliant.  ///
                    /// ------------------------------------------------ ///
                    string anime = ConvertToAPIConpliantString(_slug, '-');

                    /// --------------------------------------------------- ///
                    /// Once _anime string is API Compliant, begin the GET  ///
                    /// --------------------------------------------------- ///
                    /// 

                    try
                    {
                        // Create a client
                        HttpClient httpClient = new HttpClient();

                        // Add a new Request Message
                        string uri = "https://hbrd-v1.p.mashape.com/libraries/" + anime;
                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

                        // Add our custom headers
                        requestMessage.Headers.Add("accept", "application/json"); //"accept"
                        requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                        LibrarySelection libSel = Consts.LoggedInUser.animeLibrary.FindWhereExistsInLibrary(anime);

                        if (rating == (Consts.LoggedInUser.animeLibrary.GetObjectInLibrary(libSel, anime)).rating.value)
                        {
                            requestMessage.Content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string,string>("auth_token", Consts.LoggedInUser.AuthToken),//Consts.settings.auth_token),
                                new KeyValuePair<string,string>("status", status),
                                new KeyValuePair<string,string>("privacy", privacy),
                                new KeyValuePair<string,string>("rewatched_times", rewatchedTimes.ToString()),
                                new KeyValuePair<string,string>("notes", notes),
                                new KeyValuePair<string,string>("episodes_watched", episodesWatched.ToString()),
                                new KeyValuePair<string,string>("increment_episodes", (incrimentEpisodes.ToString()).ToLower())
                            });
                        }
                        else
                        {
                            // Add our Content
                            requestMessage.Content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string,string>("auth_token", Consts.LoggedInUser.AuthToken),//Consts.settings.auth_token),
                                new KeyValuePair<string,string>("status", status),
                                new KeyValuePair<string,string>("privacy", privacy),
                                new KeyValuePair<string,string>("rating", rating), // none = None Selected, 0-2 = Unhappy, 3 = Neutral, 4-5 = Happy
                                new KeyValuePair<string,string>("rewatched_times", rewatchedTimes.ToString()),
                                new KeyValuePair<string,string>("notes", notes),
                                new KeyValuePair<string,string>("episodes_watched", episodesWatched.ToString()),
                                new KeyValuePair<string,string>("increment_episodes", (incrimentEpisodes.ToString()).ToLower())
                            });
                        }
                        
                        // Set Timeout
                        httpClient.Timeout = new TimeSpan(0, 0, 0, 2, 0);

                        // Send the request to the server
                        Debug.WriteLine("Sending Server Request");

                        // await the response if we care about a reply.
                        Task<HttpResponseMessage> response = httpClient.SendAsync(requestMessage);
                        // ...thankfully, we dont care.

                        if (true)//response.IsSuccessStatusCode)
                        {
                            Debug.WriteLine("Response Succeded");
                            //string responseAsString = await response.Content.ReadAsStringAsync();//.Result;
                            //Debug.WriteLine(responseAsString);
                            Debug.WriteLine("PostLibraryUpdate(): Exiting Succeeded");

                            if (fireEventWhenComplete)
                            {
                                args = new APICompletedEventArgs(APIResponse.Successful, APIType.PostLibraryUpdate);
                                APICompletedEventHandler(args.UserState, args);
                            }
                            else { args = new APICompletedEventArgs(); }
                            return args;
                        }

                        Debug.WriteLine("PostLibraryUpdate(): Exiting Failed");
                        if (fireEventWhenComplete)
                        {
                            args = new APICompletedEventArgs(APIResponse.Failed, APIType.PostLibraryUpdate);
                            APICompletedEventHandler(args.UserState, args);
                        }
                        else { args = new APICompletedEventArgs(); }
                        return args;
                    }
                    catch(Exception)
                    {
                        Debug.WriteLine("PostLibraryUpdate(): Exiting Error");
                        if (fireEventWhenComplete)
                        {
                            args = new APICompletedEventArgs(APIResponse.Failed, APIType.PostLibraryUpdate);
                            APICompletedEventHandler(args.UserState, args);
                        }
                        else { args = new APICompletedEventArgs(); }
                        return args;
                    }
                }

                public static async Task<APICompletedEventArgs> LibraryRemove(string _anime, bool fireOffEvent = true)
                {
                    APICompletedEventArgs args;

                    /// ------------------------------------------------ ///
                    /// Double Check if _anime string is API Compliant.  ///
                    /// ------------------------------------------------ ///
                    string anime = ConvertToAPIConpliantString(_anime, '-');

                    /// --------------------------------------------------- ///
                    /// Once _anime string is API Compliant, begin the GET  ///
                    /// --------------------------------------------------- ///

                    Debug.WriteLine("LibraryRemove(): Entering");

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://hbrd-v1.p.mashape.com/libraries/" + anime + "/remove");

                    // Add our custom headers
                    requestMessage.Headers.Add("accept", "application/json"); //"accept"
                    requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    // Add our Content
                    requestMessage.Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string,string>("auth_token", Consts.LoggedInUser.AuthToken),//Consts.settings.auth_token),
                    });

                    // Send the request to the server
                    // add await to await response
                    Task<HttpResponseMessage> responseTask = httpClient.SendAsync(requestMessage);

                    if (true) //response.IsSuccessStatusCode)
                    {
                        // Just as an example I'm turning the response into a string here
                        //string responseAsString = await response.Content.ReadAsStringAsync();//.Result;

                        //Debug.WriteLine(responseAsString);

                        Debug.WriteLine("LibraryRemove(): Success!");
                        args = new APICompletedEventArgs(APIResponse.Successful, APIType.PostLibraryRemove);
                        if (fireOffEvent)
                        {
                            APICompletedEventHandler(args.UserState, args);
                        }
                        return args;
                    }

                    Debug.WriteLine("LibraryRemove(): Failed");
                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.PostLibraryRemove);
                    if (fireOffEvent)
                    {
                        APICompletedEventHandler(args.UserState, args);
                    }
                    return args;
                }
            }
        }

        public static class V2API
        {
            public static class Get
            {
                public static async Task<APICompletedEventArgs> UserInfo(string username, bool fireEventOff = true)
                {
                    // Create the arguments for future usage
                    APICompletedEventArgs args;

                    /// --------------------------------------------------- ///
                    /// Once _anime string is API Compliant, begin the GET  ///
                    /// --------------------------------------------------- ///
                    Debug.WriteLine("UserInfo(): Entering");

                    // Create a client
                    HttpClient httpClient = new HttpClient();

                    // Add a new Request Message                                                     
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://hummingbird.me/user_infos/" + username); //http://hummingbird.me/user_infos/killerrin

                    // Add our custom headers
                    //requestMessage.Headers.Add("Content-Type", "application/json");
                    //requestMessage.Headers.Add("X-Mashape-Authorization", Consts.appData.MashapeKey);

                    try
                    {
                        // Send the request to the server
                        HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                        if (response.IsSuccessStatusCode)
                        {
                            // Just as an example I'm turning the response into a string here
                            string responseAsString = await response.Content.ReadAsStringAsync();

                            //Debug.WriteLine(responseAsString);

                            JObject o = JObject.Parse(responseAsString); // This would be the string you defined above
                            Data_Structures.API_Classes.UserInfoRootObject userInfo = JsonConvert.DeserializeObject<Data_Structures.API_Classes.UserInfoRootObject>(o.ToString()); ;

                            Debug.WriteLine("GetAnime(): Exiting");

                            args = new APICompletedEventArgs(APIResponse.Successful, APIType.UserInfo, userInfo);
                            if (fireEventOff)
                            {
                                APICompletedEventHandler(args.UserState, args);
                            }
                            return args;
                        }
                    }
                    catch(Exception)
                    {
                        args = new APICompletedEventArgs(APIResponse.Failed, APIType.UserInfo);
                        if (fireEventOff)
                        {
                            APICompletedEventHandler(args.UserState, args);
                        }
                        return args;
                    }

                    args = new APICompletedEventArgs(APIResponse.Failed, APIType.UserInfo);
                    if (fireEventOff)
                    {
                        APICompletedEventHandler(args.UserState, args);
                    }
                    return args;
                }
            }

            public static class Post
            {

            }
        }

        #region Helper Methods

        private static string Domain = "http://www.hummingbird.me";
        public static string CreateHummingbirdUrl(string endpoint, HummingbirdAPILevel apiLevel = HummingbirdAPILevel.None, bool omitWWW = false)
        {
            string apiPoint;
            switch (apiLevel)
            {
                case HummingbirdAPILevel.Version1:
                    apiPoint = "/api/v1/";
                    break;
                case HummingbirdAPILevel.Version2:
                    apiPoint = "/api/v2/";
                    break;

                case HummingbirdAPILevel.None:
                default:
                    apiPoint = "/";
                    break;
            }


            string fullPath;
            if (omitWWW) fullPath = "http://" + Domain.Substring(11) + apiPoint + endpoint;
            else fullPath = Domain + apiPoint + endpoint;


            Debug.WriteLine(fullPath);
            return fullPath;
        }
        public static Uri CreateHummingbirdUri(string endpoint, HummingbirdAPILevel apiLevel = HummingbirdAPILevel.None, bool omitWWW = false)
        {
            return new Uri(CreateHummingbirdUrl(endpoint, apiLevel, omitWWW), UriKind.Absolute);
        }

        private static string ConvertToAPIConpliantString(string _text, char parseValue)
        {
            Debug.WriteLine("ConvertToAPIConpliantString()");
            string text = _text;
            text.ToLower();
            char[] txtarr = text.ToCharArray();
            text = "";
            foreach (char c in txtarr)
            {
                switch (c)
                {
                    case ' ':
                        text += parseValue;
                        break;
                    default:
                        text += c;
                        break;
                }
            }

            return text;
        }
        #endregion
    }
}
