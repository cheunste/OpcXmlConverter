using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using OpcLabs.BaseLib.Collections.Extensions;
using OpcLabs.EasyOpc.DataAccess;

namespace OpcXmlConverter
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("How to use: OpcXmlConverter.exe [site Abbrivation] [file path to XML] ");
                return;
            }

            //Get the siteName from the argument
            String siteName = args[0];
            //"C:\\Users\\Stephen\\Documents\\OPCPowershell\\AGC.xml";
            String filePath = args[1];

            // Create an OPC client
            var client = new EasyDAClient();
            //The OPC server name. In my experience, this is consistent for all sites
            String opcServer = "SV.OPCDAServer.1";


            //Create an arrayList for quick access
            ArrayList xmlArrayList = new ArrayList();

            //This is a script that maps  the XML tag to OPC tags
            //Probably should be in some sort of config file so I don't have to recompile this everytime Victor makes a request
            Dictionary<String, String> xmlOpcDict = new Dictionary<String, String>
            {
                { "AGC_ON",                     siteName+".WF.WAPC2_1.PlWAtv.actSt" },
                { "AGC_MODO",                   siteName+".WF.WAPC2_1.PlWMod.actSt" },
                { "SelectorConsigna",           siteName+".WF.WAPC2_1.PlWSrcAtv.actSt"},
                { "DesactivarPF",               siteName+".WF.WAPC2_1.PlHzAtv.actSt"},
                { "ModoPF",                     siteName+".WF.WAPC2_1.PlHzMod.actSt"},
                { "PotenciaNominal",            siteName+".PotenciaNominal"}
            };

            //Loop through the dictionary and store all the keys into an arraylist. 
            //You'll need this array as comparison wwhen looping the XML tags
            foreach (KeyValuePair<String, String> tag in xmlOpcDict)
            {
                xmlArrayList.Add(tag.Key);
            }

            //Load the XML documentation
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //Because the XML uses a namespace, you need to handle it
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("df", "http://tempuri.org/dtsConfigurac.xsd");

            //Get the list of XML tag nodes from the default settings section
            XmlNodeList nl = doc.SelectNodes("//df:ValoresIniciales/*", nsmgr);

            //This is testing to see what key value pair are actually in the dictionary
            foreach (KeyValuePair<String, String> tag in xmlOpcDict)
            {
                Console.WriteLine("xmlTag={0}, opcTag={1}", tag.Key, tag.Value);
            }


            /*
             * Loop through all the XML tags in the default parameter section.
             * If the XML tag is in the xmlArrayList, then you need to write the OPC value into said XML
             * 
             */
            foreach (XmlNode node in nl)
            {
                String nodeName = node.Name;
                Console.WriteLine("XML node name: " + nodeName);
                if (xmlArrayList.Contains(nodeName))
                {
                    //Write to output the key of the xmlOPCDict. YOu should replace this with logging in the furture
                    Console.WriteLine(xmlOpcDict[nodeName]);

                    //Attempt to get the OPC Value. If the OPC Value doesn't exist (or if someone fatfingered the server name)
                    //Log the exception 
                    try
                    {
                        object value;
                        value = client.ReadItemValue("", opcServer, xmlOpcDict[nodeName]);
                        Console.WriteLine("opcServer: " + opcServer + "\n nodeName: " + nodeName + "\nxmlopcDict: " + xmlOpcDict[nodeName]);
                        if (value != null)
                        {
                            // Read item value and display it in a message box
                            Console.WriteLine(value.ToString());
                            String xmlValue;
                            //Hanlde the cases differently if the values read from the OPC tags are boolean
                            if (value.ToString().ToLower().Equals("true"))
                            {
                                xmlValue = "1";
                            }
                            else if (value.ToString().ToLower().Equals("false"))
                            {
                                xmlValue = "0";
                            }
                            else
                            {
                                //Write the value to the XML. Uncomment once you're ready
                                xmlValue = value.ToString();
                            }
                            Console.WriteLine("Node value: " + node.Value + "\nxmlValue: " + xmlValue);
                            Console.WriteLine("Node Inner text: " + node.InnerText);
                            node.InnerText = xmlValue;

                        }
                    }
                    catch (Exception e)
                    {
                        //Probably should be logging this, if you ever figure it out
                        Console.WriteLine(e);
                    }
                }
            }
            //Save the xml
            doc.Save(filePath);
        }
    }
}