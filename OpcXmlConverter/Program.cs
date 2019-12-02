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
        private static String tagFile = @"./tagMap.csv";
        private static String opcServerName = "SV.OPCDAServer.1";

        /**
         * args: sitePrefix, path to XML, save/read
         */
        static void Main(string[] args)
        {
            //Checks to see if the arguments is exactly 2. If it is zero, then exist the program
            if (args.Length != 3)
            {
                log.InfoFormat("Not Enough arguments");
                return;
            }
            //Checks if the tagFile exists or not. Return if it doesn't exists 
            if (!File.Exists(tagFile))
            {
                log.ErrorFormat("The tagMap.csv file doesn't exist in the current directory");
                return;
            }

            String siteName = args[0];
            String uccXmlFilePath = args[1];
            String option = args[2].ToLower();
            ArrayList xmlArrayList = new ArrayList();
            var client = new EasyDAClient();

            //reads from the xmlMap.csv file and turns it into a dictionary where all the XML tags are stored as keys and Opc tags are stored as values
            Dictionary<String, String> xmlOpcDict = File.ReadLines(tagFile).Select(line => line.Split(',')).ToDictionary(line => line[0], line => siteName + line[1]);

            //Loop through the dictionary and store all the keys into an arraylist. 
            //You'll need this array as comparison wwhen looping the XML tags
            foreach (KeyValuePair<String, String> tag in xmlOpcDict)
                xmlArrayList.Add(tag.Key);

            //Load the AGC XML documentation
            loadXmlConfigFile();
            XmlDocument uccAgcXmlFile = new XmlDocument();
            uccAgcXmlFile.Load(uccXmlFilePath);

            //Because the XML uses a namespace, you need to handle it with a NamespaceManager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(uccAgcXmlFile.NameTable);
            nsmgr.AddNamespace("df", "http://tempuri.org/dtsConfigurac.xsd");

            //Get the list of XML tag nodes from the default settings section
            XmlNodeList xmlNodeList = uccAgcXmlFile.SelectNodes("//df:ValoresIniciales/*", nsmgr);

            //This is testing to see what key value pair are actually in the dictionary
            foreach (KeyValuePair<String, String> tag in xmlOpcDict)
            {
                Console.WriteLine("xmlTag={0}, opcTag={1}", tag.Key, tag.Value);
                log.InfoFormat("xmlTag={0}, opcTag={1}", tag.Key, tag.Value);
            }

            /*
             * Loop through all the XML tags in the default parameter section.
             * If the XML tag is in the xmlArrayList, then you need to write the OPC value into said XML
             * 
             */
            foreach (XmlNode node in xmlNodeList)
            {
                String nodeName = node.Name;
                Console.WriteLine("XML node name: " + nodeName);
                log.DebugFormat("XML node name: {0}", nodeName);
                log.InfoFormat("XML node name: {0}", nodeName);
                if (xmlArrayList.Contains(nodeName))
                {
                    //Write to output the key of the xmlOPCDict. YOu should replace this with logging in the furture
                    Console.WriteLine(xmlOpcDict[nodeName]);

                    //Attempt to get the OPC Value. If the OPC Value doesn't exist (or if someone fatfingered the server name)
                    //Log the exception 
                    try
                    {
                        object valueFromXmlFile;
                        //Value is the value fetched from the XML
                        try
                        {
                            valueFromXmlFile = client.ReadItemValue("", opcServerName, xmlOpcDict[nodeName]);
                        }
                        catch (Exception e)
                        {
                            log.ErrorFormat("Tag {0} cannot be read from {1}", xmlOpcDict[nodeName], opcServerName);
                            log.ErrorFormat("Error: {0}", e);
                        }
                        Console.WriteLine("opcServer: " + opcServerName + " nodeName: " + nodeName + " xmlopcDict: " + xmlOpcDict[nodeName]);
                        log.InfoFormat("opcServer: {0} nodeName: {1} xmlopcDict: {2}", opcServerName, nodeName, xmlOpcDict[nodeName]);

                        if (valueFromXmlFile != null)
                        {
                            // Read item value and display it 
                            Console.WriteLine(valueFromXmlFile.ToString());
                            log.InfoFormat("tag: {0} value: {1}", xmlOpcDict[nodeName], valueFromXmlFile.ToString());

                            //If the user write from the OPC tag to the XML
                            if (option.Equals("save"))
                                writeToXmlFile(valueFromXmlFile, node);

                            //If the user only wants to read from the XML and write to the default OPC tags
                            else if (option.Equals("read"))
                                readFromXmlFile(String someString,);
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Error Logged. Is the OPC server up?");
                        log.ErrorFormat(" Error: \n {0}", e);
                        Console.WriteLine(e);
                    }
                }
            }

            //Save the xml...but only if option is save
            if (option.Equals("save"))
            {
                uccAgcXmlFile.Save(uccXmlFilePath);
                log.InfoFormat("XML Saved!");
            }
        }

        public static void writeToXmlFile(object valueFromXmlFile, XmlNode node)
        {
            String xmlValue;
            //Hanlde the cases differently if the values read from the OPC tags are boolean
            // Probably should be in a tenary statement, but I'm feeling lazy
            if (valueFromXmlFile.ToString().ToLower().Equals("true"))
            {
                xmlValue = "1";
            }
            else if (valueFromXmlFile.ToString().ToLower().Equals("false"))
            {
                xmlValue = "0";
            }
            else
            {
                //Write the value to the XML. Uncomment once you're ready
                xmlValue = valueFromXmlFile.ToString();
            }
            Console.WriteLine("Node value: " + node.Value + "\nxmlValue: " + xmlValue);
            Console.WriteLine("Node Inner text: " + node.InnerText);

            log.InfoFormat("Node value: {0} xmlValue: {1}", node.Value, xmlValue);
            log.InfoFormat("Node Inner text: {0}", node.InnerText);
            node.InnerText = xmlValue;
        }

        public static void readFromXmlFile(String agcOpcTag, XmlNode node, EasyDAClient client)
        {
            //prints the OPC tag and its default value 
            Console.WriteLine("OPC Tag: " + agcOpcTag + " default: " + node.InnerText);
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

            //Reads from the XML and writes to the custom tags that victor set up

        }
        public void saveXML(XmlDocument uccAgcXmlFile, String uccXmlFilePath)
        {
            uccAgcXmlFile.Save(uccXmlFilePath);
            log.InfoFormat("XML Saved!");
        }

    }
}