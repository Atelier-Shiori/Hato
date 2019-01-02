# Hato
Hato is a REST API built on ASP.NET Core and written in C#. It allows developers to easily look up title identifiers for a certain title on different listing services to increase interoperability for things like exporting lists, list syncing and updating entries on different services of the same title. The mappings are stored in a database so that title id mappings can be retrieved without a lookup.

# Supported Services
Currently, Hato supports conversion of title ids of the following:
* AniList (Anime/Manga)
* Kitsu (Anime/Manga)
* MyAnimeList (Anime/Manga)

Coming Soon:
* Notify.moe (Anime)
* AniDB (Anime)

# How to Use

## Endpoints
```
GET http://localhost:50420/api/mappings/(service)/(media type)/(id)
```
### Parameters

| Parameter | Value | Required |
|:---|:---|:---|
| service | `mal` or `kitsu` or `anilist` | `true` |
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
        "anilist_id":21238,
        "kitsu_id":11134,
        "mal_id":31080
        }
}
```
### How to install
The instructions are in beta, to be changed in the future.

1. Clone repo
2. Execute setupschema.sql to setup the schema on your MySQL or MariaDB server.
3. Navigate to the Hato > Helpers folder in the cloned repository. Rename ConnectionConfig-samplecs to ConnectionConfig.cs
4. Open ConnectionConfig.cs and specify the database server, database name, database user and password.
5. Compile the application.
6. To deploy on Linux with Apache, navigate to the /bin/release/<target_framework_moniker>/. Type the following in the terminal.
```
dotnet publish --configuration Release
```

# License
Hato is licensed under Apache License 2.0.ß
