# Summary #
Statusnet (Identica) has a Jabber / XMPP integration. XMPPdeamon.php runs only on linux. This project is a MS Windows service which provides all functionalities described here : http://status.net/wiki/Jabber_Bot


---


# Instalation #
  * Download latest binaries from http://code.google.com/p/statusnet-windows-jabber-bot/downloads/list
  * Copy all files from zip archive to custom folder. ex. C:\Program Files\StatusNetJabberBot
  * Copy statusnet\_jabber\_service\_api.php file to Statusnet installation **scrips** directory
  * Edit config file **StatusNetJabberBot.exe.config**
  * Run **installService.bat** file to install Windows Service
  * Go to windows services console and start the service
  * Jabber account should be Online (look at your IM client)
  * Configure Statusnet Jabber - http://status.net/wiki/Jabber_Setup
  * Add IM account to your Statusnet account and confirm it
  * Send first command to bot (example: groups)