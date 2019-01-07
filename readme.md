# Hato
Hato (鳩 lit. Pigeon) is a REST API built on ASP.NET Core and written in C#. It allows developers to easily look up title identifiers for a certain title and media type (Anime or Manga) on different listing services to increase interoperability for things like exporting lists, list syncing and updating entries on different services of the same title. The mappings are stored in a database so that title id mappings can be retrieved quickly without performing another lookup.

# Why Hato over ARM (Anime Relations Mapper)
1. It doesn't depend on one service - Besides using Notify to generate a title id mapping for anime, it uses Kitsu and AniList APIs to loook up title ids as a fallback, just in case if the title is not on Notify.
2. Hato supports Manga mappings.
3. With a database backend, Hato can easily retrieve existing mappings relatively quickly. Title ID mappings are very unlikely to change over time, except for media discovery services adding titles that didn't exist previously.

# Supported Services
Currently, Hato supports conversion of title ids of the following:
* [AniList](https://anilist.co) (Anime/Manga)
* [Kitsu](https://kitsu.io) (Anime/Manga)
* [MyAnimeList](https://myanimelist.net) (Anime/Manga)
* [Notify.moe](https://notify.moe) (Anime)
* [AniDB](https://anidb.net) (Anime)

# How to Use

## Endpoints
```
GET http://localhost:50420/api/mappings/(service)/(media type)/(id)
```
### Parameters

| Parameter | Value | Required |
|:---|:---|:---|
| service | `mal` or `kitsu` or `anilist` (Anime only: `anidb` or `notify`)| `true` |
| media type | MediaType (`anime` or `manga`) | `true` |
| id | number | `true` |

### Example
```
[GET] http://localhost:50420/api/mappings/kitsu/anime/11134
```

#### Response
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
### How to install
The instructions are in beta, to be changed in the future.

1. Clone repo
2. Execute setupschema.sql to setup the schema on your MySQL or MariaDB server.
3. Navigate to the Hato > Helpers folder in the cloned repository. Rename ConnectionConfig-samplecs to ConnectionConfig.cs
4. Open ConnectionConfig.cs and specify the database server, database name, database user and password.
5. To deploy on your enviroment of choice, refer to these guides:
* [Linux and Apache](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-2.2)
* [Linux and Ngnix](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-2.2)
* [Windows Service](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-2.2)

# License
Hato is licensed under Apache License 2.0.
