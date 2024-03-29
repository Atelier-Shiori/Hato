# Hato
## Notice: Do not report mapping errors in the issue. Please use [this form](https://forms.office.com/Pages/ResponsePage.aspx?id=foLJ9a3qM0GhTLPBKYvjoJQLJAD3OI1Mi_kkp4Law6BUOFo0V0oyOU8xMjVQUlhMS09VTk1DVkJESS4u) instead.

Hato (鳩 lit. Pigeon) is a REST API built on ASP.NET Core and written in C#. It allows developers to easily look up title identifiers for a certain title and media type (Anime or Manga) on different listing services to increase interoperability for things like exporting lists, list syncing and updating entries on different services of the same title. The mappings are stored in a database so that title id mappings can be retrieved quickly without performing another lookup.

## Why Hato over ARM ([Anime Relations Mapper](https://github.com/p-chan/arm))?
1. It doesn't depend on one service - Besides using Notify to generate a title id mapping for anime, it uses Kitsu and AniList APIs to look up title ids as a fallback, just in case if the title is not on Notify.
2. Hato supports Manga mappings.
3. With a database backend, Hato can easily retrieve existing mappings relatively quickly. Title ID mappings are very unlikely to change over time, except for media discovery services adding titles that didn't exist previously.
4. C# is better than Javascript and with .NET 8, it's faster than nodeJS with crossplatform compatability.
5. Hato can fetch multiple title ids from one service and media type.

## Why Hato over [anime offline database](https://github.com/manami-project/anime-offline-database)?
1. You are not restricted to the AGPL License and the data is not licensed in any way.
2. Hato automatically feteches and updates the latest data as users look up title ids.
3. No need to fetch the whole database, retrieve mappings you only need using the REST JSON API.
4. Hato can fetch mappings for Manga, which the anime offline database does not do.
5. Hato can fetch multiple title ids from one service and media type.

## Supported Services
Currently, Hato supports conversion of title ids of the following:
* [AniList](https://anilist.co) (Anime/Manga)
* [Kitsu](https://kitsu.io) (Anime/Manga)
* [MyAnimeList](https://myanimelist.net) (Anime/Manga)
* [Notify.moe](https://notify.moe) (Anime)
* [AniDB](https://anidb.net) (Anime)

## Requirements
* .NET 8 or later
* Windows 10 or later, macOS Catalina or later, or Linux (Ubuntu 20.04 or later, Red Hat Enterprise/CentOS 8+, Debian 9+)
* MySQL Server 5.7 or later or Postgresql.
* Apache or Nginx with proxy support.

## How to Use
Note: The production endpoint (https://hato.malupdaterosx.moe) as shown below is restricted to only testing use (through the testing page) and approved applications. In order to have your application (user agent) approved, you must be an active patron. You can become a patreon [here](https://www.patreon.com/bePatron?u=4748653&redirect_uri=https%3A%2F%2Fmalupdaterosx.moe%2Fdonate%2F&utm_medium=widget)

### Endpoints
```
GET http://hato.malupdaterosx.moe/api/mappings/(service)/(media type)/(id)
```
#### Parameters

| Parameter | Value | Required |
|:---|:---|:---|
| service | `mal` or `kitsu` or `anilist` (Anime only: `anidb` or `notify`)| `true` |
| media type | MediaType (`anime` or `manga`) | `true` |
| id | number | `true` |

#### Example
```
[GET] http://hato.malupdaterosx.moe/api/mappings/kitsu/anime/11134
```

##### Response
```
{
    "data":{
        "anidb_id":11321,
        "anilist_id":21238,
        "kitsu_id":11134,
        "mal_id":31080,
        "notify_id":"090XtKimg",
        "type" : 0,
        "type_str" : "anime"
    }
}
```

```
POST https://hato.malupdaterosx.moe/api/mappings/mappings
```
#### Parameters

##### Body
Content Type: application/json
```
{
"media_type" : "anime",
"service" : "mal",
"title_ids" : [1, 31080, 37786, 25835, 21681, 24705, 22147, 20509, 10928, 19815]
}
```

#### Example
```
[GET] http://hato.malupdaterosx.moe/api/mappings/kitsu/anime/11134
```

##### Request Body
Content Type: application/json
```
{
"media_type" : "anime",
"service" : "mal",
"title_ids" : [1, 31080, 37786, 25835, 21681, 24705, 22147, 20509, 10928, 19815]
}
```

##### Response
```
{
"data": [
{
"anidb_id": 23,
"anilist_id": 1,
"kitsu_id": 1,
"mal_id": 1,
"notify_id": "Tk3ccKimg",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 11321,
"anilist_id": 21238,
"kitsu_id": 11134,
"mal_id": 31080,
"notify_id": "090XtKimg",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": null,
"anilist_id": 101573,
"kitsu_id": 41240,
"mal_id": 37786,
"notify_id": "ytl6tj7mg",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 10779,
"anilist_id": 20812,
"kitsu_id": 8698,
"mal_id": 25835,
"notify_id": "jlx1pFmiR",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 10351,
"anilist_id": 20575,
"kitsu_id": 8138,
"mal_id": 21681,
"notify_id": "ylgTtFmmg",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 10323,
"anilist_id": 20565,
"kitsu_id": 8710,
"mal_id": 24705,
"notify_id": "2SLJtFiiR",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 10399,
"anilist_id": 20602,
"kitsu_id": 8259,
"mal_id": 22147,
"notify_id": "ua1opKmmR",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 10091,
"anilist_id": 20467,
"kitsu_id": 8003,
"mal_id": 20509,
"notify_id": "sThhpFiiR",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 9155,
"anilist_id": 10928,
"kitsu_id": 6410,
"mal_id": 10928,
"notify_id": "qqgStKmiR",
"type": 0,
"type_str": "anime"
},
{
"anidb_id": 9998,
"anilist_id": 19815,
"kitsu_id": 7880,
"mal_id": 19815,
"notify_id": "cErppFmiR",
"type": 0,
"type_str": "anime"
}
],
"failed_list": []
}
```
## How to install
The instructions are in beta, to be changed in the future.

Note: Database configuration is moved to the appsettings.json. For those who are updating, make sure you respecify the database settings in the appsettings.json. See the sample config file for details.

1. Clone repo
2. Execute setupschema-mysql.sql to setup the schema on your MySQL or MariaDB server. For Postgres, run setupschema-postgres.sql.
3. Navigate to the Hato folder and copy and rename appsettings-sample.json or appsettings.Development-sample.json to appsettings.json or appsettings.Development.json respectively.
4. Open appsettings.json/appsettings.Development.json and specify the database engine, database server, database name, database user and password in the dbsettings section. For supported database engines, Hato supports MySQL and Postgresql.
5. To deploy on your enviroment of choice, refer to these guides:
* [Ubuntu and Apache](https://github.com/Atelier-Shiori/Hato/blob/master/InstallingApacheAndUbuntu.md)
* [Red Hat Linux and Apache](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-2.2)
* [Ubuntu Linux and Ngnix](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-2.2)
* [Windows Service](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-2.2)

## License
Hato is licensed under Apache License 2.0.
