/* mappingsrequest.cs
 * This class specifies the mapping request object.
 * 
 * Copyright (c) 2018-2019 MAL Updater OS X Group, a division of Moy IT Solutions
 * Licensed under Apache License 2.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hato.Helpers
{
    public class mappingsrequest
    {
        public string media_type;
        public string service;
        public List<object> title_ids;
    }
}
