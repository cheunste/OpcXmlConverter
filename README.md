# OpcXmlConverter

This is cli tool that allows a user to read the content of the AGC XML file and write it to OPC tag and vice versa

This is designed with Wind Node in mind. 

# Install Prerequisites

- .NET 4.7.1 (or higher). This is needed because of the OPC library
- OPC Server (Pretty much means PcVue)
- The tagMap.csv file in the same directory as the exe. This is included by default. More on this [the tagmap section](#TagMap)

# How to Install
1. Verify that .NET 4.7.1 is installed on the server
2. Grab a copy of the folder OpcXML from the nasshare in (\_Installs_CORE\OpcXML) and put it in the AGC folder on the server
3. Modify the opcXML.bat file and modify the line:
   **OpcXmlConverter.exe "[SITE PREFIX]" "[FULL PATH TO XML]" "read"**
   - Change the "[SITE PREFIX]" to the site prefix with quotes. For example, if your site is Junca, replace [site Name] with "JUNCA"
   - Change the "[FULL PATH TO XML]" to the path of the XML along with the file extention. This is typically the absolute path in the form "D:\XXXXXXXXXX\XXXXXXXXX\WINDAGC\DatosConfiguracion_XXXXXXXXX.xml"
   - For servers that have multiple AGCs, you need to copy and paste the command line with different prefixes and paths to XML
4. Install the OPC tags on a server.
   - Log onto the Gepora server
   - Grab a copy of the spreadsheet WF_AGC_DEF_rev1 from another site (doesn't matter which) and import it to a new site
   - Export the AGCDEF tags from Gepora
   - Load the tags to the new server
5. Install the AgcDef.pvb file on the server
   - grab a copy of the AgcDef.pvb file. This can be found in the UCC folder. Copying the UCC folder to the destination server is optional
   - Put it in the project's P/ folder
   - In the PVB, verify the filepath in the strLink variable. This is needed in case the AGC folder is using an old path naming convention (WindAGCPF vs WindAGC) different or if there are multiple sites on a UCC (see big horn)
   - Update the existing Redundancia.pvb file with the reference to the AgcDef.pvb by adding the following
   ```BASIC
    IntVal = PROGRAM("PRELOAD","AgcDef.pvb", "");
    PRINT("AGC Default");
   ```
6. Modify the existing EVENT.dat file

   - Add the following line in the project's Event.dat file

   ```BASIC
   EVTPROG,AGC_DEF_XXXXX,"",0,0,"","","XXXXX.WF.AGCDEF.ReadDefaultSettings",2,1,"SYSTEM.HISTORICO.HABILITA","ALL>1","AgcDef.pvb","","ReadAgcDef",""
   ```

   - Replace the XXXXX with the site acronym (ie JUNCA). Note there are two "XXXXX" to replace in the above statement
   - Note that The above will allow read events. Write events are not requested and are dangerous. However, in case a design change occurs and we decide to allow NCC to overwrite the default AGCXML, add the following

   ```BASIC
   EVTPROG,AGC_DEF_XXXXX,"",0,0,"","","XXXXX.WF.AGCDEF.WriteDefaultSettings",2,1,"SYSTEM.HISTORICO.HABILITA","ALL>1","AgcDef.pvb","","ReadAgcDef",""
   ```

7. Depending on the XML tags you want to read, you may need to modify the tagMap.csv file. See [the tagmap section](#TagMap) for more details

# How to use

In powershell, type in the following (assuming you're in the same directory as the program). 

./OpcXmlConverter.exe [site name] [full path to xml] [save/read]

ex:
./OpcXmlConverter.exe TESTSITE "D:\Program Files\IBERINCO\WINDAGC\DatosConfiguracion_XXXXXXXXX.xml"

If you're executing this in another script, you will need the full path of the exe file instead.

Further more, I have also included the opcXML.bat file along with the program. All you need to do is execute the opcXML.bat file and it will then read the XML file and write to the desired OPC tag

# TagMap (aka How to modify tags)

The tagMap.csv is a csv file that maps a default windAGC XML tag to its respective OPC Tag.

This file **must be in the same directory as the exe file**.

This allows users to add or remove default tags to save to WindAGC by simply editing the csv file 

- The left column is for **XML tags**
- The right column is for the **OPC tag without the siteName**

example:

| XML Tag		|OPC Tag			|
| --- | --- |
| AGC_ON		|.WF.AGCDEF.PlWAtv.actSt	|
| AGC_MODO		|.WF.AGCDEF.PlWMod.actSt	|
| SelectorConsigna	|.WF.AGCDEF.PlWSrcAtv.actSt	|
| DesactivarPF		|.WF.AGCDEF.PlHzAtv.actSt	|
| ModoPF		|.WF.AGCDEF.PlHzMod.actSt	|
| PotenciaNominal	|.WF.AGCDEF.PotenciaNominal		|

- **However** please note following:
	- Both XML and the OPC Tags are independent of the site name. The site name is added as a parameter input
	- In the OPC Tag section, all tags **MUST** start with a '.'
	- There is no header in the tagMap.csv file (that means don't add the "XML Tag" and "OPC Tag" to the tagMap.csv)

