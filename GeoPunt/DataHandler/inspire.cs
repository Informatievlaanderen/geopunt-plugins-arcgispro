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
using System.Windows.Documents;

namespace GeoPunt.DataHandler
{
    class inspire
    {
        public WebClient client;
        public string inspireThemeUrl = "https://inspire.ec.europa.eu/theme/theme.nl.json";

        public inspire(string proxyUrl = "", int port = 80, int timeout = 5000)
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
        }

        public Dictionary<string, string> inspireKeywords()
        {

            string url = inspireThemeUrl;
            string json = client.DownloadString(url);
            var inspireResp = JsonConvert.DeserializeObject<datacontract.InspireResponse>(json);
            Dictionary<string,string> inspireKeywords = inspireResp.Register.Containeditems.ToDictionary(x => x.Theme.Id, x => x.Theme.Label.Text);
            return inspireKeywords;
        }
    }
}
