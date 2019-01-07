using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hato.Helpers
{
    public class TitleIdMapping
    {
        public int anidb_id = -1;
        public int anilist_id = -1;
        public int kitsu_id = -1;
        public int mal_id = -1;
        public string notify_id = "";
        private object lookupid;
        private string servicename;
        public MediaType mediatype;
        public Service currentservice;
        public Dictionary<string, object> output;
        public bool errored;
        public bool notfound;
        private TitleIDConverter converter;
        public TitleIdMapping(string service, string type, string id)
        {
            converter = new TitleIDConverter();
            if (converter.sqlliteinitalized)
            {
                switch (type)
                {
                    case "anime":
                        mediatype = MediaType.Anime;
                        break;
                    case "manga":
                        mediatype = MediaType.Manga;
                        break;
                    default:
                        output = new Dictionary<string, object> { { "data", null }, { "error", "Invalid media type. Type must be anime or manga." } };
                        errored = true;
                        return;

                }
                try
                {
                    switch (service)
                    {
                        case "anidb":
                            anidb_id = int.Parse(id);
                            currentservice = Service.AniDB;
                            lookupid = anidb_id;
                            if (mediatype == MediaType.Manga)
                            {
                                output = invalidtypeservice();
                                errored = true;
                            }
                            break;
                        case "anilist":
                            anilist_id = int.Parse(id);
                            currentservice = Service.AniList;
                            lookupid = anilist_id;
                            break;
                        case "mal":
                            mal_id = int.Parse(id);
                            currentservice = Service.MyAnimeList;
                            lookupid = mal_id;
                            break;
                        case "kitsu":
                            kitsu_id = int.Parse(id);
                            currentservice = Service.Kitsu;
                            lookupid = kitsu_id;
                            break;
                        case "notify":
                            notify_id = id;
                            currentservice = Service.NotifyMoe;
                            lookupid = notify_id;
                            if (mediatype == MediaType.Manga)
                            {
                                output = invalidtypeservice();
                                errored = true;
                            }
                            break;
                        default:
                            if (mediatype == MediaType.Anime)
                            {
                                output = new Dictionary<string, object> { { "data", null }, { "error", "Invalid service. Services accepted for Anime are anidb, anilist, kitsu, mal, and notify" } };
                            }
                            else
                            {
                                output = new Dictionary<string, object> { { "data", null }, { "error", "Invalid service. Services accepted for Manga are anilist, kitsu, and mal" } };
                            }
                                errored = true;
                            break;
                    }
                    servicename = service;
                }
                catch (Exception e)
                {
                    output = new Dictionary<string, object> { { "data", null }, { "error", "Invalid title id." } };
                    errored = true;
                }
            }
            else
            {
                output = new Dictionary<string, object> { { "data", null }, { "error", "Can't connect to database." } };
                errored = true;
            }
        }
        public void Dispose()
        {
            converter.Dispose();
        }
        public void performLookup()
        {
            bool idvalid = lookupid is int ? ((int)lookupid > 0) : (((String)lookupid).Length > 0);
            if (idvalid)
            {
                notfound = false;
                bool notifyidpopulated = false;
                if (mediatype == MediaType.Anime)
                {
                    // Search using Notify Moe first
                    Dictionary<string, object> tmpdict = converter.notifyIdLookup(currentservice, lookupid);
                    if (tmpdict != null)
                    {
                        // Populate 
                        populateTitleIdsWithDictionary(tmpdict);
                        notifyidpopulated = true;
                    }
                    else if (currentservice == Service.NotifyMoe || currentservice == Service.AniDB)
                    {
                        notfound = true;
                    }
                }
                if (!notfound && !notifyidpopulated)
                {
                    switch (currentservice)
                    {
                        case Service.AniList:
                            mal_id = converter.GetMALIDFromAniListID(anilist_id, mediatype);
                            if (mal_id > 0)
                            {
                                kitsu_id = converter.GetKitsuIDFromMALID(mal_id, mediatype);
                            }
                            else
                            {
                                notfound = true;
                            }
                            break;
                        case Service.MyAnimeList:
                            anilist_id = converter.GetAniListIDFromMALID(mal_id, mediatype);
                            kitsu_id = converter.GetKitsuIDFromMALID(mal_id, mediatype);
                            if (anilist_id < 1 && kitsu_id < 1)
                            {
                                notfound = true;
                            }
                            break;
                        case Service.Kitsu:
                            mal_id = converter.GetMALIDFromKitsuID(kitsu_id, mediatype);
                            if (mal_id > 0)
                            {
                                anilist_id = converter.GetAniListIDFromMALID(mal_id, mediatype);
                            }
                            else
                            {
                                notfound = true;
                            }
                            break;
                    }
                }
                if (notfound)
                {
                    output = new Dictionary<string, object> { { "data", null }, { "error", "Nothing found for " + lookupid.ToString() } }; 
                }
                else
                {
                    output = generateDictionary();
                }
            }
            else
            {
                if (currentservice == Service.NotifyMoe)
                {
                    output = new Dictionary<string, object> { { "data", null }, { "error", "Id must be greater than 1 in length." } };
                }
                else
                {
                    output = new Dictionary<string, object> { { "data", null }, { "error", "Id must be greater than 0." } };
                }
                errored = true;
            }
        }
        private Dictionary<string, object> generateDictionary()
        {
            Dictionary<string, object> titleidlist;
            if (mediatype == MediaType.Anime)
            {
                titleidlist = new Dictionary<string, object> { { "anidb_id", anidb_id > 0 ? anidb_id : (int?)null }, { "anilist_id", anilist_id > 0 ? anilist_id : (int?)null }, { "kitsu_id", kitsu_id > 0 ? kitsu_id : (int?)null }, { "mal_id", (mal_id > 0) ? mal_id : (int?)null }, { "notify_id", notify_id.Length > 0 ? notify_id : null }, { "type", mediatype }, { "type_str", "anime" } };
            }
            else
            {
                titleidlist = new Dictionary<string, object> { { "anilist_id", anilist_id > 0 ? anilist_id : (int?)null }, { "kitsu_id", kitsu_id > 0 ? kitsu_id : (int?)null }, { "mal_id", (mal_id > 0) ? mal_id : (int?)null }, { "type", mediatype }, { "type_str", "manga" } };
            }
            return new Dictionary<string, object> { { "data", titleidlist } };
        }
        private Dictionary<string,object> invalidtypeservice()
        {
            return output = new Dictionary<string, object> { { "data", null }, { "error", "Invalid service for Manga. Services accepted for Manga are anilist, kitsu, and mal" } };
        }
        private void populateTitleIdsWithDictionary(Dictionary<string, object>dictionary)
        {
            if (dictionary["anidb_id"] != System.DBNull.Value)
            {
                anidb_id = (int)dictionary["anidb_id"];
            }
            if (dictionary["anilist_id"] != System.DBNull.Value)
            {
                anilist_id = (int)dictionary["anilist_id"];
            }
            if (dictionary["kitsu_id"] != System.DBNull.Value)
            {
                kitsu_id = (int)dictionary["kitsu_id"];
            }
            if (dictionary["mal_id"] != System.DBNull.Value)
            {
                mal_id = (int)dictionary["mal_id"];
            }
            if (dictionary["notify_id"] != System.DBNull.Value)
            {
                notify_id = (string)dictionary["notify_id"];
            }
        }
    }
}
