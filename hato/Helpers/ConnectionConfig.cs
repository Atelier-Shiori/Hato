/* ConnectionConfig.cs
 * This class specifies the database settings.
 * 
 * Copyright (c) 2018-2019 MAL Updater OS X Group, a division of Moy IT Solutions
 * Licensed under Apache License 2.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace hato.Helpers
{
    public class ConnectionConfig
    {
        // Specify database settings

        // Do not modify anything below this line.
		public static String connectionstring ()
		{
            //dbsettings settings = settings.Values;
            //return "SERVER =" + settings.dbhost + ";DATABASE=" + settings.dbname + ";UID=" + settings.dbuser + ";PASSWORD=" + settings.dbpassword + ";";
            return "";
        }
    }
}
