/* MappingsController
 * Responsible for the mapping api.
 * 
 * Copyright (c) 2018-2019 MAL Updater OS X Group, a division of Moy IT Solutions
 * Licensed under Apache License 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hato.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace hato.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class MappingsController : ControllerBase
    {
        private readonly UserAgentControl allowedclients;
        private readonly dbsettings dbsettings;

        public MappingsController()
        {
            var collection = new ServiceCollection();
            collection.AddOptions();
            var Configuration = new ConfigurationBuilder()
#if DEBUG
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
#else
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#endif
            .Build();
            collection.Configure<UserAgentControl>(Configuration.GetSection("UserAgentControl"));
            collection.Configure<dbsettings>(Configuration.GetSection("dbsettings"));
            var services = collection.BuildServiceProvider();
            allowedclients = services.GetService<IOptions<UserAgentControl>>().Value;
            dbsettings = services.GetService<IOptions<dbsettings>>().Value;
        }
        // GET api/mappings/(service)/(type)/(id)
        [HttpGet("{service}/{type}/{id}")]
        public ActionResult<JsonResult> Get(string service, string type, string id)
        {
            if (!this.checkClient())
            {
                return Unauthorized();
            }
            TitleIdMapping mapping = new TitleIdMapping(service, type, id, this.dbsettings);
            if (!mapping.errored)
            {
                mapping.performLookup();
                if (mapping.errored)
                {
                    BadRequestObjectResult result = BadRequest(mapping.output);
                    mapping.Dispose();
                    return result;
                }
                else if (mapping.notfound)
                {
                    NotFoundObjectResult result = NotFound(mapping.output);
                    mapping.Dispose();
                    return result;
                }
                else
                {
                    OkObjectResult result = Ok(mapping.output);
                    mapping.Dispose();
                    return result;
                }
            }
            else
            {
                BadRequestObjectResult result = BadRequest(mapping.output);
                mapping.Dispose();
                return result;
            }

        }

        [HttpPost("mappings")]
        public ActionResult<JsonResult> Post([FromBody] mappingsrequest mappings)
        {
            if (!this.checkClient())
            {
                return Unauthorized();
            }
            if (mappings.title_ids.Count > 10)
            {
                return BadRequest(new Dictionary<string, object> { { "data", null }, { "error", "Too many title ids (" + mappings.title_ids.Count.ToString() + "). The maximum is 10." } });
            }
            List<Dictionary<string, object>> tmpmappings = new List<Dictionary<string, object>>();
            List<object> failedtitleids = new List<object>();
            foreach (object titleid in mappings.title_ids)
            {
                TitleIdMapping mapping = new TitleIdMapping(mappings.service, mappings.media_type, titleid is int ? ((int)titleid).ToString() : titleid is long ? ((long)titleid).ToString() :  (string)titleid, this.dbsettings);
                if (!mapping.errored)
                {
                    mapping.performLookup();
                    if (mapping.errored || mapping.notfound)
                    {
                        mapping.Dispose();
                        failedtitleids.Add(titleid);
                    }
                    else
                    {
                        tmpmappings.Add(mapping.generatemapdictionary());
                    }
                }
                else
                {
                    return BadRequest(new Dictionary<string, object> { { "data", null }, { "error", "Unable to connect to database." } });
                } 
            }
            return Ok(new Dictionary<string, object> { { "data", tmpmappings }, { "failed_list" , failedtitleids } });
        }
        
        private bool checkClient()
        {
            string userAgent = Request.Headers["User-Agent"];
            bool usingagentcheck = allowedclients.CheckClients;
            if (usingagentcheck)
            {
                foreach (string clientstr in allowedclients.AllowedClients)
                {
                    if (userAgent.Contains(clientstr,StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }
    }
}
