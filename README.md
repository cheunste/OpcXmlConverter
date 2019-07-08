# OpcXmlConverter

This is cli tool that allows my coworkers to save OPC tags to an already existing XML file. 

Supposed to be designed with Wind Node (some internal tool) in mind. 

# Install Prerequisites

- .NET 4.7.1 (or higher). This is needed because of the OPC library
- OPC Server (Pretty much means PcVue)
- The tagMap.csv file in the same directory as the exe. More on this [the tagmap section](#TagMap)

# How to use

In powershell, type in the following (assuming you're in the same directory as the program). 

./OpcXmlConverter.exe [site name] [full path to xml] [save/read]

ex:
./OpcXmlConverter.exe TESTSITE "D:\Program Files\IBERINCO\WINDAGC\DatosConfiguracion_XXXXXXXXX.xml"

If you're executing this in another script, you will need the full path of the exe file instead.

# TagMap

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
	- Both XML and the OPC Tags are independent of the site name. The site name 
	- In the OPC Tag section, all tags **MUST** start with a '.'
	- There is no header in the tagMap.csv file

