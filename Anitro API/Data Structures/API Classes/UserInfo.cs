using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

using Anitro.Data_Structures.Enumerators;

namespace Anitro.Data_Structures.API_Classes
{
    public class TopGenres
    {
        public List<AnimeGenre> genresWithData = new List<AnimeGenre>() { };

        public double GetGenreTotal(AnimeGenre genre)
        {
            switch (genre)
            {
                case AnimeGenre.Action:
                    return Action;
                case AnimeGenre.Adventure:
                    return Adventure;
                case AnimeGenre.AnimeInfluenced:
                    return AnimeInfluenced;

                case AnimeGenre.Cars:
                    return Cars;
                case AnimeGenre.Comedy:
                    return Comedy;

                case AnimeGenre.Dementia:
                    return Dementia;
                case AnimeGenre.Demons:
                    return Demons;
                case AnimeGenre.Drama:
                    return Drama;

                case AnimeGenre.Ecchi:
                    return Ecchi;

                case AnimeGenre.Fantasy:
                    return Fantasy;

                case AnimeGenre.Game:
                    return Game;

                case AnimeGenre.Harem:
                    return Harem;
                case AnimeGenre.Historical:
                    return Historical;
                case AnimeGenre.Horror:
                    return Horror;

                case AnimeGenre.Kids:
                    return Kids;

                case AnimeGenre.Magic:
                    return Magic;
                case AnimeGenre.MartialArts:
                    return MartialArts;
                case AnimeGenre.Mecha:
                    return Mecha;
                case AnimeGenre.Military:
                    return Military;
                case AnimeGenre.Music:
                    return Music;
                case AnimeGenre.Mystery:
                    return Mystery;
                case AnimeGenre.Parody:
                    return Parody;
                case AnimeGenre.Psychological:
                    return Psychological;

                case AnimeGenre.Romance:
                    return Romance;

                case AnimeGenre.Samurai:
                    return Samurai;
                case AnimeGenre.School:
                    return School;
                case AnimeGenre.SciFi:
                    return SciFi;
                case AnimeGenre.ShoujoAi:
                    return ShoujoAi;
                case AnimeGenre.ShounenAi:
                    return ShounenAi;
                case AnimeGenre.SliceOfLife:
                    return SliceOfLife;
                case AnimeGenre.Space:
                    return Space;
                case AnimeGenre.Sports:
                    return Sports;
                case AnimeGenre.Supernatural:
                    return Supernatural;
                case AnimeGenre.SuperPower:
                    return SuperPower;

                case AnimeGenre.Thriller:
                    return Thriller;

                case AnimeGenre.Vampire:
                    return Vampire;
                default:
                    return 0.0;
            }
        }

        #region Private Variables
        private double _action;
        private double _adventure;
        private double _animeInfluenced;

        private double _cars;
        private double _comedy;

        private double _dementia;
        private double _demons;
        private double _drama;

        private double _ecchi;

        private double _fantasy;

        private double _game;

        private double _harem;
        private double _historical;
        private double _horror;

        private double _kids;

        private double _magic;

        private double _martialArts;
        private double _mecha;
        private double _military;
        private double _music;
        private double _mystery;

        private double _parody;
        private double _psychological;

        private double _romance;

        private double _samurai;
        private double _school;
        private double _sciFi;
        private double _shoujoAi;
        private double _shounenAi;
        private double _sliceOfLife;
        private double _space;
        private double _sports;
        private double _supernatural;
        private double _superPower;

        private double _thriller;

        private double _vampire;
        #endregion

        #region Properties
        public double Action 
        { 
            get { return _action;}
            set { genresWithData.Add(AnimeGenre.Action); _action = value; }
        }

        public double Adventure
        {
            get { return _adventure; }
            set { genresWithData.Add(AnimeGenre.Adventure); _adventure = value;
            }
        }

        [JsonProperty(PropertyName = "Anime Influenced")]
        public double AnimeInfluenced
        {
            get { return _animeInfluenced; }
            set { genresWithData.Add(AnimeGenre.AnimeInfluenced); _animeInfluenced = value; }
        }

        public double Cars
        {
            get { return _cars; }
            set { genresWithData.Add(AnimeGenre.Cars); _cars = value; }
        }
        public double Comedy
        {
            get { return _comedy; }
            set { genresWithData.Add(AnimeGenre.Comedy); _comedy = value; }
        }

        public double Dementia
        {
            get { return _dementia; }
            set { genresWithData.Add(AnimeGenre.Dementia); _dementia = value; }
        }
        public double Demons
        {
            get { return _demons; }
            set { genresWithData.Add(AnimeGenre.Demons); _demons = value; }
        }
        public double Drama
        {
            get { return _drama; }
            set { genresWithData.Add(AnimeGenre.Drama); _drama = value; }
        }

        public double Ecchi
        {
            get { return _ecchi; }
            set { genresWithData.Add(AnimeGenre.Ecchi); _ecchi = value; }
        }

        public double Fantasy
        {
            get { return _fantasy; }
            set { genresWithData.Add(AnimeGenre.Fantasy); _fantasy = value; }
        }

        public double Game
        {
            get { return _game; }
            set { genresWithData.Add(AnimeGenre.Game); _game = value; }
        }

        public double Harem
        {
            get { return _harem; }
            set { genresWithData.Add(AnimeGenre.Harem); _harem = value; }
        }
        public double Historical { 
            get { return _historical;}
            set { genresWithData.Add(AnimeGenre.Historical); _historical = value; }
        }
        public double Horror
        {
            get { return _horror; }
            set { genresWithData.Add(AnimeGenre.Horror); _horror = value; }
        }

        public double Kids
        {
            get { return _kids; }
            set { genresWithData.Add(AnimeGenre.Kids); _kids = value; }
        }

        public double Magic
        {
            get { return _magic; }
            set { genresWithData.Add(AnimeGenre.Magic); _magic = value; }
        }
    
        [JsonProperty(PropertyName = "Martial Arts")]
        public double MartialArts
        {
            get { return _martialArts; }
            set { genresWithData.Add(AnimeGenre.MartialArts); _martialArts = value; }
        }
        public double Mecha
        {
            get { return _mecha; }
            set { genresWithData.Add(AnimeGenre.Mecha); _mecha = value; }
        }
        public double Military
        {
            get { return _military; }
            set { genresWithData.Add(AnimeGenre.Military); _military = value; }
        }
        public double Music
        {
            get { return _music; }
            set { genresWithData.Add(AnimeGenre.Music); _music = value; }
        }
        public double Mystery
        {
            get { return _mystery; }
            set { genresWithData.Add(AnimeGenre.Mystery); _mystery = value; }
        }

        public double Parody
        {
            get { return _parody; }
            set { genresWithData.Add(AnimeGenre.Parody); _parody = value; }
        }
        public double Psychological
        {
            get { return _psychological; }
            set { genresWithData.Add(AnimeGenre.Psychological); _psychological = value; }
        }

        public double Romance  { 
            get { return _romance;}
            set { genresWithData.Add(AnimeGenre.Romance); _romance = value; }
        }

        public double Samurai
        {
            get { return _samurai; }
            set { genresWithData.Add(AnimeGenre.Samurai); _samurai = value; }
        }
        public double School
        {
            get { return _school; }
            set { genresWithData.Add(AnimeGenre.School); _school = value; }
        }

        [JsonProperty(PropertyName = "Sci-Fi")]
        public double SciFi
        {
            get { return _sciFi; }
            set { genresWithData.Add(AnimeGenre.SciFi); _sciFi = value; }
        }
    
        [JsonProperty(PropertyName = "Shoujo Ai")]
        public double ShoujoAi
        {
            get { return _shoujoAi; }
            set { genresWithData.Add(AnimeGenre.ShoujoAi); _shoujoAi = value; }
        }

        [JsonProperty(PropertyName = "Shounen Ai")]
        public double ShounenAi
        {
            get { return _shounenAi; }
            set { genresWithData.Add(AnimeGenre.ShounenAi); _shounenAi = value; }
        }

        [JsonProperty(PropertyName = "Slice of Life")]
        public double SliceOfLife
        {
            get { return _sliceOfLife; }
            set { genresWithData.Add(AnimeGenre.SliceOfLife); _sliceOfLife = value; }
        }
        public double Space
        {
            get { return _space; }
            set { genresWithData.Add(AnimeGenre.Space); _space = value; }
        }
        public double Sports
        {
            get { return _sports; }
            set { genresWithData.Add(AnimeGenre.Sports); _sports = value; }
        }
        public double Supernatural
        {
            get { return _supernatural; }
            set { genresWithData.Add(AnimeGenre.Supernatural); _supernatural = value; }
        }

        [JsonProperty(PropertyName = "Super Power")]
        public double SuperPower
        {
            get { return _superPower; }
            set { genresWithData.Add(AnimeGenre.SuperPower); _superPower = value; }
        }

        public double Thriller
        {
            get { return _thriller; }
            set { genresWithData.Add(AnimeGenre.Thriller); _thriller = value; }
        }

        public double Vampire
        {
            get { return _vampire; }
            set { genresWithData.Add(AnimeGenre.Vampire); _vampire = value; }
        }
        #endregion
    }

    public class UserInfoFavorite
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int item_id { get; set; }
        public string item_type { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int fav_rank { get; set; }
    }

    public class UserInfo
    {
        #region Undocumented
        public int id { get; set; }
        public int anime_total { get; set; }
        public int anime_watched { get; set; }
        public TopGenres top_genres { get; set; }
        #endregion

        #region Documented
        public string name { get; set; }

        private string _waifu;
        public string waifu {
            get { return _waifu; }
            set
            {
                if (value != null)
                {
                    _waifu = value;
                }
            }
        }
        public string waifu_or_husbando { get; set; }
        public string waifu_slug { get; set; }

        private string _waifuCharID;
        public string waifu_char_id
        {
            get { return _waifuCharID; }
            set 
            {
                _waifuCharID = value; 
            }
        }

        public Uri GetWaifuPictureURI()
        {
            string beginningUrl = "http://static.hummingbird.me/characters/images/000/0";
            string firstTwo = "" + _waifuCharID[0] + _waifuCharID[1] + "/";
            string lastThree = "" + _waifuCharID[2] + _waifuCharID[3] + _waifuCharID[4] + '/';
            string afterCutID = "thumb_small/";
            string fullWaifuIDExtension =  _waifuCharID + ".jpg?1375255551";

            string fullUrl = beginningUrl + firstTwo + lastThree + afterCutID + fullWaifuIDExtension;
            return new Uri(fullUrl, UriKind.Absolute);
        }

        public string location { get; set; }
        public string website { get; set; }
        public string avatar { get; set; }
        public string cover_image { get; set; }
        public object about { get; set; }
        public string bio { get; set; }
        public int karma { get; set; }
        public int life_spent_on_anime { get; set; }
        public bool show_adult_content { get; set; }
        public string title_language_preference { get; set; }
        public string last_library_update { get; set; }
        public bool online { get; set; }
        public bool following { get; set; }
        #endregion

        public void AddUnDocumentedToDocumented(UserInfo _undocumentedUserinfo)
        {
            id = _undocumentedUserinfo.id;
            life_spent_on_anime = _undocumentedUserinfo.life_spent_on_anime;
            anime_total = _undocumentedUserinfo.anime_total;
            anime_watched = _undocumentedUserinfo.anime_watched;
            top_genres = _undocumentedUserinfo.top_genres;
        }
    }

    public class UserInfoRootObject
    {
        public UserInfo user_info { get; set; }
    }
}
