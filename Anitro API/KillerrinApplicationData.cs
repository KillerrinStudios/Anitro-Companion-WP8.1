using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro
{
    public class KillerrinApplicationData
    {
        public string Name = "Anitro";
        public string Version = "1.1.0.0";
        public string Description = "Developed by Andrew Godfroy of Killerrin Studios. Hummingbird is owned and developed by the crew of hummingbird.me";

        public string Developer = "Killerrin Studios";
        public string Website = "http://www.killerrin.com";
        public string OtherWebsite = "http://www.hummingbird.me";
        public string Twitter = "http://www.twitter.com/killerrin";
        
        public string FeedbackUrl = "support@killerrin.com";
        public string FeedbackSubject = "feedback - ";

        public string SupportUrl = "support@killerrin.com";
        public string SupportSubject = "support - ";

        public KillerrinApplicationData()
        {
            FeedbackSubject += Name;
            SupportSubject += Name;
        }

        // Mashape API Key
        public string MashapeKey
        {
            get
            {
                // If we're in Debug Mode, Return the Mashape Testing Key to make testing easier
                if (Consts.DebugMode) return "JIyg90lZ0KRmT0qivz8ECXjvl0rd18lS";

                // If not, return the production key
                return "TkLbJdjaFrDjcjuGrKc5XvJREP0pgnYs";
            }
        }

        public string AnitroPackageName
        {
            get
            {
                return "55c35d32-c29b-4cd9-9dd5-7b00b0941c3c";
            }
        }
    }
}
