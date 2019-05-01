# Installing Hato on Apache and Linux
This installation guide shows you how to install Hato on a Apache web server on Ubuntu 16.04 LTS.

## 1. Install .NET Core
If you haven't already, you need to install .NET Core on Ubuntu. The following instructions are from Microsoft.

### Register Microsoft key and feed

Before installing .NET, you'll need to register the Microsoft key, register the product repository, and install required dependencies. This only needs to be done once per machine.

Open a command prompt and run the following commands:
```
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
```

### Install the .NET SDK

Update the products available for installation, then install the .NET SDK.

In your command prompt, run the following commands:
```
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

## 2. Retrieve the source
Go to the /var/www directory. Clone the Hato repository by typing the following with root privilages

```
sudo su
cd /var/www
git clone https://github.com/Atelier-Shiori/Hato.git

```
## 3. Creating a database configuration
Before you can publish the Hato service, you need to create a database configuration service. 

### Import Hato schema
```
cd /var/www/hato
mysql -u root -p < setupschema.sql
```

### Log into MySQL
```
mysql -u root -p
```

### Create a Mysql Database User
```
CREATE USER 'hato'@'localhost' IDENTIFIED BY '(generated password here)';
GRANT INSERT ON hato.* TO ‘hato’@'localhost’;
GRANT DELETE ON hato.* TO ‘hato’@'localhost’;
GRANT SELECT ON hato.* TO ‘hato’@'localhost’;
GRANT UPDATE ON hato.* TO ‘hato’@'localhost’;
exit;
```

### Create the Database Configuration file
```
cd /var/www/hato/hato/
mv appsettings-sample.json appsettings.json 
mv appsettings.Development-sample.json appsettings.Development.json
nano ConnectionConfig.cs
```

You should only modify the following as seen below. You should only need to add the generated password to mysqlpassword constant.
```
// Specify database settings
// Note: You should execute setupschema.sql before setting up this script.
private const String mysqlserver = "localhost";
private const String mysqldatabase = "hato";
private const String mysqlusername = "hato";
private const String mysqlpassword = "";
```

## 4. Publish the app
Before you can deploy the app, you need to publish it. You can do this by running the following with root privilages:
```
cd /var/www/hato
dotnet publish --configuration Release
```

## 5. Create System.d service
Hato runs as a service which Apache with a virtual site will access as a reserve proxy. All the requests Hato recieves from Apache will get processed and the response sent back to Apache, which it will get served to an application.

To create a service, run the following in the terminal:
```
sudo nano /etc/systemd/system/hato.service
```

Paste the following into hato.service and save the file.
```
[Unit]
Description=Hato Service Web API

[Service]
WorkingDirectory=/var/www/hato/bin/Release/netcoreapp2.1/publish
ExecStart=/usr/bin/dotnet /var/www/hato/bin/Release/netcoreapp2.1/publish/hato.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=hato-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Enable the service by running the following:
```
sudo systemctl enable hato.service
```

Start and verify that Hato is running.

```
sudo service hato start
```

## 6. Configure Apache
Make sure mod_proxy, mod_proxy_http is enabled. You can do this by running the following commands.
```
sudo a2enmod proxy
sudo a2enmod proxy_http
```

In `/etc/apache2/sites-available`, create a site configuration file called hato.conf with the following. (Change the server name to the domain name where you will host the service)
```
<VirtualHost *:*>
ServerName (domain name)
RequestHeader set "X-Forwarded-Proto" expr=%{REQUEST_SCHEME}
</VirtualHost>
<VirtualHost *:80>
ServerName (domain name)
ProxyPreserveHost On
ProxyPass / http://localhost:50420/
ProxyPassReverse / http://localhost:50420/
ErrorLog ${APACHE_LOG_DIR}hato-error.log
CustomLog ${APACHE_LOG_DIR}hato-access.log common
</VirtualHost>
```

To enable the site, run the following
```
sudo a2ensite hato.conf
```

Go to your web browser and navigate to `http://(domain name)`. The domain dame is where you host the Hato service. If you see the Hato introduction page, the service is running correctly.


## 7. Securing Hato (optional)
It's recommended to use HTTPS to do any requests between your application and Hato. You can use the Let's Encrypt service to retrieve a free SSL certificate. You can do this by following [these instructions](https://www.digitalocean.com/community/tutorials/how-to-secure-apache-with-let-s-encrypt-on-ubuntu-16-04).

# Updating Hato
Hato recieves updates on a frequent basis. You can easily upgrade your copy of Hato by running the following commands:
```
sudo su
service hato stop
cd /var/www/hato
git pull
dotnet publish --configuration Release
service hato start
exit
```
