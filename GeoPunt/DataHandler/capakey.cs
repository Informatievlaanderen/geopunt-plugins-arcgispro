using GeoPunt.Share;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.DataHandler
{
    internal class capakey
    {
        public WebClient client;
        NameValueCollection qryValues;
        string baseUri;

        public capakey(string proxyUrl, int port, int timeout)
        {
            this.init(proxyUrl, port, timeout);
        }
        public capakey(int timeout)
        {
            this.init("", 80, timeout);
        }
        public capakey()
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
            baseUri = "https://geo.api.vlaanderen.be/capakey/";
        }

        public datacontract.municipalityList getMunicipalities()
        {

            addExtraDefaultParameter();
            Uri capkeyUri = new Uri(baseUri + "Municipality/");
            string json = client.DownloadString(capkeyUri);
            datacontract.municipalityList response = JsonConvert.DeserializeObject<datacontract.municipalityList>(json);
            client.QueryString.Clear();
            return response;
        }

      

        public datacontract.municipality getMunicipalitiy(int NIScode, DataHandler.CRS srs, DataHandler.capakeyGeometryType geomType)
        {
            addExtraDefaultParameter();
            qryValues.Add("srs", Convert.ToString((int)srs));
            if (geomType == capakeyGeometryType.no) qryValues.Add("geometry", "no");
            else if (geomType == capakeyGeometryType.bbox) qryValues.Add("geometry", "bbox");
            else if (geomType == capakeyGeometryType.full) qryValues.Add("geometry", "full");

            client.QueryString = qryValues;
            Uri gipodUri = new Uri(baseUri + "Municipality/" + NIScode.ToString());
            string json = client.DownloadString(gipodUri);
            datacontract.municipality response = JsonConvert.DeserializeObject<datacontract.municipality>(json);

            client.QueryString.Clear();
            return response;
        }

        public datacontract.departmentList getDepartments(int NIScode)
        {
            addExtraDefaultParameter();
            Uri capkeyUri = new Uri(baseUri + "Municipality/" + NIScode.ToString() + "/department");
            string json = client.DownloadString(capkeyUri);
            datacontract.departmentList response = JsonConvert.DeserializeObject<datacontract.departmentList>(json);
            client.QueryString.Clear();
            return response;
        }
        public datacontract.department getDepartment(int NIScode, int departmentCode, CRS srs, capakeyGeometryType geomType)
        {
            addExtraDefaultParameter();
            qryValues.Add("srs", Convert.ToString((int)srs));
            if (geomType == capakeyGeometryType.no) qryValues.Add("geometry", "no");
            else if (geomType == capakeyGeometryType.bbox) qryValues.Add("geometry", "bbox");
            else if (geomType == capakeyGeometryType.full) qryValues.Add("geometry", "full");

            client.QueryString = qryValues;
            Uri gipodUri = new Uri(baseUri + "Municipality/" + NIScode.ToString() + "/department/" + departmentCode.ToString());
            string json = client.DownloadString(gipodUri);
            datacontract.department response = JsonConvert.DeserializeObject<datacontract.department>(json);

            client.QueryString.Clear();
            return response;
        }

        public datacontract.sectionList getSecties(int NIScode, int departmentCode)
        {
            addExtraDefaultParameter();
            Uri capkeyUri = new Uri(baseUri + "Municipality/" + NIScode.ToString() + "/department/" + departmentCode.ToString() + "/section");
            string json = client.DownloadString(capkeyUri);
            datacontract.sectionList response = JsonConvert.DeserializeObject<datacontract.sectionList>(json);
            client.QueryString.Clear();
            return response;
        }
        public datacontract.section getSectie(int NIScode, int departmentCode, string sectie, CRS srs, capakeyGeometryType geomType)
        {
            addExtraDefaultParameter();
            qryValues.Add("srs", Convert.ToString((int)srs));
            if (geomType == capakeyGeometryType.no) qryValues.Add("geometry", "no");
            else if (geomType == capakeyGeometryType.bbox) qryValues.Add("geometry", "bbox");
            else if (geomType == capakeyGeometryType.full) qryValues.Add("geometry", "full");

            client.QueryString = qryValues;

            Uri capkeyUri = new Uri(baseUri + "Municipality/" + NIScode.ToString() + "/department/" + departmentCode.ToString() + "/section/" + sectie);
            string json = client.DownloadString(capkeyUri);
            datacontract.section response = JsonConvert.DeserializeObject<datacontract.section>(json);

            client.QueryString.Clear();
            return response;
        }

        public datacontract.parcelList getParcels(int NIScode, int departmentCode, string sectie)
        {
            addExtraDefaultParameter();
            Uri capkeyUri = new Uri(baseUri + "Municipality/" + NIScode.ToString() + "/department/" + departmentCode.ToString() + "/section/" + sectie + "/parcel");
            string json = client.DownloadString(capkeyUri);
            datacontract.parcelList response = JsonConvert.DeserializeObject<datacontract.parcelList>(json);
            client.QueryString.Clear();
            return response;
        }
        public datacontract.parcel getParcel(int NIScode, int departmentCode, string sectie, string perceelsNr, CRS srs, capakeyGeometryType geomType)
        {
            addExtraDefaultParameter();
            qryValues.Add("srs", Convert.ToString((int)srs));
            if (geomType == capakeyGeometryType.no) qryValues.Add("geometry", "no");
            else if (geomType == capakeyGeometryType.bbox) qryValues.Add("geometry", "bbox");
            else if (geomType == capakeyGeometryType.full) qryValues.Add("geometry", "full");

            client.QueryString = qryValues;
            Uri gipodUri = new Uri(baseUri + "Municipality/" + NIScode.ToString() + "/department/" + departmentCode.ToString() + "/section/" + sectie + "/parcel/" + perceelsNr);
            string json = client.DownloadString(gipodUri);
            datacontract.parcel response = JsonConvert.DeserializeObject<datacontract.parcel>(json);

            client.QueryString.Clear();
            return response;
        }
        private void addExtraDefaultParameter()
        {
            qryValues.Add("data", "adp");
            qryValues.Add("status", "actual");
            client.QueryString = qryValues;
        }
    }
}
