using GeoPunt.Share;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ArcGIS.Desktop.Framework.Dialogs;

namespace GeoPunt.DataHandler
{
    internal class adres
    {
    }

    public class adresSuggestion
    {
        public delegate void adresSuggestionDelegate(object sender, DownloadStringCompletedEventArgs e);
        public WebClient client;
        string crabBaseUrl = "https://geo.api.vlaanderen.be/geolocation";

        public adresSuggestion(adresSuggestionDelegate callback, string proxyUrl, int port, int timeout)
        {
            init(callback, proxyUrl, port, timeout);
        }
        public adresSuggestion(adresSuggestionDelegate callback, int timeout)
        {
            init(callback, "", 80, timeout);
        }
        public adresSuggestion(adresSuggestionDelegate callback)
        {
            init(callback, "", 80, 5000);
        }
        public adresSuggestion(string proxyUrl, int port, int timeout)
        {
            init(proxyUrl, port, timeout);
        }
        public adresSuggestion(int timeout)
        {
            init("", 80, timeout);
        }
        public adresSuggestion()
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
        private void init(adresSuggestionDelegate callback, string proxyUrl, int port, int timeout)
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
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
        }


        public void getAdresSuggestionAsync(string q, int c)
        {
            //MessageBox.Show($@"{q} !! {c}");
            string adresUrl = string.Format(CultureInfo.InvariantCulture, crabBaseUrl + "/Suggestion?c={1}&q={0}", q, c);
            //MessageBox.Show($@"{adresUrl} !!");
            client.DownloadStringAsync(new Uri(adresUrl));
        }

        public List<string> getAdresSuggestion(string q, int c)
        {
            string adresUrl = string.Format(CultureInfo.InvariantCulture, crabBaseUrl + "/Suggestion?c={1}&q={0}", q, c);
            string json = client.DownloadString(new Uri(adresUrl));
            datacontract.crabSuggestion crabSug = JsonConvert.DeserializeObject<datacontract.crabSuggestion>(json);
            return crabSug.SuggestionResult;
        }
    }

    public class adresLocation
    {
        public delegate void adresLocationDelegate(object sender, DownloadStringCompletedEventArgs e);
        public WebClient client;
        string crabBaseUrl = "https://geo.api.vlaanderen.be/geolocation";

        public adresLocation(adresLocationDelegate callback, string proxyUrl, int port, int timeout)
        {
            this.init(callback, proxyUrl, port, timeout);
        }
        public adresLocation(string proxyUrl, int port, int timeout)
        {
            this.init(proxyUrl, port, timeout);
        }
        public adresLocation(adresLocationDelegate callback, int timeout)
        {
            this.init(callback, "", 80, timeout);
        }
        public adresLocation(int timeout)
        {
            this.init("", 80, timeout);
        }
        public adresLocation(adresLocationDelegate callback)
        {
            this.init(callback, "", 80, 5000);
        }
        public adresLocation()
        {
            this.init("", 80, 5000);
        }

        private void init(string proxyUrl, int port, int timeout)
        {
            if (proxyUrl == null | proxyUrl == "")
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
        private void init(adresLocationDelegate callback, string proxyUrl, int port, int timeout)
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
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
        }

        public void getAdresLocationAsync(string q, int c)
        {
            string adresUrl = string.Format(CultureInfo.InvariantCulture, crabBaseUrl + "/Location?c={1}&q={0}", q, c);
            client.DownloadStringAsync(new Uri(adresUrl));
        }

        public List<datacontract.locationResult> getAdresLocation(string q, int c)
        {
            string adresUrl = string.Format(CultureInfo.InvariantCulture, crabBaseUrl + "/Location?c={1}&q={0}", q, c);
            string json = client.DownloadString(new Uri(adresUrl));
            datacontract.crabLocation crabLoc = JsonConvert.DeserializeObject<datacontract.crabLocation>(json);
            return crabLoc.LocationResult;
        }

        public void getXYadresLocationAsync(Double xLam72, Double yLam72, int c)
        {
            string adresUrl = string.Format(CultureInfo.InvariantCulture, crabBaseUrl + "/Location?xy={0},{1}&c={2}", xLam72, yLam72, c);
            client.DownloadStringAsync(new Uri(adresUrl));
        }

        public List<datacontract.locationResult> getXYadresLocation(Double xLam72, Double yLam72, int c)
        {
            string adresUrl = string.Format(CultureInfo.InvariantCulture, crabBaseUrl + "/Location?xy={0},{1}&c={2}", xLam72, yLam72, c);
            string json = client.DownloadString(new Uri(adresUrl));
            datacontract.crabLocation crabLoc = JsonConvert.DeserializeObject<datacontract.crabLocation>(json);
            return crabLoc.LocationResult;
        }

    }
}
