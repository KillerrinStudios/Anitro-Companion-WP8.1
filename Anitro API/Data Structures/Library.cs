using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Anitro.APIs;
using Anitro.APIs.Events;

using Anitro.Data_Structures.API_Classes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;

namespace Anitro.Data_Structures
{
    public class Library
    {
        // Library Events
        public event LibraryLoadedEventHandler LibraryLoadedEventHandler;
        public event LibraryUpdatedEventHandler LibraryUpdatedEventHandler;

        // Library Data
        public string Owner; // { get; set; }
        public DateTime lastPulled;

        public ObservableCollection<LibraryObject> CurrentlyWatching;
        public ObservableCollection<LibraryObject> Completed;
        public ObservableCollection<LibraryObject> PlanToWatch;
        public ObservableCollection<LibraryObject> OnHold;
        public ObservableCollection<LibraryObject> Dropped;

        public ObservableCollection<LibraryObject> Favourites;
        public ObservableCollection<LibraryObject> Recent;

        public bool savingOrUpdatingLibrary = false;

        public Library(string _owner)
        {
            Owner               = _owner;
            lastPulled          = new DateTime();

            CurrentlyWatching   = new ObservableCollection<LibraryObject> { };
            Completed           = new ObservableCollection<LibraryObject> { };
            PlanToWatch         = new ObservableCollection<LibraryObject> { };
            OnHold              = new ObservableCollection<LibraryObject> { };
            Dropped             = new ObservableCollection<LibraryObject> { };
            Favourites          = new ObservableCollection<LibraryObject> { };

            Recent              = new ObservableCollection<LibraryObject> { };
        }
        public Library(Library lib)
        {
            Owner               = lib.Owner;
            lastPulled          = lib.lastPulled;

            CurrentlyWatching   = lib.CurrentlyWatching;
            Completed           = lib.Completed;        
            PlanToWatch         = lib.PlanToWatch;      
            OnHold              = lib.OnHold;           
            Dropped             = lib.Dropped;          
            Favourites          = lib.Favourites;

            Recent              = lib.Recent;
        }

        public void ClearLibrary(LibrarySelection selection, bool save = false)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    CurrentlyWatching = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.PlanToWatch:
                    PlanToWatch = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Completed:
                    Completed = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.OnHold:
                    OnHold = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Dropped:
                    Dropped = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Favourites:
                    Favourites = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.Recent:
                    Recent = new ObservableCollection<LibraryObject>();
                    break;
                case LibrarySelection.APISupported:
                    ClearLibrary(LibrarySelection.CurrentlyWatching);
                    ClearLibrary(LibrarySelection.PlanToWatch);
                    ClearLibrary(LibrarySelection.Completed);
                    ClearLibrary(LibrarySelection.OnHold);
                    ClearLibrary(LibrarySelection.Dropped);
                    break;
                case LibrarySelection.All:
                    ClearLibrary(LibrarySelection.APISupported);
                    ClearLibrary(LibrarySelection.Favourites);
                    ClearLibrary(LibrarySelection.Recent);
                    break;
            }

            if (save) Consts.LoggedInUser.Save();
        }

        public bool IsEveryLibraryEmpty(LibrarySelection libSel)
        {
            switch (libSel)
            {
                case LibrarySelection.CurrentlyWatching:
                    return CurrentlyWatching.Count == 0;
                case LibrarySelection.PlanToWatch:
                    return PlanToWatch.Count == 0;
                case LibrarySelection.Completed:
                    return Completed.Count == 0;
                case LibrarySelection.OnHold:
                    return OnHold.Count == 0;
                case LibrarySelection.Dropped:
                    return Dropped.Count == 0;
                case LibrarySelection.Favourites:
                    return Favourites.Count == 0;
                case LibrarySelection.Recent:
                    return Recent.Count == 0;

                case LibrarySelection.APISupported:
                    return (IsEveryLibraryEmpty(LibrarySelection.CurrentlyWatching) &&
                            IsEveryLibraryEmpty(LibrarySelection.PlanToWatch) &&
                            IsEveryLibraryEmpty(LibrarySelection.Completed) &&
                            IsEveryLibraryEmpty(LibrarySelection.OnHold) &&
                            IsEveryLibraryEmpty(LibrarySelection.Dropped));

                case LibrarySelection.All:
                    return (IsEveryLibraryEmpty(LibrarySelection.APISupported) &&
                            IsEveryLibraryEmpty(LibrarySelection.Favourites));

                case LibrarySelection.None:
                default:
                    break;
            }
            return true;
        }

        public ObservableCollection<LibraryObject> GetLibraryInSingleCollection(LibrarySelection libSel, bool addFavourites = true)
        {
            ObservableCollection<LibraryObject> allLibrary = new ObservableCollection<LibraryObject>() { };
            switch (libSel)
            {
                case LibrarySelection.APISupported:
                    foreach (LibraryObject lO in CurrentlyWatching) allLibrary.Add(lO);
                    foreach (LibraryObject lO in Completed) allLibrary.Add(lO);
                    foreach (LibraryObject lO in PlanToWatch) allLibrary.Add(lO);
                    foreach (LibraryObject lO in OnHold) allLibrary.Add(lO);
                    foreach (LibraryObject lO in Dropped) allLibrary.Add(lO);
                    if (addFavourites) foreach (LibraryObject lO in Favourites) allLibrary.Add(lO);
                    return allLibrary;
                case LibrarySelection.All:
                    ObservableCollection<LibraryObject> apiLib = GetLibraryInSingleCollection(LibrarySelection.APISupported);
                    foreach (LibraryObject lO in apiLib) allLibrary.Add(lO);
                    foreach (LibraryObject lO in Recent) allLibrary.Add(lO);
                    return allLibrary;
            }
            return new ObservableCollection<LibraryObject> { };
        }


        #region Storage Tools
        #region Json Tools
        public string ThisToJson()
        {
            List<LibraryObject> libObj = new List<LibraryObject> { };
            foreach (LibraryObject o in CurrentlyWatching) { libObj.Add(o); }
            foreach (LibraryObject o in Completed) { libObj.Add(o); }
            foreach (LibraryObject o in PlanToWatch) { libObj.Add(o); }
            foreach (LibraryObject o in OnHold) { libObj.Add(o); }
            foreach (LibraryObject o in Dropped) { libObj.Add(o); }

            List<LibraryObject> favObj = new List<LibraryObject> { };
            foreach (LibraryObject o in Favourites) { favObj.Add(o); }

            List<LibraryObject> resObj = new List<LibraryObject> { };
            foreach (LibraryObject o in Recent) { resObj.Add(o); }

            LibraryList lL = new LibraryList { };
            lL.library = libObj;
            lL.favourites = favObj;
            lL.recent = resObj;

            return JsonConvert.SerializeObject(lL);
        }
        public void JsonToThis(string json)
        {
            JObject jObject = JObject.Parse(json);
            LibraryList libraryList = JsonConvert.DeserializeObject<LibraryList>(jObject.ToString());

            foreach (LibraryObject temp in libraryList.library)
            {
                switch (temp.status)
                {
                    case "currently-watching":
                        CurrentlyWatching.Add(temp);
                        break;
                    case "completed":
                        Completed.Add(temp);
                        break;
                    case "plan-to-watch":
                        PlanToWatch.Add(temp);
                        break;
                    case "on-hold":
                        OnHold.Add(temp);
                        break;
                    case "dropped":
                        Dropped.Add(temp);
                        break;
                }
            }

            foreach (LibraryObject temp in libraryList.favourites)
            {
                Favourites.Add(temp);
            }

            foreach (LibraryObject temp in libraryList.recent)
            {
                Recent.Add(temp);
            }
        }
        #endregion

        public async Task LoadFromStorageOrServer(bool forceServerGrab = false)
        {
            try
            {
                HummingbirdAPI.APICompletedEventHandler += HummingbirdAPI_LibraryLoaded;
                await HummingbirdAPI.V1API.Get.Streamlining.AllLibraries(forceServerGrab);
            }
            catch (Exception) { }
        }
        void HummingbirdAPI_LibraryLoaded(object sender, APICompletedEventArgs e)
        {
            if (e.Type != APIType.LoadLibrary) return;
            LibraryLoadedEventArgs args;

            switch (e.Result)
            {
                case APIResponse.Successful:
                    Debug.WriteLine("HummingbirdAPI_LibraryLoaded(): Library Loaded Successsfully!");
                    args = new LibraryLoadedEventArgs(APIResponse.Successful, APIType.LoadLibrary,
                                                      new Exception(), false, this);
                    break;
                case APIResponse.Failed:
                    Debug.WriteLine("HummingbirdAPI_LibraryLoaded(): Library Failed to Load");
                    args = new LibraryLoadedEventArgs(APIResponse.Failed, APIType.LoadLibrary,
                                                      new Exception(), false, this);
                    break;
                default:
                    args = new LibraryLoadedEventArgs();
                    break;
            }

            // Remove the Event Handler
            HummingbirdAPI.APICompletedEventHandler -= HummingbirdAPI_LibraryLoaded;

            LibraryLoadedEventHandler(args.UserState, args);
            return;
        }
        #endregion

        #region UpdateLibrary
        #endregion

        #region Helper Methods
        public bool AddToRecent(LibraryObject temp, bool save = true)
        {
            Debug.WriteLine("AddToRecent(" + temp.anime.slug + ", " + save.ToString() + ")" );

            // First check if it is already in the Recent
            if (DoesExistInLibrary(LibrarySelection.Recent, temp))
            {
                // If it is, Remove the item from the Recent
                for (int i = 0; i < Recent.Count; i++)
                {
                    if (temp.anime.slug == Recent[i].anime.slug) { Recent.RemoveAt(i); break; }
                }
            }

            // Add the item to the Recent at Index 0
            Recent.Insert(0, temp);
            if (Recent.Count > 9) { Recent.RemoveAt(Recent.Count - 1); }

            // If we're told to save, then save
            if (save) Consts.LoggedInUser.Save();
            return true;
        }

        #region Library
        public bool AddToLibrary(LibrarySelection selection, LibraryObject temp, bool insertAtZero = true, bool save = false)
        {
            if (!DoesExistInLibrary(selection, temp))
            {
                Debug.WriteLine("AddToLibrary(): Entering : Does Not Exist");
                switch (selection)
                {
                    case LibrarySelection.CurrentlyWatching:
                        if (insertAtZero) CurrentlyWatching.Insert(0, temp);
                        else CurrentlyWatching.Add(temp);

                        //if (save) Storage.SaveAnimeLibrary("currently-watching");
                        break;
                    case LibrarySelection.PlanToWatch:
                        if (insertAtZero) PlanToWatch.Insert(0, temp);
                        else PlanToWatch.Add(temp);

                        //if (save) Storage.SaveAnimeLibrary("plan-to-watch");
                        break;
                    case LibrarySelection.Completed:
                        if (insertAtZero) Completed.Insert(0, temp);
                        else Completed.Add(temp);

                        //if (save) Storage.SaveAnimeLibrary("completed");
                        break;
                    case LibrarySelection.OnHold:
                        if (insertAtZero) OnHold.Insert(0, temp);
                        else OnHold.Add(temp);

                        //if (save) Storage.SaveAnimeLibrary("on-hold");
                        break;
                    case LibrarySelection.Dropped:
                        if (insertAtZero) Dropped.Insert(0, temp);
                        else Dropped.Add(temp);

                        //if (save) Storage.SaveAnimeLibrary("dropped");
                        break;
                    case LibrarySelection.Favourites:
                        AddToFavourites(temp, save);
                        break;
                    case LibrarySelection.None: Debug.WriteLine("AddToLibrary(): Exiting"); return false;
                    default: Debug.WriteLine("AddToLibrary(): Exiting"); return false;
                }

                UpdateFavourites(temp, false);
                if (save)
                    Consts.LoggedInUser.Save();
            
                Debug.WriteLine("AddToLibrary(): Exiting");
                return true;
            }
            else
            {
                Debug.WriteLine("AddToLibrary(): Entering : Exists");
                UpdateLibrary(selection, temp, save);
                return true;
            }
        }
        public bool UpdateLibrary(LibrarySelection selection, LibraryObject temp, bool save = true)
        {
            if (selection == LibrarySelection.None) return false;

            if (DoesExistInLibrary(selection, temp))
            {
                Debug.WriteLine("UpdateLibrary(): Entering : Exists");

                bool result = false;
                switch (selection)
                {
                    case LibrarySelection.CurrentlyWatching:
                        for (int i = 0; i < CurrentlyWatching.Count; i++)
                        {
                            if (temp.anime.slug == CurrentlyWatching[i].anime.slug)
                            {
                                CurrentlyWatching[i] = temp;
                                result = true;
                            }
                        }
                        break;
                    case LibrarySelection.PlanToWatch:
                        for (int i = 0; i < PlanToWatch.Count; i++)
                        {
                            if (temp.anime.slug == PlanToWatch[i].anime.slug)
                            {
                                PlanToWatch[i] = temp;
                                result = true;
                            }
                        }
                        break;
                    case LibrarySelection.Completed:
                        for (int i = 0; i < Completed.Count; i++)
                        {
                            if (temp.anime.slug == Completed[i].anime.slug)
                            {
                                Completed[i] = temp;
                                result = true;
                            }
                        }
                        break;
                    case LibrarySelection.OnHold:
                        for (int i = 0; i < OnHold.Count; i++)
                        {
                            if (temp.anime.slug == OnHold[i].anime.slug)
                            {
                                OnHold[i] = temp;
                                result = true;
                            }
                        }
                        break;
                    case LibrarySelection.Dropped:
                        for (int i = 0; i < Dropped.Count; i++)
                        {
                            if (temp.anime.slug == Dropped[i].anime.slug)
                            {
                                Dropped[i] = temp;
                                result = true;
                            }
                        }
                        break;
                    case LibrarySelection.Favourites:
                        UpdateFavourites(temp, save);
                        return true;
                    case LibrarySelection.None: break;
                    default: break;
                }

                if (result)
                {
                    UpdateFavourites(temp, false);

                    if (save) Consts.LoggedInUser.Save(); //.SaveAnimeLibrary("");

                    Debug.WriteLine("UpdateLibrary(): Exiting");
                    return true;
                }
            }
            Debug.WriteLine("UpdateLibrary(): Exiting");
            return false;
        }
        public bool RemoveFromLibrary(LibrarySelection selection, LibraryObject temp, bool save = false)
        {
            if (!DoesExistInLibrary(selection, temp)) { return false; }
            Debug.WriteLine("RemoveFromLibrary(" + selection.ToString() + ", " + temp.anime.slug + "): Entering");

            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    for (int i = 0; i < CurrentlyWatching.Count; i++)
                    {
                        if (temp.anime.slug == CurrentlyWatching[i].anime.slug) {
                            CurrentlyWatching.RemoveAt(i);
                            
                            if (save) Consts.LoggedInUser.Save(); //Storage.SaveAnimeLibrary(""); //"currently-watching");
                            UpdateFavourites(temp);
                            return true;
                        }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    for (int i = 0; i < PlanToWatch.Count; i++)
                    {
                        if (temp.anime.slug == PlanToWatch[i].anime.slug) { 
                            PlanToWatch.RemoveAt(i);

                            if (save) Consts.LoggedInUser.Save(); //Storage.SaveAnimeLibrary(""); //"plan-to-watch");
                            UpdateFavourites(temp);
                            return true;  
                        }
                    }
                    break;
                case LibrarySelection.Completed:
                    for (int i = 0; i < Completed.Count; i++)
                    {
                        if (temp.anime.slug == Completed[i].anime.slug)
                        {
                            Completed.RemoveAt(i);

                            if (save) Consts.LoggedInUser.Save(); //Storage.SaveAnimeLibrary(""); //"plan-to-watch");
                            UpdateFavourites(temp);
                            return true;
                        }
                    }
                    break;
                case LibrarySelection.OnHold:
                    for (int i = 0; i < OnHold.Count; i++)
                    {
                        if (temp.anime.slug == OnHold[i].anime.slug)
                        {
                            OnHold.RemoveAt(i);

                            if (save) Consts.LoggedInUser.Save(); //Storage.SaveAnimeLibrary(""); //"plan-to-watch");
                            UpdateFavourites(temp);
                            return true;
                        }
                    }
                    break;
                case LibrarySelection.Dropped:
                    for (int i = 0; i < Dropped.Count; i++)
                    {
                        if (temp.anime.slug == Dropped[i].anime.slug)
                        {
                            Dropped.RemoveAt(i);

                            UpdateFavourites(temp);
                            if (save) Consts.LoggedInUser.Save(); //Storage.SaveAnimeLibrary(""); //"dropped");
                            return true;
                        }
                    }
                    break;
                case LibrarySelection.Favourites:
                    return RemoveFromFavourites(temp);
                case LibrarySelection.None:
                    LibrarySelection libSel = FindWhereExistsInLibrary(temp);

                    if (libSel == LibrarySelection.None) { return false; }
                    return RemoveFromLibrary(libSel, temp, save);
                default:
                    break;
            }

            Debug.WriteLine("RemoveFromLibrary(" + selection.ToString() + ", " + temp.anime.slug + "): Returning");
            return false;
        }
        #endregion

        #region Favourites
        public void AddToFavourites(LibraryObject temp, bool insertAtZero = true, bool save = true)
        {
            if (DoesExistInLibrary(LibrarySelection.Favourites, temp)) { UpdateFavourites(temp, save); }
            else if (DoesExistInLibrary(LibrarySelection.APISupported, temp))
            {
                LibrarySelection location = FindWhereExistsInLibrary(temp);
                temp = GetObjectInLibrary(location, temp.anime.slug);
            }

            Favourites.Add(temp);
            if (save) Consts.LoggedInUser.Save(); //Storage.SaveAnimeLibrary("favourites");
        }
        public void UpdateFavourites(LibraryObject temp, bool save = true)
        {
            if (!DoesExistInLibrary(LibrarySelection.Favourites, temp)) { return; }

            for (int i = 0; i < Favourites.Count; i++)
            {
                if (temp.anime.slug == Favourites[i].anime.slug)
                {
                    Favourites[i] = temp;
                    if (save) Consts.LoggedInUser.Save();
                    return;
                }
            }

        }
        public bool RemoveFromFavourites(LibraryObject temp, bool save = false)
        {
            if (!DoesExistInLibrary(LibrarySelection.Favourites, temp)) { return false; }

            for (int i = 0; i < Favourites.Count; i++)
            {
                if (temp.anime.slug == Favourites[i].anime.slug)
                {
                    Favourites.RemoveAt(i);

                    if (save) Consts.LoggedInUser.Save();
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Library Tools
        public bool SwitchLibraries(LibrarySelection switchToLibrary, LibraryObject temp, bool save = false)
        {
            LibrarySelection whereInLibrary = FindWhereExistsInLibrary(temp);
            if (whereInLibrary == LibrarySelection.None || whereInLibrary == LibrarySelection.Favourites || temp.status == "search")  // Check if its currently in any of the libraries
            {
                LibrarySelection newSelection = GetLibrarySelectionFromStatus(temp);
                if (newSelection != LibrarySelection.None && newSelection != LibrarySelection.Favourites && temp.status != "search") { AddToLibrary(newSelection, temp); } // Check if the new value is able to be added to the library
                return false;
            }
            if (DoesExistInLibrary(switchToLibrary, temp)) { return true; }

            RemoveFromLibrary(whereInLibrary, temp);
            AddToLibrary(switchToLibrary, temp);

            UpdateFavourites(temp, save);

            return true;
        }
        public LibraryObject GetObjectInLibrary(LibrarySelection selection, string slug)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    foreach (LibraryObject lO in CurrentlyWatching)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    foreach (LibraryObject lO in PlanToWatch)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Completed:
                    foreach (LibraryObject lO in Completed)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    foreach (LibraryObject lO in OnHold)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    foreach (LibraryObject lO in Dropped)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    foreach (LibraryObject lO in Favourites)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.Recent:
                    foreach (LibraryObject lO in Recent)
                    {
                        if (slug == lO.anime.slug) { return lO; }
                    }
                    break;
                case LibrarySelection.APISupported:
                    LibraryObject libObjA;

                    libObjA = GetObjectInLibrary(LibrarySelection.CurrentlyWatching, slug);
                    if (libObjA != null) return libObjA;

                    libObjA = GetObjectInLibrary(LibrarySelection.PlanToWatch, slug);
                    if (libObjA != null) return libObjA;

                    libObjA = GetObjectInLibrary(LibrarySelection.Completed, slug);
                    if (libObjA != null) return libObjA;

                    libObjA = GetObjectInLibrary(LibrarySelection.OnHold, slug);
                    if (libObjA != null) return libObjA;

                    libObjA = GetObjectInLibrary(LibrarySelection.Dropped, slug);
                    if (libObjA != null) return libObjA;

                    break;
                case LibrarySelection.All:
                    LibraryObject libObj;

                    libObj = GetObjectInLibrary(LibrarySelection.APISupported, slug);
                    if (libObj != null) return libObj;

                    libObj = GetObjectInLibrary(LibrarySelection.Favourites, slug);
                    if (libObj != null) return libObj;

                    break;

                case LibrarySelection.None:
                    break;
                default: break;
            }
            return null; // new LibraryObject();
        }

        public bool DoesExistInLibrary(LibrarySelection selection, LibraryObject temp)
        {
            return DoesExistInLibrary(selection, temp.anime.slug);
        }
        public bool DoesExistInLibrary(LibrarySelection selection, string slug)
        {
            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    foreach (LibraryObject lO in CurrentlyWatching)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    foreach (LibraryObject lO in PlanToWatch)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Completed:
                    foreach (LibraryObject lO in Completed)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    foreach (LibraryObject lO in OnHold)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    foreach (LibraryObject lO in Dropped)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    foreach (LibraryObject lO in Favourites)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.Recent:
                    foreach (LibraryObject lO in Recent)
                    {
                        if (slug == lO.anime.slug) { return true; }
                    }
                    break;
                case LibrarySelection.APISupported:
                    if (DoesExistInLibrary(LibrarySelection.CurrentlyWatching, slug)) return true;
                    if (DoesExistInLibrary(LibrarySelection.PlanToWatch, slug)) return true;
                    if (DoesExistInLibrary(LibrarySelection.Completed, slug)) return true;
                    if (DoesExistInLibrary(LibrarySelection.OnHold, slug)) return true;
                    if (DoesExistInLibrary(LibrarySelection.Dropped, slug)) return true;
                    break;
                case LibrarySelection.All:
                    if (DoesExistInLibrary(LibrarySelection.APISupported, slug)) return true;
                    if (DoesExistInLibrary(LibrarySelection.Favourites, slug)) return true;
                    break;
                case LibrarySelection.None:
                    break;
                default: break;
            }

            return false;
        }

        public LibrarySelection FindWhereExistsInLibrary(LibraryObject temp)
        {
            return FindWhereExistsInLibrary(temp.anime.slug);
        }
        public LibrarySelection FindWhereExistsInLibrary(string slug)
        {
            foreach (LibraryObject lO in CurrentlyWatching)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.CurrentlyWatching; }
            }
            foreach (LibraryObject lO in PlanToWatch)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.PlanToWatch; }
            }
            foreach (LibraryObject lO in Completed)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Completed; }
            }
            foreach (LibraryObject lO in OnHold)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.OnHold; }
            }
            foreach (LibraryObject lO in Dropped)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Dropped; }
            }
            foreach (LibraryObject lO in Favourites)
            {
                if (lO.anime.slug == slug) { return LibrarySelection.Favourites; }
            }

            return LibrarySelection.None;
        }
        public int IndexInLibrary(LibrarySelection selection, LibraryObject temp)
        {
            if (!DoesExistInLibrary(selection, temp)) { return -1; }

            switch (selection)
            {
                case LibrarySelection.CurrentlyWatching:
                    for (int i = 0; i < CurrentlyWatching.Count; i++)
                    {
                        if (temp.anime.slug == CurrentlyWatching[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.PlanToWatch:
                    for (int i = 0; i < PlanToWatch.Count; i++)
                    {
                        if (temp.anime.slug == PlanToWatch[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Completed:
                    for (int i = 0; i < Completed.Count; i++)
                    {
                        if (temp.anime.slug == Completed[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.OnHold:
                    for (int i = 0; i < OnHold.Count; i++)
                    {
                        if (temp.anime.slug == OnHold[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Dropped:
                    for (int i = 0; i < Dropped.Count; i++)
                    {
                        if (temp.anime.slug == Dropped[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Favourites:
                    for (int i = 0; i < Favourites.Count; i++)
                    {
                        if (temp.anime.slug == Favourites[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.Recent:
                    for (int i = 0; i < Recent.Count; i++)
                    {
                        if (temp.anime.slug == Recent[i].anime.slug) { return i; }
                    }
                    break;
                case LibrarySelection.APISupported:
                    int iA;

                    iA = IndexInLibrary(LibrarySelection.CurrentlyWatching, temp);
                    if (iA != -1) return iA;

                    iA = IndexInLibrary(LibrarySelection.PlanToWatch, temp);
                    if (iA != -1) return iA;

                    iA = IndexInLibrary(LibrarySelection.Completed, temp);
                    if (iA != -1) return iA;

                    iA = IndexInLibrary(LibrarySelection.OnHold, temp);
                    if (iA != -1) return iA;

                    iA = IndexInLibrary(LibrarySelection.Dropped, temp);
                    if (iA != -1) return iA;

                    break;
                case LibrarySelection.All:
                    int ii;

                    ii = IndexInLibrary(LibrarySelection.APISupported, temp);
                    if (ii != -1) return ii;

                    ii = IndexInLibrary(LibrarySelection.Favourites, temp);
                    if (ii != -1) return ii;

                    break;
            }

            return -1;
        }
        #endregion
        #endregion

        #region Static Methods
        public static LibrarySelection GetLibrarySelectionFromStatus(LibraryObject lO)
        {
            return GetLibrarySelectionFromStatus(lO.status);
        }
        public static LibrarySelection GetLibrarySelectionFromStatus(string status)
        {
            switch (status)
            {
                case "currently-watching":
                    return LibrarySelection.CurrentlyWatching;
                case "plan-to-watch":
                    return LibrarySelection.PlanToWatch;
                case "completed":
                    return LibrarySelection.Completed;
                case "on-hold":
                    return LibrarySelection.OnHold;
                case "dropped":
                    return LibrarySelection.Dropped;

                case "favourites":
                    return LibrarySelection.Favourites;
                case "recent":
                    return LibrarySelection.Recent;
                case "all":
                    return LibrarySelection.All;
                default:
                    return LibrarySelection.None;
            }
        }
        public static string GetStatusFromLibrarySelection(LibrarySelection lS)
        {
            switch (lS)
            {
                case LibrarySelection.CurrentlyWatching:
                    return "currently-watching";
                case LibrarySelection.PlanToWatch:
                    return "plan-to-watch";
                case LibrarySelection.Completed:
                    return "completed";
                case LibrarySelection.OnHold:
                    return "on-hold";
                case LibrarySelection.Dropped:
                    return "dropped";
                case LibrarySelection.All:
                case LibrarySelection.Favourites:
                case LibrarySelection.Recent:
                case LibrarySelection.None:
                default:
                    return "";
            }
        }
        #endregion
    }
}
