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

namespace hato.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class MappingsController : ControllerBase
    {
        private readonly UserAgentControl allowedclients;

        public MappingsController(IOptions<UserAgentControl> allowedclients)
        {
            this.allowedclients = allowedclients.Value;
        }
        // GET api/mappings/(service)/(type)/(id)
        [HttpGet("{service}/{type}/{id}")]
        public ActionResult<JsonResult> Get(string service, string type, string id)
        {
            if (!this.checkClient())
            {
                return Unauthorized();
            }
            TitleIdMapping mapping = new TitleIdMapping(service, type, id);
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
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
