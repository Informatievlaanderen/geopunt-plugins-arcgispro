using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Specialized;

using GeoPunt.Share;


namespace GeoPunt.DataHandler
{
    internal class gipod
    {
        public WebClient client;
        NameValueCollection qryValues;
        string GipodBaseUrl = "https://api.gipod.vlaanderen.be/ws/v1";

        public gipod(string proxyUrl, int port, int timeout)
        {
            this.init(proxyUrl, port, timeout);
        }
        public gipod( int timeout)
        {
            this.init("", 80, timeout);
        }
        public gipod()
        {
            this.init("", 80, 5000);
        }

        private void init(string proxyUrl, int port, int timeout)
        {
            if (proxyUrl == null || proxyUrl == "")
            {
                client = new gpWebClient() { Encoding = System.Text.Encoding.UTF8, timeout= timeout };
            }
            else
            {
                client = new gpWebClient() { Encoding = System.Text.Encoding.UTF8, 
                                             Proxy = new System.Net.WebProxy(proxyUrl, port), timeout= timeout };
            }
            client.Headers["Content-type"] = "application/json";
            qryValues = new NameValueCollection();
        }

        public List<string> getReferencedata(gipodReferencedata typeReferenceData)
        {
            string tail;
            switch (typeReferenceData)
            {
                case gipodReferencedata.city:
                    tail = "city"; break;
                case gipodReferencedata.province:
                    tail = "province"; break;
                case gipodReferencedata.owner:
                    tail = "owner"; break;
                case gipodReferencedata.eventtype:
                    tail = "eventtype"; break;
                default: return null;
            }

            client.QueryString = qryValues;
            Uri gipodUri = new Uri( GipodBaseUrl + "/referencedata/" + tail);
            string json = client.DownloadString(gipodUri);
            List<string> gipodResponse = JsonConvert.DeserializeObject<List<string>>(json);
            return gipodResponse;
        }

        //manifestation
        public List<datacontract.gipodResponse> getManifestation( 
                DateTime? startdate, DateTime? enddate,
                string city , string province, string owner, string eventtype,
                CRS CRS, int c, int offset
                //, boundingBox bbox
                 )
        {
            setQueryValues(startdate, enddate, city, province, owner, eventtype, CRS, c, offset
            // , bbox
            );

            client.QueryString = qryValues;
            
            Uri gipodUri = new Uri(GipodBaseUrl + "/manifestation");
            string json = client.DownloadString(gipodUri);

            var gipodRespons = JsonConvert.DeserializeObject<List<datacontract.gipodResponse>>(json);
            
            client.QueryString.Clear();
            return gipodRespons;
        }

        public List<datacontract.gipodResponse> allManifestations( DateTime? startdate, DateTime? enddate,
                string city, string province, string owner, string eventtype, CRS CRS
                //, boundingBox bbox
                )
        {
            List<datacontract.gipodResponse> allWA = new List<datacontract.gipodResponse>();
            List<datacontract.gipodResponse> WA = getManifestation(startdate, enddate, city, province, owner, eventtype, CRS, 500, 0
            // , bbox
            );

            allWA.AddRange(WA);

            int counter = 0;

            while (WA.Count == 500)
            {
                counter += 500;
                WA = getManifestation(startdate, enddate, city, province, owner, eventtype, CRS, 500, counter
                // , bbox
                );
                allWA.AddRange(WA);
            }
            return allWA;
        }

        //workassignments
        public List<datacontract.gipodResponse> getWorkassignment(DateTime? startdate , DateTime? enddate,
                string city, string province, string owner, CRS CRS, int c, int offset
                //, boundingBox bbox
                )
        {
            setQueryValues(startdate, enddate, city, province, owner, "", CRS, 500, offset
            // , bbox
            );

            client.QueryString = qryValues;

            Uri gipodUri = new Uri(GipodBaseUrl + "/workassignment");
            string json = client.DownloadString(gipodUri);

            var gipodRespons = JsonConvert.DeserializeObject<List<datacontract.gipodResponse>>(json);

            client.QueryString.Clear();
            return gipodRespons;
        }

        public List<datacontract.gipodResponse> allWorkassignments(DateTime? startdate, DateTime? enddate,
                string city, string province, string owner, CRS CRS
            //
            //, boundingBox bbox
            
            )
        {
            List<datacontract.gipodResponse> allWA = new List<datacontract.gipodResponse>();
            List<datacontract.gipodResponse> WA = getWorkassignment(startdate, enddate, city, province, owner, CRS, 500, 0
            // , bbox
            );

            allWA.AddRange(WA);

            int counter = 0;

            while (WA.Count == 500)
            {
                counter += 500;
                WA = getWorkassignment(startdate, enddate, city, province, owner, CRS, 500, counter
                // , bbox
                );
                allWA.AddRange(WA);
            }
            return allWA;
        }

        private void setQueryValues(DateTime? startdate, DateTime? enddate, 
            string city, string province, string owner, string eventtype, CRS sRS, int c, int offset
            //
            //, boundingBox bbox
            
            )
        {
            qryValues.Clear();
            //counters
            qryValues.Add("offset", offset.ToString());
            qryValues.Add("limit", c.ToString());
            //string lists
            if (city != "" || city != null) qryValues.Add("city", city);
            if (province != "" || province != null) qryValues.Add("province", province);
            if (owner != "" || owner != null) qryValues.Add("owner", owner);
            if (eventtype != "" || eventtype != null) qryValues.Add("eventtype", eventtype);
            //CRS
            qryValues.Add("CRS", Convert.ToString((int)sRS));

            //Time
            if (startdate != null) qryValues.Add("startdate", startdate.Value.ToString("yyyy-MM-dd"));
            if (enddate != null) qryValues.Add("enddate", enddate.Value.ToString("yyyy-MM-dd"));

            //bbox
            //if (bbox != null) qryValues.Add("bbox", bbox.ToBboxString(",", "|") );
        }

    }
}
