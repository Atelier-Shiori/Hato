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

namespace hato.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class MappingsController : ControllerBase
    {

        // GET api/mappings/(service)/(type)/(id)
        [HttpGet("{service}/{type}/{id:int}")]
        public ActionResult<JsonResult> Get(string service, string type, int id)
        {
            int AniListID = 0, MALID = 0, KitsuID = 0;
            Service currentservice;
            MediaType currenttype;
			TitleIDConverter converter = new TitleIDConverter();
            if (converter.sqlliteinitalized) {
                switch (type)
                {
                    case "anime":
                        currenttype = MediaType.Anime;
                        break;
                    case "manga":
                        currenttype = MediaType.Manga;
                        break;
                    default:
						converter.Dispose();
                        Dictionary<string, object> erroroutput = new Dictionary<string, object> { { "data", null }, { "error", "Invalid media type. Type must be anime or manga." } };
                        return BadRequest(erroroutput);

                }
                switch (service)
                {
                    case "anilist":
                        AniListID = id;
                        currentservice = Service.AniList;
                        break;
                    case "mal":
                        MALID = id;
                        currentservice = Service.MyAnimeList;
                        break;
                    case "kitsu":
                        KitsuID = id;
                        currentservice = Service.Kitsu;
                        break;
                    default:
						converter.Dispose();
                        Dictionary<string, object> erroroutput = new Dictionary<string, object> { { "data", null }, { "error", "Invalid service. Services accepted are anilist, kitsu, and mal" } };
                        return BadRequest(erroroutput);
                }
                if (id > 0)
                {
                    bool notfound = false;
                    switch (currentservice)
                    {
                        case Service.AniList:
                            MALID = converter.GetMALIDFromAniListID(AniListID, currenttype);
                            if (MALID > 0) {
                                KitsuID = converter.GetKitsuIDFromMALID(MALID, currenttype);
                            }
                            else
                            {
                                notfound = true;
                            }
                            break;
                        case Service.MyAnimeList:
                            AniListID = converter.GetAniListIDFromMALID(MALID, currenttype);
                            KitsuID = converter.GetKitsuIDFromMALID(MALID, currenttype);
                            if (AniListID < 1 && KitsuID < 1)
                            {
                                notfound = true;
                            }
                            break;
                        case Service.Kitsu:
                            MALID = converter.GetMALIDFromKitsuID(KitsuID, currenttype);
                            if (MALID > 0)
                            {
                                AniListID = converter.GetAniListIDFromMALID(MALID, currenttype);
                            }
                            else
                            {
                                notfound = true;
                            }
                            break;
                    }
                    if (notfound)
                    {
						converter.Dispose();
                        Dictionary<string, object> erroroutput = new Dictionary<string, object> { { "data", null }, { "error", "Nothing found for " + service + " title id: " + id.ToString() } };
                        return NotFound(erroroutput);
                    }
                    else
                    {
						converter.Dispose();
                        Dictionary<string, object> titleidlist = new Dictionary<string, object> { { "anilist_id", AniListID }, { "kitsu_id", KitsuID }, { "mal_id", MALID }, { "type", currenttype }, { "type_str", type } };
                        return Ok(new Dictionary<string, object> { { "data", titleidlist } });
                    }
                }
                else
                {
                    Dictionary<string, object> erroroutput = new Dictionary<string, object> { { "data", null }, { "error", "Id must be greater than 0." } };
                    return BadRequest(erroroutput);
                }
            }
			converter.Dispose();
            Dictionary<string, object> eoutput = new Dictionary<string, object> { { "data", null }, { "error", "Can't connect to database." } };
            return BadRequest(eoutput);
        }
    }
}
