using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Newtonsoft.Json;
using Windows.Storage;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using Anitro_API.Data_Structures.Enumerators;

namespace Anitro.APIs
{
    public static class StorageTools
    {
        public static bool isSavingComplete = true; // If True, it is safe to exit. If not, loop continuously

        public static class StorageConsts
        {
            public static string UserFile = "User.hmb";
            public static string SettingsFile = "Settings.hmb";
            public static string LibraryFile = "Library.hmb";
            public static string AvatarImage = "UserAvatar.jpg";

            public static string LocalStorageFolderPrefix = "ms-appdata:///local/";
            public static string VisualStudioSolutionFilePrefix = "ms-appx:///";
        }

        public static async Task<bool> DoesFileExist(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            
            foreach(StorageFile file in files)
            {
                if (fileName == file.Name) { return true; }
            }

            return false;
        }

        public static async Task<bool> DoesFolderExist(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFolder> files = await folder.GetFoldersAsync();

            foreach (StorageFolder file in files)
            {
                if (fileName == file.Name) { return true; }
            }

            return false;
        }

        #region Simple Tools
        public static async Task<bool> CreateFolder(string folderName, CreationCollisionOption collisionSettings = CreationCollisionOption.OpenIfExists)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, collisionSettings);

            return true;
        }

        public static async Task<bool> SaveToStorage(string fileName, string content)
        {
            if (Consts.isApplicationClosing) return false;

            // Hold up while we wait for the previous person to finish
            //if (!isSavingComplete)
            //{
            //    bool loop = true;
            //    while (loop)
            //    {
            //        if (isSavingComplete) { loop = false; }
            //    }
            //}
            
            isSavingComplete = false;

            try
            {
                Debug.WriteLine("SaveToStorage(): Saving to Storage");
                byte[] data = Encoding.UTF8.GetBytes(content);

                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (Stream s = await file.OpenStreamForWriteAsync())
                {
                    await s.WriteAsync(data, 0, data.Length);
                }
                Debug.WriteLine("Storage Saved: " + fileName);

                isSavingComplete = true;
                return true;
            }
            catch (Exception) { isSavingComplete = true; return false; }
        }
        public static async Task<bool> SaveFileFromServer(Uri serverURI, string fileName)
        {
            isSavingComplete = false;

            try
            {
                Debug.WriteLine("Opening Client");
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

                Debug.WriteLine("Grabbing File");
                byte[] result = await client.GetByteArrayAsync(serverURI);

                Debug.WriteLine("Writing File");
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (Stream s = await file.OpenStreamForWriteAsync())
                {
                    await s.WriteAsync(result, 0, result.Length);
                }

                Debug.WriteLine("Storage Saved: " + fileName);

                isSavingComplete = true;
                return true;
            }
            catch (Exception) { isSavingComplete = true; return false; }
        }
        public static async Task<string> LoadFileFromStorage(string fileName)
        {
            byte[] data;

            StorageFolder folder = ApplicationData.Current.LocalFolder;

            StorageFile file = await folder.GetFileAsync(fileName);
            using (Stream s = await file.OpenStreamForReadAsync())
            {
                data = new byte[s.Length];
                await s.ReadAsync(data, 0, (int)s.Length);
            }
            Debug.WriteLine(fileName + " Loaded");
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }
        #endregion

        #region Deletion Tools
        public static async Task<bool> DeleteFolder(string folderName, StorageDeleteOption deleteOptions = StorageDeleteOption.PermanentDelete)
        {
            StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(folderName);

            await folder.DeleteAsync(deleteOptions);
            return true;
        }
        public static async Task<bool> DeleteFile(string fileName, StorageDeleteOption deleteOptions = StorageDeleteOption.PermanentDelete)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await file.DeleteAsync(deleteOptions);
            return true;
        }
        #endregion

        #region Settings
        public static class Settings
        {
            public static class Lockscreen
            {
                public static readonly IsolatedStorageProperty<bool> randomizeFavourites = new IsolatedStorageProperty<bool>("lockscreen randomize Favourites", true);
                public static readonly IsolatedStorageProperty<bool> randomizeCurrentlyWatching = new IsolatedStorageProperty<bool>("lockscreen randomize Currently Watching", true);
                public static readonly IsolatedStorageProperty<bool> randomizeCompleted = new IsolatedStorageProperty<bool>("lockscreen randomize Completed", true);
                public static readonly IsolatedStorageProperty<bool> randomizePlanToWatch = new IsolatedStorageProperty<bool>("lockscreen randomize Plan To Watch", true);
                public static readonly IsolatedStorageProperty<bool> randomizeOnHold = new IsolatedStorageProperty<bool>("lockscreen randomize OnHold", true);
                public static readonly IsolatedStorageProperty<bool> randomizeDropped = new IsolatedStorageProperty<bool>("lockscreen randomize Dropped", true);
            }

            public static readonly IsolatedStorageProperty<DateTime> ScheduledTaskLastRun = new IsolatedStorageProperty<DateTime>("ScheduledTaskLastRun", new DateTime());
            public static readonly IsolatedStorageProperty<DateTime> ScheduledTaskNextRun = new IsolatedStorageProperty<DateTime>("ScheduledTaskLastRun", new DateTime());
            
            public static readonly IsolatedStorageProperty<Uri> CurrentLockscreenImageUri = new IsolatedStorageProperty<Uri>("CurrentLockscreenUri", new Uri("ms-appdata:///local/"));
            public static readonly IsolatedStorageProperty<bool> LockscreenImageFile = new IsolatedStorageProperty<bool>("LockscreenImageFile", false);

            public static readonly IsolatedStorageProperty<AutoUpdateSetting> BackgroundTaskSettings = new IsolatedStorageProperty<AutoUpdateSetting>("AutoUpdateSetting", AutoUpdateSetting.EveryActivation);

        }
        #endregion
    }
}

#region Isolated Storage Settings
/// <summary>
/// Helper class is needed because IsolatedStorageProperty is generic and 
/// can not provide singleton model for static content
/// </summary>
internal static class IsolatedStoragePropertyHelper
{
    /// <summary>
    /// We must use this object to lock saving settings
    /// </summary>
    public static readonly object ThreadLocker = new object();

    public static readonly IsolatedStorageSettings Store = IsolatedStorageSettings.ApplicationSettings;
}

/// <summary>
/// This is wrapper class for storing one setting
/// Object of this type must be single
/// </summary>
/// <typeparam name="T">Any serializable type</typeparam>
public class IsolatedStorageProperty<T>
{
    private readonly object _defaultValue;
    private readonly string _name;
    private readonly object _syncObject = new object();

    public IsolatedStorageProperty(string name, T defaultValue = default(T))
    {
        _name = name;
        _defaultValue = defaultValue;
    }

    /// <summary>
    /// Determines if setting exists in the storage
    /// </summary>
    public bool Exists
    {
        get { return IsolatedStoragePropertyHelper.Store.Contains(_name); }
    }

    /// <summary>
    /// Use this property to access the actual setting value
    /// </summary>
    public T Value
    {
        get
        {
            //If property does not exist - initializing it using default value
            if (!Exists)
            {
                //Initializing only once
                lock (_syncObject)
                {
                    if (!Exists) SetDefault();
                }
            }

            return (T)IsolatedStoragePropertyHelper.Store[_name];
        }
        set
        {
            IsolatedStoragePropertyHelper.Store[_name] = value;
            Save();
        }
    }

    private static void Save()
    {
        lock (IsolatedStoragePropertyHelper.ThreadLocker)
        {
            IsolatedStoragePropertyHelper.Store.Save();
        }
    }

    public bool IsDefault()
    {
        return (Value.ToString() == _defaultValue.ToString());
    }

    public void SetDefault()
    {
        Value = (T)_defaultValue;
    }
}
#endregion