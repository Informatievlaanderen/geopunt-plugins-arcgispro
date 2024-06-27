using GeoPunt.Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GeoPunt.DataHandler
{
    internal class wfsHelper
    {
        public WebClient client;

        public wfsHelper(string proxyUrl, int port, int timeout)
        {
            init(proxyUrl, port, timeout);
        }
        public wfsHelper(int timeout)
        {
            init("", 80, timeout);
        }
        public wfsHelper()
        {
            init("", 80, 5000);
        }

        private void init(string proxyUrl, int port, int timeout)
        {
            if (proxyUrl == null || proxyUrl == "")
            {
                client = new gpWebClient() { Encoding = System.Text.Encoding.UTF8, timeout = timeout };
            }
            else
            {
                client = new gpWebClient()
                {
                    Encoding = System.Text.Encoding.UTF8,
                    Proxy = new System.Net.WebProxy(proxyUrl, port),
                    timeout = timeout
                };
            }
            client.Headers["Content-type"] = "application/json";
        }


        

        public Dictionary<string, string> getFeatureTypes(string url)
        {
            string wfsUrl = string.Format(CultureInfo.InvariantCulture, $@"{url}?SERVICE=WFS&REQUEST=GetCapabilities");
            string xmlResponse = client.DownloadString(new Uri(wfsUrl));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            XmlElement root = doc.DocumentElement;
            XmlNodeList featureTypeList = root["FeatureTypeList"].ChildNodes;
           
            Dictionary<string,string> featureTypes = new Dictionary<string, string>();

            foreach (XmlNode featureType in featureTypeList)
            {
                featureTypes[featureType["Name"].InnerText.Substring(featureType["Name"].InnerText.IndexOf(":") + 1)] = featureType["Title"].InnerXml;
            }
            return featureTypes;
        }
    }


}
