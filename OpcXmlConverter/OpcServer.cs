using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;

namespace OpcXmlConverter
{
    class OpcServer
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        //private static string server = String.Format("opcda:/{0}", DatabaseInteractor.GetServerName());

        public static object ReadTag(string serverName,string opcTag = "")
        {
            Uri url = UrlBuilder.Build(serverName);
            object readString;
            using (var client = new OpcDaServer(url))
            {
                client.Connect();
                var g1 = client.AddGroup("g1");
                g1.IsActive = true;
                var item = new[]
                {
                    new OpcDaItemDefinition
                    {
                        ItemId = opcTag,
                        IsActive = true
                    }
                };
                g1.AddItems(item);
                try
                {
                    readString = g1.Read(g1.Items)[0].Value;
                    return readString;
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Error reading Tag {0}", opcTag));
                    log.Error(e);
                    return null;
                }
            }
        }

        internal static void WriteTag(string serverName,string opcTag, object valueToWrite)
        {
            Uri url = UrlBuilder.Build(serverName);
            using (var client = new OpcDaServer(url))
            {
                client.Connect();
                var g1 = client.AddGroup("g1");
                var item = new[]
                {
                    new OpcDaItemDefinition
                    {
                        ItemId = opcTag,
                        IsActive = true
                    }
                };
                g1.AddItems(item);
                object[] writeObject = { valueToWrite };
                try
                {
                    g1.Write(g1.Items, writeObject);
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Error writing Tag {opcTag}", opcTag));
                    log.Error(e);
                }
            }

        }
    }
}
