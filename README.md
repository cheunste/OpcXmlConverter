# OpcXmlConverter

This is cli tool that allows my coworkers to save OPC tags to an already existing XML file. 

Supposed to be designed with Wind Node (some internal tool) in mind. 

# Install Prerequisites

- .NET 4.7.1 (or higher). This is needed because of the OPC library
- OPC Server. 

# How to use

In powershell, type in the following (assuming you're in the same directory as the program)

./OpcXmlConverter.exe [site name] [full path to xml]

ex:

./OpcXmlConverter.exe JUNCA "D:\Program Files\IBERINCO\WINDAGC\DatosConfiguracion_NombreParque.xml"
