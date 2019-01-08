# About Tools
These tools can help you set up your Hato instance

## primehato.py
This Python script allows you to automate the lookup of title ids to generate mappings beforehand. This will reduce the need to perform mapping generation in the future and improve performance.

Running this script requires Python 3 and the requests package installed. You can install the necessary packages by running the following:
```
pip install requests
```
**Note:** On some installations, you may need to use `pip3` command instead of `pip`.

### Running the script
Usage:
```
python primehato.py <filename containing title ids seperated by line> (mal|kitsu|anilist) (anime|manga)
```
**Note:** The file that contains title ids are deliminated by line breaks. In other words, one title id per line.

**Note 2:** Disable user agent check before running this script. This can be done by modifying the `appsettings.json` file before deploying. Change "CheckClients" value to false to disable the user agent check. 
