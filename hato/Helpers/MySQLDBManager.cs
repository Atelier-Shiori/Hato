using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace hato.Helpers
{
    public class MySQLDBManager
    {
        MySqlConnection connection;
        public bool initalized;

        public MySQLDBManager(String connectionstring)
        {
            initalizeDatabase(connectionstring);
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

        public object RetreiveSavedTargetIDFromServiceID(Service targetservice, Service listService, object titleid, MediaType type)
        {
            String sql = "";
            int mediatype = type == MediaType.Anime ? 0 : 1;
            string targetidname = retrieveServiceIDFieldName(targetservice);
            string sourceidname = retrieveServiceIDFieldName(listService);
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

        public object CheckIfEntryExists(Service targetservice, object targetid, MediaType type)
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

        public object CheckIfEntryExists(Service targetservice, object targetid, Service sourceservice, object sourceid, MediaType type)
        {
            string sourceidname = retrieveServiceIDFieldName(sourceservice);
            string targetidname = retrieveServiceIDFieldName(targetservice);
            int mediatype = type == MediaType.Anime ? 0 : 1;
            String sql = "SELECT id FROM titleids WHERE (" + targetidname + " = @param_val_1 OR " + sourceidname + " = @param_val_2 ) AND mediatype = @param_val_3";
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@param_val_1", targetid is int ? targetid.ToString() : targetid);
            cmd.Parameters.AddWithValue("@param_val_2", sourceid is int ? targetid.ToString() : targetid);
            cmd.Parameters.AddWithValue("@param_val_3", mediatype.GetHashCode());
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

        private void initalizeDatabase(String connectionstring)
        {
            try
            {
                connection = new MySqlConnection(connectionstring);
                connection.Open();
                initalized = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                initalized = false;
            }
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
