using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Internal.Framework.Controls;
using ArcGIS.Desktop.Internal.Mapping.TOC;
using GeoPunt.Share;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.DataHandler
{
    internal class poi
    {
        public WebClient client;
        NameValueCollection qryValues;
        string baseUrl = "https://poi.api.geopunt.be/v1/core";

        public poi(string proxyUrl, int port, int timeout)
        {
            this.init(proxyUrl, port, timeout);
        }
        public poi(int timeout)
        {
            this.init("", 80, timeout);
        }
        public poi()
        {
            this.init("", 80, 5000);
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

        public datacontract.poiCategories listThemes()
        {
            Uri poiUri = new Uri(baseUrl + "/themes");
            string json = client.DownloadString(poiUri);

            datacontract.poiCategories poiResponse = JsonConvert.DeserializeObject<datacontract.poiCategories>(json);

            client.QueryString.Clear();
            return poiResponse;
        }

        public datacontract.poiCategories listCategories(string themeid)
        {

            if(themeid == "Welzijn, gezondheid en gezin") { themeid = "WelzijnGezondheidEnGezin"; };
            if(themeid == "Cultuur, sport en toerisme") { themeid = "CultuurSportEnToerisme"; };
            if(themeid == "Natuur en milieu") { themeid = "NatuurEnMilieu"; };
            if(themeid == "Bouwen en wonen") { themeid = "BouwenEnWonen"; };

            //MessageBox.Show($@"themeID: {themeid}");

            Uri poiUri;
            if (themeid == null || themeid == "") poiUri = new Uri(baseUrl + "/categories");
            else poiUri = new Uri(baseUrl + string.Format("/themes/{0}/categories", themeid));
            string json = client.DownloadString(poiUri);

            datacontract.poiCategories poiResponse = JsonConvert.DeserializeObject<datacontract.poiCategories>(json);

            client.QueryString.Clear();
            return poiResponse;
        }
        public datacontract.poiCategories listCategories()
        {
            return listCategories("CultuurSportEnToerisme");
        }

        public datacontract.poiCategories listPOItypes(string themeid, string categoryid)
        {

            if (categoryid == "Transport over land") { categoryid = "TransportLand"; };
            if (categoryid == "Zorg en gezondheid") { categoryid = "ZorgEnGezondheid"; };
            if (categoryid == "Kind en gezin") { categoryid = "KindEnGezin"; };
            if (categoryid == "GPBV-installaties industrie") { categoryid = "GPBVInstallatiesIndustrie"; };
            if (categoryid == "GPBV-installaties veeteelt") { categoryid = "GPBVInstallatiesVeeteelt"; };
            if (categoryid == "Lager onderwijs") { categoryid = "LagerOnderwijs"; };
            if (categoryid == "Secundair onderwijs") { categoryid = "SecundairOnderwijs"; };
            if (categoryid == "Hoger onderwijs") { categoryid = "HogerOnderwijs"; };
            if (categoryid == "Deeltijds kunstonderwijs") { categoryid = "DeeltijdsKunstonderwijs"; };


            Uri poiUri;
            if ((themeid != "" && themeid != null) && (categoryid != "" && categoryid != null))
            {
                poiUri = new Uri(baseUrl + string.Format("/themes/{0}/categories/{1}/poitypes", themeid, categoryid));
            }
            else if (categoryid != "" && categoryid != null)
            {
                poiUri = new Uri(baseUrl + string.Format("/categories/{0}/poitypes", categoryid));
            }
            else
            {
                poiUri = new Uri(baseUrl + "/poitypes");
            }

            string json = client.DownloadString(poiUri);

            datacontract.poiCategories poiResponse = JsonConvert.DeserializeObject<datacontract.poiCategories>(json);

            client.QueryString.Clear();
            return poiResponse;
        }
        //public datacontract.poiCategories listPOItypes(string themeid)
        //{
        //    return listPOItypes(themeid, null);
        //}

        public datacontract.poiCategories listPOItypes(string categoryid)
        {
            return listPOItypes(null, categoryid);
        }
        //public datacontract.poiCategories listPOItypes()
        //{
        //    return listPOItypes(null, "Cultuur");
        //}

        public datacontract.poiMinResponse getMinmodel(string q, bool Clustering, string theme, string category,
            string POItype, CRS srs, int? id, string niscode, string bbox)
        {
            setQueryValues(q, 1024, Clustering, false, theme, category, POItype, srs, id, niscode);
            client.QueryString = qryValues;

            string json = client.DownloadString(baseUrl);

            datacontract.poiMinResponse poiResponse = JsonConvert.DeserializeObject<datacontract.poiMinResponse>(json);

            client.QueryString.Clear();

            return poiResponse;
        }

        public datacontract.poiMinResponse getMinmodel(string q, bool Clustering, string theme, string category,
            string POItype, CRS srs, int? id, string niscode)
        {
            return getMinmodel(q, Clustering, theme, category, POItype, srs, id, niscode, null);
        }
        public datacontract.poiMinResponse getMinmodel(string q, bool Clustering, string theme, string category,
            string POItype, CRS srs, int? id)
        {
            return getMinmodel(q, Clustering, theme, category, POItype, srs, id, "", null);
        }

        public datacontract.poiMaxResponse getMaxmodel(string q, int c, bool Clustering, string theme, string category,
            string POItype, CRS srs, int? id, string niscode, string bbox)
        {
            MessageBox.Show($@"cat:: {category} || theme:: {theme}");

            if (theme == "Welzijn, gezondheid en gezin") { theme = "WelzijnGezondheidEnGezin"; };
            if (theme == "Cultuur, sport en toerisme") { theme = "CultuurSportEnToerisme"; };
            if (theme == "Natuur en milieu") { theme = "NatuurEnMilieu"; };
            if (theme == "Bouwen en wonen") { theme = "BouwenEnWonen"; };

            if (category == "Transport over land") { category = "TransportLand"; };
            if (category == "Zorg en gezondheid") { category = "ZorgEnGezondheid"; };
            if (category == "Kind en gezin") { category = "KindEnGezin"; };
            if (category == "GPBV-installaties industrie") { category = "GPBVInstallatiesIndustrie"; };
            if (category == "GPBV-installaties veeteelt") { category = "GPBVInstallatiesVeeteelt"; };
            if (category == "Lager onderwijs") { category = "LagerOnderwijs"; };
            if (category == "Secundair onderwijs") { category = "SecundairOnderwijs"; };
            if (category == "Hoger onderwijs") { category = "HogerOnderwijs"; };
            if (category == "Deeltijds kunstonderwijs") { category = "DeeltijdsKunstonderwijs"; };

            client.QueryString.Clear();
            client.QueryString.Add("theme", theme);
            client.QueryString.Add("maxcount", "1000");
            client.QueryString.Add("clustering", "false");
            client.QueryString.Add("region", niscode);
            client.QueryString.Add("category", category);

            Debug.WriteLine("MY PARAMETERS: ");
            foreach (var item in client.QueryString)
            {
                Debug.WriteLine(item.ToString());
            }
            

            string json = client.DownloadString(baseUrl);

            datacontract.poiMaxResponse poiResponse = JsonConvert.DeserializeObject<datacontract.poiMaxResponse>(json);

            client.QueryString.Clear();

            return poiResponse;
        }
        public datacontract.poiMaxResponse getMaxmodel(string q, int c, bool Clustering, string theme, string category,
            string POItype, CRS srs, int? id, string niscode)
        {
            return getMaxmodel(q, c, Clustering, theme, category, POItype, srs, id, niscode, null);
        }
        public datacontract.poiMaxResponse getMaxmodel(string q, int c, bool Clustering, string theme, string category,
            string POItype, CRS srs, int? id)
        {
            return getMaxmodel(q, c, Clustering, theme, category, POItype, srs, id, "", null);
        }

        private void setQueryValues(string q, int c, bool Clustering, bool maxModel,
             string theme, string category, string POItype, CRS? srs,
             //int? id, string niscode, boundingBox bbox)
             int? id, string niscode)
        {
            qryValues.Clear();

            if (q != null || q != "") qryValues.Add("keyword", q);

            if (Clustering && !maxModel) qryValues.Add("Clustering", "true");
            else qryValues.Add("Clustering", "false");
            //srs
            qryValues.Add("srsIn", Convert.ToString((int)srs));
            qryValues.Add("srsOut", Convert.ToString((int)srs));

            //string lists
            if (id != null) qryValues.Add("id", id.ToString());
            if (niscode != null || niscode != "") qryValues.Add("region", niscode);
            if (theme != null || theme != "") qryValues.Add("theme", theme);
            if (category != null || category != "") qryValues.Add("category", category);
            if (POItype != null || POItype != "") qryValues.Add("POItype", POItype);

            qryValues.Add("maxcount", c.ToString());

            if (maxModel)
            {
                qryValues.Add("maxmodel", "true");
            }
            else
            {
                qryValues.Add("maxmodel", "false");
            }
            //if (bbox != null) qryValues.Add("bbox", bbox.ToBboxString("|", "|"));
        }
    }
}
