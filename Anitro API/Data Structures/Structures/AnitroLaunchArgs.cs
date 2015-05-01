using System;
using System.Collections.Generic;
using System.Text;
using Anitro.Data_Structures.Enumerators;
using System.Diagnostics;

namespace Anitro.Data_Structures.Structures
{
    public class AnitroLaunchArgs
    {
        public AnitroLaunchType type;
        public string args;
        public string status;


        public AnitroLaunchArgs(AnitroLaunchType _type, string _args, string _status = "uriAssociation")
        {
            type = _type;
            args = _args;
            status = _status;
        }

        public AnitroLaunchArgs(string _parameters)
        {
            // Split the strings off of the & symbol
            string[] param = _parameters.Split('&');

            // Break each one down further to get the individual parts
            string[] paramType = param[0].Split('=');
            type = GetAnitroLaunchTypeFromString(paramType[1]);

            string[] paramArgs = param[1].Split('=');
            args = paramArgs[1];

            // 3 items is not always guarenteed, so check if there is 3 or more then grab the third.
            if (param.Length >= 3)
            {
                string[] paramStatus = param[2].Split('=');
                status = paramStatus[1];
            }
            else
            {
                status = "uriAssociation";
            }
        }

        public override string ToString()
        {
            string sArgs = "type=" + type.ToString() + "&" +
                           "args=" + args + "&" +
                           "status=" + "uriAssociation";

            Debug.WriteLine(sArgs);
            return sArgs;
        }

        public static AnitroLaunchType GetAnitroLaunchTypeFromString(string _str)
        {
            switch (_str)
            {
                case "Anime":
                    return AnitroLaunchType.Anime;
                case "Manga":
                    return AnitroLaunchType.Manga;
                case "Search": 
                    return AnitroLaunchType.Search;
                case "Url":
                    return AnitroLaunchType.Url;
                case "None":
                default:
                    return AnitroLaunchType.None;
            }
        }
    }
}
