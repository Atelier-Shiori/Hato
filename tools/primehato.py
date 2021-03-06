import sys
import requests
import time

instance = "http://localhost:50420"
filename = sys.argv[1]
site = sys.argv[2]
mediatype = sys.argv[3]

print("Filename:" + filename)
print("Media Type" + mediatype)
print("Site Source:" + site)

if len(filename) > 0 and (mediatype == "anime" or mediatype == "manga") and len(site) > 0:
    titleids = []
    with open(filename, 'r') as filehandler:
        for line in filehandler:
            currentid = line[:-1]
            titleids.append(currentid)
        
    for titleid in titleids:
        URL = instance + "/api/mappings/" + site + "/" + mediatype + "/" + titleid
        request = requests.get(url = URL)
        data = request.json()
        print(data);
        time.sleep(2)

else:
    print("File name or mediatype and site missing or invalid. Usage: python primehato.py <filename containing title ids seperated by line> (mal|kitsu|anilist) (anime|manga)")
    
