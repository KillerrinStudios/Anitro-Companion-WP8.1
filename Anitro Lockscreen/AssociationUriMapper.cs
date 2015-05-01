using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Navigation;

namespace Anitro_Lockscreen
{
    class AssociationUriMapper : UriMapperBase
    {
        private string tempUri;
        private string path = "";//"/Pages";
        public override Uri MapUri(Uri uri)
        {
            Debug.WriteLine("\n\n");

            tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());
            Debug.WriteLine("Opening App with: " + tempUri);
            // URI association launch for my app detected
            //if (tempUri.Contains("myappuri:MainPage?Category="))
            //{
            //    // Get the category (after "Category=").
            //    int categoryIndex = tempUri.IndexOf("Category=") + 9;
            //    string category = tempUri.Substring(categoryIndex);
            //    // Redirect to the MainPage.xaml with the proper category to be displayed
            //    return new Uri("/MainPage.xaml?Category=" + category, UriKind.Relative);
            //}

            if (tempUri.Contains("anitrols:type="))
            {
                Debug.WriteLine("anitrols:type= UriAssociation Found");

                int categoryIndex = tempUri.IndexOf("type=");
                string uriAssociation = tempUri.Substring(0, categoryIndex);
                string category = tempUri.Substring(categoryIndex);
                return GenerateURI(categoryIndex, uriAssociation, category);
            }

            #region Launch Specific Page
            // If the above gives nothing, ensure mainpage atleast opens
            if (tempUri.Contains("anitrols:"))
            {
                Debug.WriteLine("UriAssociation Found. Forcing Open through URI Accociation");
                int categoryIndex = tempUri.IndexOf(':');
                string page = tempUri.Substring(categoryIndex + 1);

                return new Uri(path + "/MainPage.xaml", UriKind.Relative);

                //if (string.IsNullOrEmpty(page))
                //{
                //    Debug.WriteLine("Uri Association invalid: Loading /MainPage.xaml");
                //    return new Uri(path + "/MainPage.xaml", UriKind.Relative);
                //}
                //else
                //{
                //    if (page[0] != '/') page = "/" + page;

                //    Debug.WriteLine("UriQueryString: " + page);
                //    return new Uri(path + "/MainPage.xaml?" + page, UriKind.Relative);
                //}
            }
            #endregion

            // Otherwise perform normal launch.
            Debug.WriteLine("No Uri Association Found: Loading: " + uri.OriginalString);
            Uri mappedUri = new Uri(path + uri.OriginalString, UriKind.Relative);
            return mappedUri; //uri;
        }

        private Uri GenerateURI(int categoryIndex, string _uriAssosiation, string _queryString)
        {
            Debug.WriteLine("GenerateURI(): Entering");
            Debug.WriteLine(categoryIndex);
            Debug.WriteLine(_uriAssosiation);
            Debug.WriteLine(_queryString);


            if (_uriAssosiation.Contains("anitrols:")) //Uri uri = new Uri("killerrin-anitro:type=anime&slug=steins-gate", UriKind.RelativeOrAbsolute);
            { //Uri uri = new Uri("anitro:type=anime&slug=steins-gate", UriKind.RelativeOrAbsolute);
                string type = _queryString.Substring(5, 5);
                if (type.Contains("Anime"))
                {
                    string sendData = _queryString.Substring(5 + 6) + "&status=uriAssociation";
                    Debug.WriteLine("sendData: " + sendData);

                    return new Uri(path + "/MainPage.xaml?" + sendData, UriKind.Relative);
                }
                else
                {
                    if (String.IsNullOrEmpty(_queryString))
                    {
                        return new Uri(path + "/MainPage.xaml", UriKind.Relative);
                    }
                    else
                    {
                        Debug.WriteLine(path + _queryString);
                        return new Uri(path + _queryString, UriKind.Relative);
                    }
                }

            }

            return new Uri(path + "/MainPage.xaml", UriKind.Relative);
        }
    }
}
