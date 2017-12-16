# SoftwareUpdater
Application for Windows which installs configured software silently and keeps it up to date!  

If you don't run the application in silent mode, it will prompt you to close the softwares you are currently updating, if they are currtly opened.
You can also configure softwares to get installed once and to not to receive updates.

You can specifiy the temporary folder (working folder) and the location of the configuration file.

The default configuration which comes with a blank installation is already configured to install and update the following softwares:
> 7-Zip  
> Acrobat Reader DC  
> Android Studio  
> Bitvise SSH Client  
> Brother MFL-Pro Suite (not silent)  

Planned softwares:
> Blizzard Battle.net Gameclient (not silent)
> Flash Player (not silent)
> CCleaner  
> Discord  
> Dropbox  
> Easy2Sync  
> Epic Games Launcher
> FileZilla  
> GIMP  
> Git  
> Chrome  
> HeidiSQL  
> IrfanView  
> JRE & JDK  
> KeePass  
> League of Legends  
> Lightshot  
> .NET Framework  
> Office Pro Plus  
> Security Essentials  
> SQL Server Management Studio  
> Visual Studio 2017 Enterprise  
> Visual Studio Code  
> Minecraft  
> Firefox  
> Nmap  
> Notepad++  
> OpenVPN  
> PuTTY  
> Razer Chroma  
> Razer Synapse  
> Remote Desktop Manager  
> SceneBuilder  
> SmartInspect and SmartInspect Pro  
> Spotify  
> Steam  
> TeamSpeak  
> TeamViewer  
> PLC Utility  
> VLC Media player  
> WhatsApp  
> Yatqa  
> Windows Essentials  

Softwares, which can't be installed/updated silently:
> Flash Player (Requires a enterprise access - which is not allowed to be public, but you can request one - to deploy (the msi installers) silent)  
> Blizzard Battle.net Gameclient (Does not support a silent installation)  
> Brother MFL-Pro Suite  

Let me know whats your wish! Contact: marvin.klar@yahoo.de



## Download

You can download the latest stable build of the application here:  
https://klar.ddns.net/download/SoftwareUpdater/SoftwareUpdater.exe  

And the default configuration here (the application downloads and uses the default configuration, if no other configutaion is defined):  
https://klar.ddns.net/download/SoftwareUpdater/config.xml  



## Start arguments
> -tempDir <Path of the temporary folder>
> Example: -tempDir "C:\Windows\Temp\"
>
> -configFile <Path to the configuration file>
> Example: -configFile "C:\Users\yourUserName\Documents\SoftwareUpdater\config.xml"
>
> -silent ``Installs and updates the given softwares silently without a window``
>
> -clearCache ```Clears the generated cache (downloaded installation files which could not be installed). Use this option, if you have defective installation files (e.g. when the download was canceled)``



## Configuration

The configuration is a little tricky to understand. The configuration is XML based, so make sure to follow the rules for XML files: https://www.w3schools.com/xml/xml_syntax.asp

### Activate/Deactivate the software
Commonly you just need to know, that a software updates itself, if the attribute ``active`` is set to ``true``.  
Example:
>``<software active="true">``

All softwares are commonly so configured that you don't need to change there anything. If you want to add own software make sure to read the ``hints`` at the bottom of the page.

### Disable updates for a software
If you don't want to update a software (e.g. if you just have a licence for a specific version or if the software has an own updater) you can disable updates for a software whilst setting the attribute ``update`` to ``false`` like this:
>``<software active="true" update="false">``


### Uninstall the software before update
You can force the application to uninstall the software before updating the software whilst setting attribute ``requiresUninstall`` of the ``software`` node to ``true``. If nothing or ``false`` is specified the application will just install the software when updating it.  
Example:
>``<software active="true" requiresUninstall="true">``

### The name of the software
The ``name`` attribute is just for you to know which software you configure right now, and later when the software gets installed/updated which software was installed/updated or not.  
Example:
>``<name>7-Zip</name>``

### The local installation of the software
The ``installPath`` node is needed for the application to know, which version of the software is currently installed. Provide the path of the application (included the application filename itself).  
Example:
>``<installPath>C:\Program Files\7-Zip\7zG.exe</installPath>``

### The installation arguments
The ``installationArguments`` node defines the arguments, which are required to install the software unattended. The value ``%installationfile%`` will be replaced with the path of the installation file. You can configure the arguments for your needs.
Example:
>``<installationArguments>%installationfile% /S</installationArguments>``

### The processes of the software
The ``processes`` node defines all processes, which might be started by the software, when the software runs. The processes are comma seperated. The application checks, if any of the processes listed here are running, before updating the configured software. Make sure, that these processes are not running when the software should be updated. But if the application will skip these softwares in silent mode or ask the user to close the processes in normal mode.  
Example:
>``<processes>7zFM,7zG,7z</processes>``

### The website of the software
The ``website`` node is used to get the latest version number of the software and to get the download link of the software. So make sure that the website contains both, the latest version number and the download link for the latest version of the software.

### The version string
The ``versionString`` node describes a string, which represents a part of the source code (press ```Ctrl + U`` to view the source code) of the website, which contains the latest version. If this part on the website changes, the software will be updated. Make sure to replace the version with a star (``*``) like this:
>``<versionString>7-Zip * (</versionString>``
The application will search for the first occurence of the string on the website and use it to parse the latest version number.

### The download link
The ``downloadLink`` node works similar like the ``versionString`` node. The applcation searches for the first occurence of the string on the website and uses it to download the software. If the download link is not on the website, you can simply specify the link and let the application replace the star (``*``) to the latest version. You can also specify the ``versionFormat`` attribute to specify the format of the version of software in the downloadlink. You can use the following varaibles and other chars (like e.g. dots): ``%major% (the major versionumber), %minor% (the minor versionumber), %revision% (the revision number) and %build% (the build number)``  
With the ``validateVersion`` attribute you can specify, if only valid version numbers should be used for the url. If so, e.g. ``1.09.2`` will be converted to ``1.9.2``. So only deactivate this, if you really want to use the original version of the software from the website.  
Important: The download link must be a direct download link! If you only have the link of the website, which starts the download, start the download once (you can cancel the download of the software) and then go to you download history in your browser and search for the direct download link of the installer.
Example:
>``<downloadLink versionFormat="%major%%minor%%revision%%build%" validateVersion="false">http://7-zip.org/a/7z*-x64.exe</downloadLink>``

That's it! Easy, huh?


## Hints for configuring own softwares

When you want to add own softwares to install/update make sure to specify all required nodes in the XML file and follow the XML rules.
Keep in mind that the website of the software you configure might change. If you have successfully configured a software make sure to share it (simply send me a email with the configuration to ``marvin.klar@yahoo.de``). I will add the software to the default configuration. But if you failed let me know about that - I'm glad to help you to set up your software! 



## Planned implementations
- Logging
- Possiblity to configure params of the installer of the softwares
- Language support
- Possiblity to configure the way to get the installed version of the software (productversion, fileversion, registry)



## Bugs, issues or simply nice ideas

If you have any trouble using the software or missing any feature, let me know about that (in english or german):  
Issues, bugs: https://github.com/MarvinKlar/SoftwareUpdater/issues  
Questions, feature requests or feedback: marvin.klar@yahoo.de  
(I commonly respond/react within minutes - it should never take more than 12 hours)  