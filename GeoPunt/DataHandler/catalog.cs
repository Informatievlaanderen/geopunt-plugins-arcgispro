using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using GeoPunt.Share;
using Newtonsoft.Json;
using System.Web;

namespace GeoPunt.DataHandler
{
    class catalog
    {
        public WebClient client;
        NameValueCollection qryValues;

        public string geoNetworkUrl = "https://metadata.vlaanderen.be/srv/dut";

        public Dictionary<string, string> dataTypes = new Dictionary<string, string>() {
                   {"Dataset","dataset"}, {"Datasetserie","series"},
                   {"Objectencatalogus","model"}, {"Service","service"} };


        public catalog(string proxyUrl = "", int port = 80, int timeout = 5000)
        {
            this.init(proxyUrl, port, timeout);
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
            qryValues = new NameValueCollection();
        }

        public List<string> getKeyWords(string q = "", string field = "anylight")
        {
            qryValues.Add("q", q);
            qryValues.Add("field", field);
            client.QueryString = qryValues;

            string url = geoNetworkUrl + "/suggest";

            string jsonString = client.DownloadString(new Uri(url));
            JArray jsonArr = JArray.Parse(jsonString) as JArray;
            JArray keywords = (JArray)jsonArr[1];

            qryValues.Clear();
            client.QueryString.Clear();
            return keywords.Select(c => (string)c).ToList();
        }

        public List<string> getOrganisations()
        {
            string url = geoNetworkUrl + @"/suggest?field=orgName";
            string jsonString = client.DownloadString(new Uri(url));
            JArray jsonArr = JArray.Parse(jsonString) as JArray;
            JArray orgs = (JArray)jsonArr[1];
            return orgs.Select(c => (string)c).ToList();
        }

        public List<string> inspireKeywords()
        {
            List<string> keywords = new List<string>();
            string url = geoNetworkUrl + "/q?_content_type=xml&fast=index&resultType=details&to=1";
            string xmlDoc = client.DownloadString(new Uri(url));
            XElement element = XElement.Parse(xmlDoc);
            IEnumerable<XElement> dims = element.Element("summary").Elements("dimension");
            foreach (var dim in dims)
            {
                if (dim.Attribute("name").Value == "inspireTheme")
                    foreach (var cat in dim.Elements("category"))
                        keywords.Add(cat.Attribute("value").Value);
            }
            return keywords;
        }

        public Dictionary<string, string> getSources()
        {
            Dictionary<string, string> sourcesDict = new Dictionary<string, string>();
            string url = geoNetworkUrl + "/q?_content_type=xml&fast=index&resultType=details&to=1";

            string xmlDoc = client.DownloadString(new Uri(url));
            XElement element = XElement.Parse(xmlDoc);
            IEnumerable<XElement> dims = element.Element("summary").Elements("dimension");
            foreach (var dim in dims)
            {
                if (dim.Attribute("name").Value == "sourceCatalog")
                    foreach (var cat in dim.Elements("category"))
                        sourcesDict.Add(cat.Attribute("value").Value, cat.Attribute("label").Value);
            }
            return sourcesDict;
        }

        public List<string> getGDIthemes()
        {
            string url = geoNetworkUrl + @"/suggest?field=flanderskeyword";
            string jsonString = client.DownloadString(new Uri(url));
            JArray jsonArr = JArray.Parse(jsonString) as JArray;
            JArray GDIthemes = (JArray)jsonArr[1];
            return GDIthemes.Select(c => (string)c).ToList();
        }

        public datacontract.metadataResponse search(string q = "", int start = 1, int to = 21,
           string themekey = "", string orgName = "", string dataType = "", string siteId = "", string inspiretheme = "")
        {
            qryValues.Add("fast", "index");
            qryValues.Add("_content_type", "json");
            qryValues.Add("sortBy", "changeDate");
            if (q != "" && q != null) qryValues.Add("any", q);
            qryValues.Add("from", start.ToString());
            qryValues.Add("to", to.ToString());

            var facets = new List<string>();
            if (themekey != "" && themekey != null) facets.Add("flanderskeyword/" + themekey);
            if (orgName != "" && orgName != null) facets.Add("orgName/" + orgName);
            if (dataType != "" && dataType != null) facets.Add("type/" + dataType);
            if (siteId != "" && siteId != null) facets.Add("sourceCatalog/" + siteId);
            if (inspiretheme != "" && inspiretheme != null) facets.Add("inspireTheme/" + inspiretheme);

            if (facets.Count() > 0)
            {
                qryValues.Add("facet.q", HttpUtility.UrlEncode(String.Join("&", facets.Select(facet => facet.Replace("&", "%26")).ToArray())));
            }

            client.QueryString = qryValues;

            Uri url = new Uri(geoNetworkUrl + "/q");

            string json = client.DownloadString(url);
            var metaResp = JsonConvert.DeserializeObject<datacontract.metadataResponse>(json);

            qryValues.Clear();
            client.QueryString.Clear();

            return metaResp;
        }

        public datacontract.metadataResponse searchAll(string q,
            string themekey, string orgName, string dataType, string siteId,
            string inspiretheme)
        {
            var metaResp1 = this.search(
                q, 1, 100, themekey, orgName, dataType, siteId, inspiretheme);

            for (int i = 101; i < metaResp1.summary.count; i += 100)
            {
                var metaResp2 = this.search(q, i, i + 99, themekey, orgName, dataType, siteId, inspiretheme);
                metaResp1.metadataRecords.AddRange(metaResp2.metadataRecords);
            }
            metaResp1.to = metaResp1.summary.count;
            return metaResp1;
        }
    }
}
