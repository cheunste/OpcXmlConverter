using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using OpcLabs.BaseLib.Collections.Extensions;
using log4net;
using OpcLabs.EasyOpc.DataAccess;

namespace OpcXmlConverter
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        //tagFile is a mandatory csv that is used to map OPC tag to the XML tag in the Wind NOde XML
        private static String OpcTagCsvFile = @"./tagMap.csv";
        private static String opcServerName = "SV.OPCDAServer.1";
        private static readonly int CORRECT_ARGUMENT_LENGTH = 3;
        private static EasyDAClient client;

        /**
         * args: sitePrefix, path to XML, save/read
         */
        static void Main(string[] args)
        {
#if DEBUG
            args = new[] { "SCRAB", "./DatosConfiguracion_SCRAB.xml", "read" };
#endif
            String siteName = args[0];
            String uccXmlFilePath = args[1];
            String option = args[2].ToLower();
            ArrayList agcXmlElementList = new ArrayList();
            client = new EasyDAClient();
            object valueFromXmlFile = null;

            if (args.Length != CORRECT_ARGUMENT_LENGTH)
            {
                log.InfoFormat("Not Enough arguments");
                return;
            }
            if (!File.Exists(OpcTagCsvFile))
            {
                log.ErrorFormat("The file tagMap.csv doesn't exist in the current directory. Cannot run without it.");
                return;
            }

            //reads from the xmlMap.csv file and turns it into a dictionary where all the XML tags are stored as keys and Opc tags are stored as values
            //Then store the store all the xml element into an arraylist for easier access
            Dictionary<String, String> xmlOpcDict = File.ReadLines(OpcTagCsvFile).Select(line => line.Split(',')).ToDictionary(line => line[0], line => siteName + line[1]);
            foreach (KeyValuePair<String, String> tag in xmlOpcDict)
                agcXmlElementList.Add(tag.Key);

            XmlDocument uccAgcXmlFile = new XmlDocument();
            uccAgcXmlFile.Load(uccXmlFilePath);

            //The namespace of the xml settings file is http://tempuri.org/dtsConfigurac.xsd as odd as that is
            XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(uccAgcXmlFile.NameTable);
            nameSpaceManager.AddNamespace("df", "http://tempuri.org/dtsConfigurac.xsd");

            //Get the list of XML tag nodes from the settings section of the XML file. Which is in the ValoresIniciales element
            XmlNodeList xmlNodeList = uccAgcXmlFile.SelectNodes("//df:ValoresIniciales/*", nameSpaceManager);

            verifyElementsInXmlOpcDict(xmlOpcDict);

            foreach (XmlNode element in xmlNodeList)
            {
                String agcXmlElementName = element.Name;
                Console.WriteLine("XML node name: " + agcXmlElementName);
                log.DebugFormat("XML node name: {0}", agcXmlElementName);
                log.InfoFormat("XML node name: {0}", agcXmlElementName);
                string opcTagInXmlFile = xmlOpcDict[agcXmlElementName];

                if (agcXmlElementList.Contains(agcXmlElementName))
                {
                    valueFromXmlFile = readElementValueFromXmlFile(opcTagInXmlFile);
                    log.InfoFormat("opcServer: {0} nodeName: {1} xmlopcDict: {2}", opcServerName, agcXmlElementName, opcTagInXmlFile);

                    if (valueFromXmlFile != null)
                    {
                        // Read item value and display it 
                        Console.WriteLine(valueFromXmlFile.ToString());
                        log.InfoFormat("tag: {0} value: {1}", opcTagInXmlFile, valueFromXmlFile.ToString());

                        //If the user write from the OPC tag to the XML
                        if (option.Equals("save"))
                            writeToXmlFile(valueFromXmlFile, element);

                        //If the user only wants to read from the XML and write to the default OPC tags
                        else if (option.Equals("read"))
                            readFromXmlFile(opcTagInXmlFile, element, client);
                    }
                }
            }

            if (option.Equals("save"))
                saveXml(uccAgcXmlFile, uccXmlFilePath);
        }

        private static void verifyElementsInXmlOpcDict(Dictionary<string, string> xmlOpcDict)
        {
            foreach (KeyValuePair<String, String> element in xmlOpcDict)
            {
                Console.WriteLine("xmlTag={0}, opcTag={1}", element.Key, element.Value);
                log.InfoFormat("xmlTag={0}, opcTag={1}", element.Key, element.Value);
            }
        }

        private static object readElementValueFromXmlFile(string opcTagInXmlFile)
        {
            //Value is the value fetched from the XML
            try
            {
                return client.ReadItemValue("", opcServerName, opcTagInXmlFile);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Tag {0} cannot be read from {1}", opcTagInXmlFile, opcServerName);
                log.ErrorFormat("Error: {0}", e);
                return null;
            }
        }

        public static void writeToXmlFile(object valueFromXmlFile, XmlNode node)
        {
            String xmlValue;
            //Hanlde the cases differently if the values read from the OPC tags are boolean. Otherwise, it won't be a boolean and will be a regular string.
            if (valueFromXmlFile.ToString().ToLower().Equals("true"))
                xmlValue = "1";
            else if (valueFromXmlFile.ToString().ToLower().Equals("false"))
                xmlValue = "0";
            else
                xmlValue = valueFromXmlFile.ToString();
            log.InfoFormat("Node value: {0} xmlValue: {1}", node.Value, xmlValue);
            log.InfoFormat("Node Inner text: {0}", node.InnerText);
            node.InnerText = xmlValue;
        }

        public static void readFromXmlFile(String agcOpcTag, XmlNode node, EasyDAClient client)
        {
            log.InfoFormat("Writing {0} to {1}", node.InnerText, agcOpcTag);

            try
            {
                client.WriteItemValue(opcServerName, agcOpcTag, node.InnerText);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Cannot write {0} to tag {1} cannot be written to {2}", node.InnerText, agcOpcTag, opcServerName);
                log.ErrorFormat("Error: {0}", e);
            }
        }
        public static void saveXml(XmlDocument uccAgcXmlFile, String uccXmlFilePath)
        {
            uccAgcXmlFile.Save(uccXmlFilePath);
            log.InfoFormat("XML Saved!");
        }

    }
}