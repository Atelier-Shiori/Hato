/* TitleIDConverter.cs
 * This class converts one title id from one service to another.
 * 
 * Copyright (c) 2018-2019 MAL Updater OS X Group, a division of Moy IT Solutions
 * Licensed under Apache License 2.0
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RestSharp;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace hato.Helpers
{
    public enum MediaType
    {
        Anime = 0,
        Manga = 1
    }
    public enum Service
    {
        Kitsu = 1,
        AniList = 2,
        MyAnimeList = 3,
        NotifyMoe = 4,
        AniDB = 5 
    }

    public class TitleIDConverter
    {
        RestClient raclient;
        RestClient rkclient;
        RestClient rmoeclient;
        MySqlConnection connection;
        public bool sqlliteinitalized;

        public TitleIDConverter()
        {
            raclient = new RestClient("https://graphql.anilist.co");
            rkclient = new RestClient("https://kitsu.io/api/edge");
            rmoeclient = new RestClient("https://notify.moe/");
            rmoeclient.FollowRedirects = false;
            this.initalizeDatabase();
        }

        public void Dispose()
        {
            try
            {
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public Dictionary<string, object> notifyIdLookup (Service listservice, object titleid)
        {
            string notifymoeid;
            if (listservice == Service.NotifyMoe)
            {
                notifymoeid = (string)titleid;
            }
            else
            {
                notifymoeid = retrieveNotifyId(titleid, listservice);
                if (notifymoeid.Length == 0)
                {
                    return null;
                }
            }
            Dictionary<string, object> tmpdict = RetreiveSavedIDsFromServiceID(listservice,titleid,MediaType.Anime);
            if (tmpdict != null && tmpdict["notify_id"] != System.DBNull.Value)
            {
                return tmpdict;
            }
            else
            {
                RestRequest request = new RestRequest("/api/anime/" + notifymoeid, Method.GET);
                IRestResponse response = rmoeclient.Execute(request);
                if (response.StatusCode.GetHashCode() == 200)
                {
                    Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                    if (jsonData.ContainsKey("mappings"))
                    {
                        List<Dictionary<string, object>> mappings = ((JArray)jsonData["mappings"]).ToObject<List<Dictionary<string, object>>>();
                        int anilistid = 0, myanimelistid = 0, anidbid = 0, kitsuid = 0;
                        foreach (Dictionary<string, object> map in mappings)
                        {
                            string service = (string)map["service"];
                            try
                            {
                                switch (service)
                                {
                                    case "anilist/anime":
                                        anilistid = int.Parse((String)map["serviceId"]);
                                        break;
                                    case "myanimelist/anime":
                                        myanimelistid = int.Parse((String)map["serviceId"]);
                                        break;
                                    case "anidb/anime":
                                        anidbid = int.Parse((String)map["serviceId"]);
                                        break;
                                    case "kitsu/anime":
                                        kitsuid = int.Parse((String)map["serviceId"]);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        if (myanimelistid > 0)
                        {
                            SaveIDtoDatabase(Service.NotifyMoe, Service.MyAnimeList, notifymoeid, myanimelistid, MediaType.Anime);
                        }
                        if (kitsuid > 0)
                        {
                            if (myanimelistid > 0)
                            {
                                SaveIDtoDatabase(Service.Kitsu, Service.MyAnimeList, kitsuid, myanimelistid, MediaType.Anime);
                            }
                            else
                            {
                                SaveIDtoDatabase(Service.Kitsu, Service.NotifyMoe, kitsuid, notifymoeid, MediaType.Anime);
                            }
                        }
                        if (anilistid > 0)
                        {
                                if (myanimelistid > 0)
                                {
                                    SaveIDtoDatabase(Service.AniList, Service.MyAnimeList, anilistid, myanimelistid, MediaType.Anime);
                                }
                                else
                                {
                                    SaveIDtoDatabase(Service.AniList, Service.NotifyMoe, anilistid, notifymoeid, MediaType.Anime);
                                }
                        }

                        if (anidbid > 0)
                        {
                              if (myanimelistid > 0)
                              {
                                    SaveIDtoDatabase(Service.AniDB, Service.MyAnimeList, anidbid, myanimelistid, MediaType.Anime);
                              }
                              else
                              {
                                SaveIDtoDatabase(Service.AniDB, Service.NotifyMoe, anidbid, notifymoeid, MediaType.Anime);
                              }
                        }
                        tmpdict = RetreiveSavedIDsFromServiceID(listservice, titleid, MediaType.Anime);
                    }
                    else
                    {
                        tmpdict = null;
                    }
                }
            }
            return tmpdict;
        }

        public string retrieveNotifyId(object titleid, Service listservice)
        {
            object notifyid = RetreiveSavedTargetIDFromServiceID(Service.NotifyMoe, listservice, titleid, MediaType.Anime);
            bool notfoundblocker = notifyid is string ? ((string)notifyid).Length == 0 : false;
            if (notfoundblocker)
            {
                return "";
            }
            else if (notifyid is int)
            {
                RestRequest request = new RestRequest(retrieveServiceName(listservice) + "/anime/" + titleid.ToString() , Method.GET);
                IRestResponse response = rmoeclient.Execute(request);
                if (response.StatusCode.GetHashCode() == 302)
                {
                    string redirectedurl = response.Headers.ToList().Find(x => x.Name == "Location").Value.ToString();
                    string tnotifyid = redirectedurl.Replace("/anime/", "");
                    return tnotifyid;
                }
                else
                {
                    int existid = (int)CheckIfEntryExists(listservice, titleid, MediaType.Anime);
                    if (existid > 0)
                    {
                        SaveIDtoDatabase(Service.NotifyMoe, listservice, "", titleid, MediaType.Anime);
                    }
                    return "";
                }
            }
            return (string)notifyid;
        }
        
        public int GetMALIDFromKitsuID(int kitsuid, MediaType type)
        {
            int titleid = (int)this.RetreiveSavedTargetIDFromServiceID(Service.MyAnimeList,Service.Kitsu, kitsuid, type);
            if (titleid > -1)
            {
                return titleid;
            }
            String typestr = "";
            switch (type)
            {
                case MediaType.Anime:
                    typestr = "anime";
                    break;
                case MediaType.Manga:
                    typestr = "manga";
                    break;
                default:
                    break;
            }

            String filterstr = "myanimelist/" + typestr;
   
            RestRequest request = new RestRequest( "/" + typestr + "/" + kitsuid.ToString() + "?include=mappings&fields[anime]=id", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept", "application/vnd.api+json");

            IRestResponse response = rkclient.Execute(request);
            Thread.Sleep(1000);
            if (response.StatusCode.GetHashCode() == 200)
            {
                Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                if (jsonData.ContainsKey("included"))
                {
                    List<Dictionary<string, object>> included = ((JArray)jsonData["included"]).ToObject<List<Dictionary<string, object>>>();
                    foreach (Dictionary<string, object> map in included)
                    {
                        Dictionary<string, object> attr = JObjectToDictionary((JObject)map["attributes"]);
                        if (String.Equals(((String)attr["externalSite"]), "myanimelist/" + typestr, StringComparison.OrdinalIgnoreCase))
                        {
                            int malid = int.Parse((string)attr["externalId"]);
                            this.SaveIDtoDatabase(Service.MyAnimeList,Service.Kitsu, malid, kitsuid, type);
                            return malid;
                        }
                    }
                }
                return -1;
            }
            else
            {
                return -1;
            }
        }

        public int GetKitsuIDFromMALID(int malid, MediaType type)
        {
            int titleid = (int)this.RetreiveSavedTargetIDFromServiceID(Service.Kitsu,Service.MyAnimeList,malid, type);
            if (titleid > -1)
            {
                return titleid;
            }
            String typestr = "";
            switch (type)
            {
                case MediaType.Anime:
                    typestr = "anime";
                    break;
                case MediaType.Manga:
                    typestr = "manga";
                    break;
                default:
                    break;
            }

            String filterstr = "myanimelist/" + typestr;

            RestRequest request = new RestRequest("/mappings?filter[externalSite]=myanimelist/" + typestr + "&filter[external_id]=" + malid.ToString() , Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept", "application/vnd.api+json");

            IRestResponse response = rkclient.Execute(request);
            if (response.StatusCode.GetHashCode() == 200)
            {
                Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                if (jsonData.ContainsKey("data"))
                {
                    List<Dictionary<string, object>> data = ((JArray)jsonData["data"]).ToObject<List<Dictionary<string, object>>>();
                    foreach (Dictionary<string, object> map in data)
                    {
                        Dictionary<string, object> relationships = JObjectToDictionary((JObject)map["relationships"]);
                        Dictionary<string, object> item = JObjectToDictionary((JObject)relationships["item"]);
                        Dictionary<string, object> links = JObjectToDictionary((JObject)item["links"]);
                        String targetURL = (String)links["self"];
                        targetURL = targetURL.Replace("https://kitsu.io/api/edge", "");
                        request = new RestRequest(targetURL);
                        request.RequestFormat = DataFormat.Json;
                        request.AddHeader("Accept", "application/vnd.api+json");
                        response = rkclient.Execute(request);
                        if (response.StatusCode.GetHashCode() == 200)
                        {
                            Dictionary<string, object> idjsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                            if (idjsonData.ContainsKey("data"))
                            {
                                Dictionary<string, object> titleentrydata = JObjectToDictionary((JObject)idjsonData["data"]);
                                int kitsuid = int.Parse((String)titleentrydata["id"]);
                                this.SaveIDtoDatabase(Service.Kitsu,Service.MyAnimeList,kitsuid,malid,type);
                                return kitsuid;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
                return -1;
            }
            else
            {
                return -1;
            }

        }

        public int GetMALIDFromAniListID(int anilistid, MediaType type)
        {
            int titleid = (int)this.RetreiveSavedTargetIDFromServiceID(Service.MyAnimeList,Service.AniList, anilistid, type);
            if (titleid > -1)
            {
                return titleid;
            }
            String typestr = "";
            switch (type)
            {
                case MediaType.Anime:
                    typestr = "ANIME";
                    break;
                case MediaType.Manga:
                    typestr = "MANGA";
                    break;
                default:
                    break;
            }

            RestRequest request = new RestRequest("/", Method.POST);
            request.RequestFormat = DataFormat.Json;
            GraphQLQuery gquery = new GraphQLQuery();
            gquery.query = "query ($id: Int!, $type: MediaType) {\n  Media(id: $id, type: $type) {\n    id\n    idMal\n  }\n}";
            gquery.variables = new Dictionary<string, object> { { "id" , anilistid.ToString() }, { "type", typestr } };
            request.AddJsonBody(gquery);
            IRestResponse response = raclient.Execute(request);
            if (response.StatusCode.GetHashCode() == 200)
            {
                Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                Dictionary<string, object> data = JObjectToDictionary((JObject)jsonData["data"]);
                Dictionary<string, object> media = JObjectToDictionary((JObject)data["Media"]);
                int malid = !object.ReferenceEquals(null, media["idMal"]) ? Convert.ToInt32((long)media["idMal"]) : -1;
                if (malid > 0)
                {
                    this.SaveIDtoDatabase(Service.MyAnimeList,Service.AniList, malid, anilistid, type);
                    return malid;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        public int GetAniListIDFromMALID(int malid, MediaType type)
        {
            int titleid = (int)this.RetreiveSavedTargetIDFromServiceID(Service.AniList, Service.MyAnimeList, malid, type);
            if (titleid > -1)
            {
                return titleid;
            }
            String typestr = "";
            switch (type)
            {
                case MediaType.Anime:
                    typestr = "ANIME";
                    break;
                case MediaType.Manga:
                    typestr = "MANGA";
                    break;
                default:
                    break;
            }

            RestRequest request = new RestRequest("/", Method.POST);
            request.RequestFormat = DataFormat.Json;
            GraphQLQuery gquery = new GraphQLQuery();
            gquery.query = "query ($id: Int!, $type: MediaType) {\n  Media(idMal: $id, type: $type) {\n    id\n    idMal\n  }\n}";
            gquery.variables = new Dictionary<string, object> { { "id", malid.ToString() }, { "type", typestr } };
            request.AddJsonBody(gquery);
            IRestResponse response = raclient.Execute(request);
            if (response.StatusCode.GetHashCode() == 200)
            {
                Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                Dictionary<string, object> data = JObjectToDictionary((JObject)jsonData["data"]);
                Dictionary<string, object> media = JObjectToDictionary((JObject)data["Media"]);
                int anilistid = !object.ReferenceEquals(null, media["id"]) ? Convert.ToInt32((long)media["id"]) : -1;
                if (anilistid > 0)
                {
                    this.SaveIDtoDatabase(Service.AniList, Service.MyAnimeList, anilistid, malid, type);
                    return anilistid;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }

        }
        public Dictionary<string, object> RetreiveSavedIDsFromServiceID(Service listService, object titleid, MediaType type)
        {
            String sql = "";
            int mediatype = type == MediaType.Anime ? 0 : 1;
            string sourceidname = retrieveServiceIDFieldName(listService);
            sql = "SELECT * FROM titleids WHERE " + sourceidname + "= @param_val_1 AND mediatype = @param_val_2";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", titleid is int ? titleid.ToString() : titleid);
            cmd.Parameters.AddWithValue("@param_val_2", mediatype.GetHashCode());
            MySqlDataReader reader = cmd.ExecuteReader();
            bool isfound = false;
            Dictionary<string, object> result = null;
            try
            {
                while (reader.Read() && !isfound)
                {
                    if (reader["id"] != System.DBNull.Value)
                    {
                        if (type == MediaType.Anime)
                        {
                            result = new Dictionary<string, object> { { "anidb_id", reader["anidb_id"] }, { "anilist_id", reader["anilist_id"] }, { "kitsu_id", reader["kitsu_id"] }, { "mal_id", reader["malid"] }, { "notify_id", reader["notify_id"] } };
                        }
                        else
                        {
                            result = new Dictionary<string, object> { { "anilist_id", reader["anilist_id"] }, { "kitsu_id", reader["kitsu_id"] }, { "mal_id", reader["mal_id"] } };
                        }
                        isfound = true;
                    }
                }
            }
            finally
            {
                reader.Close();
            }
            return result;
        }

        public object RetreiveSavedTargetIDFromServiceID(Service targetservice , Service listService, object titleid, MediaType type)
        {
            String sql = "";
            int mediatype = type == MediaType.Anime ? 0 : 1;
            string targetidname = retrieveServiceIDFieldName(targetservice);
            string sourceidname = retrieveServiceIDFieldName (listService);
            sql = "SELECT " + targetidname + " FROM titleids WHERE " + sourceidname + "= @param_val_1 AND mediatype = @param_val_2";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", titleid is int ? titleid.ToString() : titleid);
            cmd.Parameters.AddWithValue("@param_val_2", mediatype.GetHashCode());
            MySqlDataReader reader = cmd.ExecuteReader();
            bool isfound = false;
            object foundtitleid = -1;
            try
            {
                while (reader.Read() && !isfound)
                {
                    if (reader[targetidname] != System.DBNull.Value)
                    {
                        foundtitleid = reader[targetidname];
                        isfound = true;
                    }
                }
            }
            finally
            {
                reader.Close();
            }
            return foundtitleid;
        }

        public void SaveIDtoDatabase(Service targetservice, Service listservice, object targettitleid, object servicetitleid, MediaType type)
        {
           object idrecord = this.CheckIfEntryExists(listservice, servicetitleid, targetservice, targettitleid, type);
           bool validid = idrecord is int ? ((int)idrecord > 0) : (((string)idrecord).Length > 0);
           if (validid)
           {
                string targetidname = retrieveServiceIDFieldName(targetservice);
                // Update entry
               String sql = "";
               int mediatype = type == MediaType.Anime ? 0 : 1;
               sql = "UPDATE titleids SET " + targetidname + " = @param_val_1 WHERE id = @param_val_2";
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@param_val_1", targettitleid is int ? targettitleid.ToString() : targettitleid);
                cmd.Parameters.AddWithValue("@param_val_2", idrecord is int ? idrecord.ToString() : idrecord);
                cmd.ExecuteNonQuery();
           }
           else
            {
                // Insert entry
                this.InsertIDtoDatabase(targetservice, listservice, targettitleid, servicetitleid, type);
            }
        }

        private void InsertIDtoDatabase(Service targetservice, Service listservice, object targettitleid, object servicetitleid, MediaType type)
        {
            string targetidname = retrieveServiceIDFieldName(targetservice);
            string sourceidname = retrieveServiceIDFieldName(listservice);
            String sql = "";
            int mediatype = type == MediaType.Anime ? 0 : 1;
            sql = "INSERT INTO titleids (" + targetidname + "," + sourceidname + ",mediatype) VALUES (@param_val_1,@param_val_2,@param_val_3)";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", targettitleid is int ? targettitleid.ToString() : targettitleid);
            cmd.Parameters.AddWithValue("@param_val_2", servicetitleid is int ? servicetitleid.ToString() : servicetitleid);
            cmd.Parameters.AddWithValue("@param_val_3", mediatype.GetHashCode());
            cmd.ExecuteNonQuery();
        }
        private object CheckIfEntryExists(Service targetservice, object targetid, MediaType type)
        {
            string targetidname = retrieveServiceIDFieldName(targetservice);
            int mediatype = type == MediaType.Anime ? 0 : 1;
            String sql = "SELECT id FROM titleids WHERE " + targetidname + " = @param_val_1 AND mediatype = @param_val_2";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", targetid is int ? targetid.ToString() : targetid);
            cmd.Parameters.AddWithValue("@param_val_2", mediatype.GetHashCode());
            MySqlDataReader reader = cmd.ExecuteReader();
            bool isfound = false;
            object foundid = -1;
            try
            {
                while (reader.Read() && !isfound)
                {
                    if (reader["id"] != System.DBNull.Value)
                    {
                        foundid = reader["id"];
                        isfound = true;
                    }
                }
            }
            finally
            {
                reader.Close();
            }
            return foundid;
        }

        private object CheckIfEntryExists(Service targetservice, object targetid, Service sourceservice, object sourceid, MediaType type)
        {
            string sourceidname = retrieveServiceIDFieldName(sourceservice);
            string targetidname = retrieveServiceIDFieldName(targetservice);
            int mediatype = type == MediaType.Anime ? 0 : 1;
            String sql = "SELECT id FROM titleids WHERE (" + targetidname + " = @param_val_1 OR " + sourceidname + " = @param_val_2 ) AND mediatype = @param_val_3";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", targetid is int ? targetid.ToString(): targetid);
            cmd.Parameters.AddWithValue("@param_val_2", sourceid is int ? targetid.ToString() : targetid);
            cmd.Parameters.AddWithValue("@param_val_3", mediatype.GetHashCode());
            MySqlDataReader reader = cmd.ExecuteReader();
            bool isfound = false;
            object foundid = -1;
            try
            {
                while (reader.Read() && !isfound)
                {
                    if (reader["id"] != System.DBNull.Value) { 
                        foundid = reader["id"];
                        isfound = true;
                    }
                }
            }
            finally
            {
                reader.Close();
            }
            return foundid;
        }

        private void initalizeDatabase()
        {
            try
            {
                connection = new MySqlConnection(hato.Helpers.ConnectionConfig.connectionstring());
                connection.Open();
                sqlliteinitalized = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                sqlliteinitalized = false;
            }
        }

        private Dictionary<string, object> JObjectToDictionary(JObject jobject)
        {
            return jobject.ToObject<Dictionary<string, object>>();
        }

        private string retrieveServiceIDFieldName(Service service)
        {
            switch (service)
            {
                case Service.AniDB:
                    return "anidb_id";
                case Service.AniList:
                    return "anilist_id";
                case Service.Kitsu:
                    return "kitsu_id";
                case Service.MyAnimeList:
                    return "malid";
                case Service.NotifyMoe:
                    return "notify_id";
                default:
                    return "";
            }
        }
        private string retrieveServiceName(Service service)
        {
            switch (service)
            {
                case Service.AniDB:
                    return "anidb";
                case Service.AniList:
                    return "anilist";
                case Service.Kitsu:
                    return "kitsu";
                case Service.MyAnimeList:
                    return "mal";
                default:
                    return "";
            }
        }
    }
}
