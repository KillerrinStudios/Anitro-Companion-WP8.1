using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Anitro;//using Anitro_API; //using Hummingbird_API;
using System;
using System.Net.NetworkInformation;
using Anitro.APIs;
using Anitro_API.Data_Structures.Enumerators;

namespace AnitroScheduler
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            Debug.WriteLine("Entering Background Agent");

            #region Early Exit Checks
            // If settings don't exist, chances are that their wont be any library info, so just exit now.
            //if (Storage.Settings.User.userName.IsDefault() || Storage.Settings.User.authToken.IsDefault())//Storage.DoesFileExist(Storage.SETTINGS))
            //{
            //    Debug.WriteLine("Login Info Not Found: Exiting Background Agent");
            //    NotifyComplete();
            //    return;
            //}

            if (!(Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication))
            {
                Debug.WriteLine("Lockscreen Not Provider: Exiting Background Agent");
                NotifyComplete();
                return;
            }

            if (!Consts.IsConnectedToInternet())
            {
                Debug.WriteLine("Network Not Found");
                NotifyComplete();
                return;
            }
            #endregion

            #region Settings Based Exit Checks
            AutoUpdateSetting currentSetting = StorageTools.Settings.BackgroundTaskSettings.Value;

            /// Current time     5:00
            /// Next Run         5:30   Last Ran 4:30   

            switch (currentSetting)
            {
                case AutoUpdateSetting.EveryActivation:
                    // Do nothing and just let it through
                    break;
                case AutoUpdateSetting.EveryHour:
                    if (DateTime.Now < StorageTools.Settings.ScheduledTaskLastRun.Value.AddHours(1.0))
                    {
                        Debug.WriteLine("Background Task Turned Off");
                        NotifyComplete();
                        return;
                    }
                    break;
                case AutoUpdateSetting.EveryFourHours:
                    if (DateTime.Now < StorageTools.Settings.ScheduledTaskLastRun.Value.AddHours(4.0))
                    {
                        Debug.WriteLine("Background Task Turned Off");
                        NotifyComplete();
                        return;
                    }
                    break;
                case AutoUpdateSetting.OnceADay:
                    if (DateTime.Now < StorageTools.Settings.ScheduledTaskLastRun.Value.AddDays(1.0))
                    {
                        Debug.WriteLine("Background Task Turned Off");
                        NotifyComplete();
                        return;
                    }
                    break;
                case AutoUpdateSetting.TwiceADay:
                    if (DateTime.Now < StorageTools.Settings.ScheduledTaskLastRun.Value.AddHours(12.0))
                    {
                        Debug.WriteLine("Background Task Turned Off");
                        NotifyComplete();
                        return;
                    }
                    break;
                case AutoUpdateSetting.EveryOtherDay:
                    if (DateTime.Now < StorageTools.Settings.ScheduledTaskLastRun.Value.AddDays(2.0))
                    {
                        Debug.WriteLine("Background Task Turned Off");
                        NotifyComplete();
                        return;
                    }
                    break;
                case AutoUpdateSetting.Off:
                default:
                    Debug.WriteLine("Background Task Turned Off");
                    NotifyComplete();
                    return;
            }
            #endregion

            // Update the Lockscreen Image
            await Lockscreen_Helper.SetRandomImageFromLibrary();

            #region Updating Settings
            Debug.WriteLine("Updating the settings");

            StorageTools.Settings.ScheduledTaskLastRun.Value = DateTime.Now;
            #endregion

            // If a debugger is attached, relaunch it in 30 seconds for testing purposes.
            if (Debugger.IsAttached)
            {
                ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(30));
            }
            
            Debug.WriteLine("Exiting Background Agent");
            NotifyComplete();
        }
    }
}