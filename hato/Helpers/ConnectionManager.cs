using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hato.Helpers
{
    public class ConnectionManager
    {
        private readonly dbsettings settings;
        private MySQLDBManager mysqldbmgr;
        private PostgresDBManager postgresdbmgr;

        public ConnectionManager(dbsettings settings)
        {
            this.settings = settings;
            String connectionString;
            switch (this.settings.dbengine)
            {
                case "mysql":
                    connectionString = "SERVER =" + this.settings.dbhost + ";DATABASE=" + this.settings.dbname + ";UID=" + this.settings.dbuser + ";PASSWORD=" + this.settings.dbpassword + ";";
                    mysqldbmgr = new MySQLDBManager(connectionString);
                    break;
                case "postgres":
                    connectionString = "SERVER=" + this.settings.dbhost + ";User Id=" + this.settings.dbuser + ";Password=" + this.settings.dbpassword + ";Database=" + this.settings.dbname;
                    postgresdbmgr = new PostgresDBManager(connectionString);
                    break;
                default:
                    throw new System.ArgumentException("Invalid engine type" + this.settings.dbengine);
            }
        }

        public void Dispose()
        {
            switch (this.settings.dbengine)
            {
                case "mysql":
                    mysqldbmgr.Dispose();
                    break;
                case "postgres":
                    postgresdbmgr.Dispose();
                    break;
            }
        }
        
        public bool isInitalized()
        {
            switch (this.settings.dbengine)
            {
                case "mysql":
                    return mysqldbmgr.initalized;
                case "postgres":
                    return postgresdbmgr.initalized;
            }
            return false;
        }

        public Dictionary<string, object> RetreiveSavedIDsFromServiceID(Service listService, object titleid, MediaType type)
        {
            switch (this.settings.dbengine)
            {
                case "mysql":
                    return mysqldbmgr.RetreiveSavedIDsFromServiceID(listService, titleid, type);
                case "postgres":
                    return postgresdbmgr.RetreiveSavedIDsFromServiceID(listService, titleid, type);
            }
            return null;
        }

        public object RetreiveSavedTargetIDFromServiceID(Service targetservice, Service listService, object titleid, MediaType type)
        {
            switch (this.settings.dbengine)
            {
                case "mysql":
                    return mysqldbmgr.RetreiveSavedTargetIDFromServiceID(targetservice, listService, titleid, type);
                case "postgres":
                    return postgresdbmgr.RetreiveSavedTargetIDFromServiceID(targetservice, listService, titleid, type);
            }
            return null;
        }

        public void SaveIDtoDatabase(Service targetservice, Service listservice, object targettitleid, object servicetitleid, MediaType type)
        {
            switch (this.settings.dbengine)
            {
                case "mysql": 
                    mysqldbmgr.SaveIDtoDatabase(targetservice, listservice, targettitleid, servicetitleid, type);
                    break;
                case "postgres":
                    postgresdbmgr.SaveIDtoDatabase(targetservice, listservice, targettitleid, servicetitleid, type);
                    break;
            }
        }

        public object CheckIfEntryExists(Service targetservice, object targetid, MediaType type)
        {
            switch (this.settings.dbengine)
            {
                case "mysql":
                    return mysqldbmgr.CheckIfEntryExists(targetservice, targetid, type);
                case "postgres":
                    return postgresdbmgr.CheckIfEntryExists(targetservice, targetid, type);
            }
            return null;
        }

        public object CheckIfEntryExists(Service targetservice, object targetid, Service sourceservice, object sourceid, MediaType type)
        {
            switch (this.settings.dbengine)
            {
                case "mysql":
                    return mysqldbmgr.CheckIfEntryExists(targetservice, targetid, sourceservice, sourceid, type);
                case "postgres":
                    return postgresdbmgr.CheckIfEntryExists(targetservice, targetid, sourceservice, sourceid, type);
            }
            return null;
        }

    }
}
