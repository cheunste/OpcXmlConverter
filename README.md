# OpcXmlConverter

This is cli tool that allows a user to read the content of the AGC XML file and write it to OPC tag and vice versa

This is designed with Wind Node in mind. 

# Install Prerequisites

- .NET 4.7.1 (or higher). This is needed because of the OPC library
- OPC Server (Pretty much means PcVue)
- The tagMap.csv file in the same directory as the exe. This is included by default. More on this [the tagmap section](#TagMap)

# How to Install

1) Verify that .NET 4.7.1 is installed on the server
2) Grab a copy of the folder OpcXML from the nasshare  in (_CORE\OpcXML) and put it in the AGC folder on the server (technically, this can be run anywhere on the server)
3) Modify the opcXML.bat file and modify the line:
	**OpcXmlConverter.exe "[SITE PREFIX]" "[FULL PATH TO XML]" "read"**
	i) Change the "[SITE PREFIX]" to the site prefix with quotes. For example, if your site is Junca, replace [site Name] with "JUNCA"
	ii) Change the "[FULL PATH TO XML]" to the path of the XML along with the file extention.  This is typically in the form "D:\XXXXXXXXXX\XXXXXXXXX\WINDAGC\DatosConfiguracion_XXXXXXXXX.xml"
	iii) For servers that have multiple AGCs, you need to copy and paste the command line with different prefixes and paths to XML
4) Depending on the XML tags you want to read, you may need to modify the tagMap.csv file. See [the tagmap section](#TagMap) for more details

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

