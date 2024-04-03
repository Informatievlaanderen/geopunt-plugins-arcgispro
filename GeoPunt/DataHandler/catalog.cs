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
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Diagnostics;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;
using Xceed.Wpf.AvalonDock.Themes;

namespace GeoPunt.DataHandler
{
    class catalog
    {
        public WebClient client;
        NameValueCollection qryValues;

        public string geoNetworkUrl = "https://datavindplaats.api.vlaanderen.be/v1/catalogrecords";

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
            client.Headers["x-api-key"] = Config.config.catalogusKey;
            qryValues = new NameValueCollection();
        }

        public List<string> getKeyWords(string q = "", int limit = 100)
        {
            qryValues.Clear();
            qryValues.Add("q", q);
            qryValues.Add("limit", limit.ToString());
            client.QueryString = qryValues;

            string url = geoNetworkUrl + "/suggestions";
        
            string jsonString = client.DownloadString(new Uri(url));
            JObject jsonObject = JObject.Parse(jsonString);
            JArray keywords = jsonObject.GetValue("itemListElement") as JArray;
            
            qryValues.Clear();
            client.QueryString.Clear();
            return keywords.Select(c => (string)c).ToList();
        }

        public List<string> getOrganisations()
        {
            string url = geoNetworkUrl + @"?limit=1";
            string jsonString = client.DownloadString(new Uri(url));
            JObject jsonObject = JObject.Parse(jsonString) as JObject;
            JArray jsonArr = jsonObject.GetValue("facet") as JArray;
            JObject resultObject = jsonArr.Children<JObject>()
                .FirstOrDefault(o => o["value"] != null && o["value"].Value<string>() == "orgName");

            if (resultObject == null)
            {
                throw new Exception("Organisations not found. Check request.");
            }

            JArray GDIthemes = resultObject.GetValue("children") as JArray;
            return GDIthemes.Select(c => (string)c["value"]).ToList();

        }

        public List<string> getGDIthemes()
        {
            string url = geoNetworkUrl + @"?limit=1";
            string jsonString = client.DownloadString(new Uri(url));
            JObject jsonObject = JObject.Parse(jsonString) as JObject;
            JArray jsonArr = jsonObject.GetValue("facet") as JArray;
            JObject resultObject = jsonArr.Children<JObject>()
                .FirstOrDefault(o => o["value"] != null && o["value"].Value<string>() == "flanderskeyword");

            if (resultObject == null)
            {
                throw new Exception("GDI themes not found. Check request.");
            }

            JArray GDIthemes = resultObject.GetValue("children") as JArray;
            return GDIthemes.Select(c => (string)c["value"]).ToList();
        }

        public List<string> inspireKeywords()
        {
            string url = geoNetworkUrl + "?offset=0&limit=20&taxonomy=type%2Fservice&taxonomy=inspireTheme%2Fhttp%3A%2F%2Finspire.ec.europa.eu%2Ftheme%2Fer";
            string json = client.DownloadString(url);
            var cataResp = JsonConvert.DeserializeObject<datacontract.catalogResponse>(json);

            return cataResp.catalogRecords.Select(c => c.Title).ToList();
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

        public datacontract.catalogRecordInfo searchCatalogRecordInfo(string id)
        {
            Uri uri = new Uri(geoNetworkUrl);
            string url = uri.GetLeftPart(UriPartial.Authority) + id;

            string json = client.DownloadString(url);
            var cataResp = JsonConvert.DeserializeObject<datacontract.catalogRecordInfoResponse>(json);

            qryValues.Clear();
            client.QueryString.Clear();

            return cataResp.CatalogRecord;
        }


        public datacontract.catalogResponse search(string searchfield = "", int offset = 0, int limit = 21,
           string themekey = "", string orgName = "", string dataType = "", string siteId = "", string inspiretheme = "")
        {
            qryValues.Add("sort", "title:asc");
            if (searchfield == "" || searchfield == null) 
            { 
                qryValues.Add("searchfield", "any"); 
            } 
            else
            {
                qryValues.Add("searchfield", searchfield);
            };
            qryValues.Add("offset", offset.ToString());
            qryValues.Add("limit", limit.ToString());

            var taxonomies = new List<string>();
            if (themekey != "" && themekey != null) taxonomies.Add("flanderskeyword/" + themekey);
            if (orgName != "" && orgName != null) taxonomies.Add("orgName/" + orgName);
            if (dataType != "" && dataType != null) taxonomies.Add("type/" + dataType);
            if (siteId != "" && siteId != null) taxonomies.Add("sourceCatalog/" + siteId);
            if (inspiretheme != "" && inspiretheme != null) taxonomies.Add("inspireTheme/" + inspiretheme);

            if (taxonomies.Count() > 0)
            {
                qryValues.Add("taxonomy", String.Join("&taxonomy=", taxonomies.Select(facet => HttpUtility.UrlEncode(facet.Replace("&", "%26"))).ToArray()));

                //foreach (string taxonomy in taxonomies)
                //{
                //    qryValues.Add("taxonomy", HttpUtility.UrlEncode(taxonomy.Replace("&", "%26")));
                //}

                //qryValues.Add("taxonomy", HttpUtility.UrlEncode(String.Join("&taxonomy=", taxonomies.Select(facet => facet.Replace("&", "%26")).ToArray())));
                //qryValues.Add("taxonomy", HttpUtility.UrlEncode(String.Join("&taxonomy=", taxonomies.Select(facet => facet.Replace("&", "%26")).ToArray())));
            }

            client.QueryString = qryValues;

            Uri url = new Uri(geoNetworkUrl);

            string json = client.DownloadString(url);
            var cataResp = JsonConvert.DeserializeObject<datacontract.catalogResponse>(json);

            qryValues.Clear();
            client.QueryString.Clear();

            return cataResp;
        }

        public datacontract.catalogResponse searchAll(string q,
            string themekey, string orgName, string dataType, string siteId,
            string inspiretheme)
        {
            var cataResp1 = this.search(
                q, 0, 100, themekey, orgName, dataType, siteId, inspiretheme);

            for (int i = 100; i < cataResp1.TotalItems; i += 100)
            {
                var cataResp2 = this.search(q, i, 100, themekey, orgName, dataType, siteId, inspiretheme);
                cataResp1.catalogRecords.AddRange(cataResp2.catalogRecords);
            }
            
            return cataResp1;
        }
    }
}
