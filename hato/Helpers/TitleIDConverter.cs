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

            }
        }
        
        public int GetMALIDFromKitsuID(int kitsuid, MediaType type)
        {
            int titleid = this.RetreiveSavedTargetIDFromServiceID(Service.MyAnimeList,Service.Kitsu, kitsuid, type);
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
            int titleid = this.RetreiveSavedTargetIDFromServiceID(Service.Kitsu,Service.MyAnimeList,malid, type);
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
            int titleid = this.RetreiveSavedTargetIDFromServiceID(Service.MyAnimeList,Service.AniList, anilistid, type);
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
            int titleid = this.RetreiveSavedTargetIDFromServiceID(Service.AniList, Service.MyAnimeList, malid, type);
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

        public int RetreiveSavedTargetIDFromServiceID(Service targetservice , Service listService, int titleid, MediaType type)
        {
            String sql = "";
            int mediatype = type == MediaType.Anime ? 0 : 1;
            string targetidname = retrieveServiceIDFieldName(targetservice);
            string sourceidname = retrieveServiceIDFieldName (listService);
            sql = "SELECT " + targetidname + " FROM titleids WHERE " + sourceidname + "= @param_val_1 AND mediatype = @param_val_2";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", titleid.ToString());
            cmd.Parameters.AddWithValue("@param_val_2", mediatype.GetHashCode());
            MySqlDataReader reader = cmd.ExecuteReader();
            bool isfound = false;
            int foundtitleid = -1;
            try
            {
                while (reader.Read() && !isfound)
                {
                    if (reader[targetidname] != System.DBNull.Value)
                    {
                        foundtitleid = (int)reader[targetidname];
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

        public void SaveIDtoDatabase(Service targetservice, Service listservice, int targettitleid, int servicetitleid, MediaType type)
        {
           int idrecord = this.CheckIfEntryExists(listservice, servicetitleid, type);
           if (idrecord > 0)
           {
                string targetidname = retrieveServiceIDFieldName(targetservice);
                // Update entry
               String sql = "";
               int mediatype = type == MediaType.Anime ? 0 : 1;
               sql = "UPDATE titleids SET " + targetidname + " = @param_val_1 WHERE id = @param_val_2";
               MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@param_val_1", targettitleid.ToString());
                cmd.Parameters.AddWithValue("@param_val_2", idrecord.ToString());
                cmd.ExecuteNonQuery();
           }
           else
            {
                // Insert entry
                this.InsertIDtoDatabase(targetservice, listservice, targettitleid, servicetitleid, type);
            }

        }

        private void InsertIDtoDatabase(Service targetservice, Service listservice, int targettitleid, int servicetitleid, MediaType type)
        {
            string targetidname = retrieveServiceIDFieldName(targetservice);
            string sourceidname = retrieveServiceIDFieldName(listservice);
            String sql = "";
            int mediatype = type == MediaType.Anime ? 0 : 1;
            sql = "INSERT INTO titleids (" + targetidname + "," + sourceidname + ",mediatype) VALUES (@param_val_1,@param_val_2,@param_val_3)";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", targettitleid.ToString());
            cmd.Parameters.AddWithValue("@param_val_2", servicetitleid.ToString());
            cmd.Parameters.AddWithValue("@param_val_3", mediatype.GetHashCode());
            cmd.ExecuteNonQuery();
        }

        private int CheckIfEntryExists(Service targetservice, int targetid, MediaType type)
        {
            string targetidname = retrieveServiceIDFieldName(targetservice);
            int mediatype = type == MediaType.Anime ? 0 : 1;
            String sql = "SELECT id FROM titleids WHERE " + targetidname + " = @param_val_1 AND mediatype = @param_val_2";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", targetid.ToString());
            cmd.Parameters.AddWithValue("@param_val_2", mediatype.GetHashCode());
            MySqlDataReader reader = cmd.ExecuteReader();
            bool isfound = false;
            int foundid = -1;
            try
            {
                while (reader.Read() && !isfound)
                {
                    if (reader["id"] != System.DBNull.Value) { 
                        foundid = (int)reader["id"];
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
                default:
                    return "";
            }
        }
    }
}
