using Anitro.APIs;
using Anitro_API.Data_Structures.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Anitro.Data_Structures
{
    public class Settings
    {
        public static bool Loaded = false;

        public double AutoGenerateLibraryAfterXDays;

        public AutoUpdateSetting BackgroundTaskSettings;

        public DateTime ScheduledTaskLastRun;
        public DateTime ScheduledTaskNextRun;

        public bool randomizeFavourites;
        public bool randomizeCurrentlyWatching;
        public bool randomizeCompleted;
        public bool randomizePlanToWatch;
        public bool randomizeOnHold;
        public bool randomizeDropped;

        public Settings()
        {
            AutoGenerateLibraryAfterXDays = 1.0;

            BackgroundTaskSettings = AutoUpdateSetting.EveryActivation;

            ScheduledTaskLastRun = new DateTime();
            ScheduledTaskNextRun = new DateTime();

            randomizeFavourites = true;
            randomizeCurrentlyWatching = true;
            randomizeCompleted = true;
            randomizePlanToWatch = true;
            randomizeOnHold = true;
            randomizeDropped = true;

        }
        public Settings(Settings settings)
        {
            SetFromSettings(settings);
        }
        public void SetFromSettings(Settings settings)
        {
            AutoGenerateLibraryAfterXDays = settings.AutoGenerateLibraryAfterXDays;

            BackgroundTaskSettings = settings.BackgroundTaskSettings;

            ScheduledTaskLastRun = settings.ScheduledTaskLastRun;
            ScheduledTaskNextRun = settings.ScheduledTaskNextRun;

            randomizeFavourites = settings.randomizeFavourites;
            randomizeCurrentlyWatching = settings.randomizeCurrentlyWatching;
            randomizeCompleted = settings.randomizeCompleted;
            randomizePlanToWatch = settings.randomizePlanToWatch;
            randomizeOnHold = settings.randomizeOnHold;
            randomizeDropped = settings.randomizeDropped;
        }

        public static async System.Threading.Tasks.Task<Settings> Load()
        {
            Settings settings = new Settings();

            if (await StorageTools.DoesFileExist(StorageTools.StorageConsts.SettingsFile))
            {
                try
                {
                    string json = await StorageTools.LoadFileFromStorage(StorageTools.StorageConsts.SettingsFile);
                    settings.JsonToThis(json);

                    Debug.WriteLine("Settings Loaded");
                }
                catch (Exception)
                {
                    Debug.WriteLine("Exception Loading Settings");
                    Consts.AppSettings.DeleteFile();
                    settings = new Settings();
                }
            }

            Loaded = true;
            return settings;
        }
        public async System.Threading.Tasks.Task<bool> Save()
        {
            try
            {
                Debug.WriteLine("Settings:Save(): Begun");
                StorageTools.isSavingComplete = false;

                string json = ThisToJson();
                bool result = await StorageTools.SaveToStorage(StorageTools.StorageConsts.SettingsFile, json);

                Debug.WriteLine("Settings:Save(): Success!");

                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("Settings:Save(): Failed");
                StorageTools.isSavingComplete = true;
                return false;
            }
        }
        public async System.Threading.Tasks.Task DeleteFile()
        {
            try
            {
                Debug.WriteLine("Settings:DeleteFile()");
                await StorageTools.DeleteFile(StorageTools.StorageConsts.SettingsFile, Windows.Storage.StorageDeleteOption.PermanentDelete);
            }
            catch (Exception) { }
        }

        #region Json Tools
        public string ThisToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void JsonToThis(string json)
        {
            JObject jObject = JObject.Parse(json);
            Settings settings = JsonConvert.DeserializeObject<Settings>(jObject.ToString());

            SetFromSettings(settings);
        }
        #endregion

        public override string ToString()
        {
            return "AutoGenerateLibraryAfterXDays: " + AutoGenerateLibraryAfterXDays.ToString() + " | " +
                   "BackgroundTaskSettings: " + BackgroundTaskSettings.ToString() + " | " +
                   "ScheduledTaskLastRun: " + ScheduledTaskLastRun.ToString() + " | " +
                   "ScheduledTaskNextRun: " + ScheduledTaskNextRun.ToString() + " | " +
                   "randomizeFavourites: " + randomizeFavourites.ToString() + " | " +
                   "randomizeCurrentlyWatching: " + randomizeCurrentlyWatching.ToString() + " | " +
                   "randomizeCompleted: " + randomizeCompleted.ToString() + " | " +
                   "randomizePlanToWatch: " + randomizePlanToWatch.ToString() + " | " +
                   "randomizeOnHold: " + randomizeOnHold.ToString() + " | " +
                   "randomizeDropped: " + randomizeDropped.ToString();
        }
    }
}