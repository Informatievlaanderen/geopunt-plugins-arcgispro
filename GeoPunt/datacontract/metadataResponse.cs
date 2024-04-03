using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoPunt.datacontract
{
    public class metadata
    {
        [JsonProperty("source")]
        public string sourceID { get; set; }
        [JsonProperty("link")]
        [JsonConverter(typeof(CustomArrayConverter<string>))]
        public List<string> links { get; set; }
        public string title { get; set; }
        [JsonProperty("abstract")]
        public string description { get; set; }
        [JsonProperty("geonet:info")]
        public geonet geonet { get; set; }
    }

    public class catalogRecord
    {
        [JsonProperty("@id")]
        public string ID { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }


        [JsonProperty("modified")]
        public string Modified { get; set; }


        //[JsonProperty("format")]
        //[JsonConverter(typeof(CustomArrayConverter<string>))]
        //public List<string> Formats { get; set; }


    }

    public class catalogRecordExtra : catalogRecord
    {
        

        [JsonProperty("distribution")]
        [JsonConverter(typeof(CustomArrayConverter<catalogDistribution>))]
        public List<catalogDistribution> Distributions { get; set; }
    }



    public class catalogDistribution : catalogRecord
    {


        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }



        [JsonProperty("accessUrl")]
        public string AccessUrl { get; set; }
        
        

    }



    public class catalogRecordInfo
    {   
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("primaryTopic")]
        public catalogRecordExtra CatalogRecordExtra { get; set; }

    }

    public class geonet
    {
        public int id { get; set; }
        public string uuid { get; set; }
        public DateTime createDate { get; set; }
        public DateTime changeDate { get; set; }
        public string source { get; set; }
    }

    public class summary
    {
        [JsonProperty("@count")]
        public int count { get; set; }

    }

    public class metadataResponse
    {

        [JsonProperty("@from")]
        public int from { get; set; }
        [JsonProperty("@to")]
        public int to { get; set; }

        public summary summary { get; set; }
        [JsonProperty("metadata")]
        public List<metadata> metadataRecords { get; set; }

        #region methods
        public bool geturl(string searchText, string stype, out string wmsUrl, out string wmsLayer, int field_idx = 3)
        {
            wmsUrl = wmsLayer = null;

            if (searchText == null | stype == null | metadataRecords == null | metadataRecords.Count == 0) return false;

            var metaObj = metadataRecords.FirstOrDefault(n => n.title == searchText);

            if (metaObj.links == null || metaObj.links.Count == 0) return false;

            foreach (var link in metaObj.links)
            {
                var linkList = link.Split('|');
                if (linkList.Count() <= 3) return false;

                wmsUrl = linkList[2];
                if (String.IsNullOrEmpty(linkList[0])) wmsLayer = wmsUrl;
                else wmsLayer = linkList[0];

                if (linkList[field_idx].ToUpper().Contains(stype.ToUpper())) return true;
            }
            return false;
        }

        public bool geturl(string searchText, string stype, int field_idx = 3)
        {
            string wmsUrl; string wmsLayer;
            return geturl(searchText, stype, out wmsUrl, out wmsLayer, field_idx);
        }
        #endregion
    }

    public class catalogResponse
    {

        [JsonProperty("totalItems")]
        public int TotalItems { get; set; }
        

        [JsonProperty("member")]
        public List<catalogRecord> catalogRecords { get; set; }

    }

    public class catalogRecordInfoResponse
    {
        [JsonProperty("catalogRecord")]
        public catalogRecordInfo CatalogRecord { get; set; }
    }


    internal class CustomArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
                return token.ToObject<List<T>>();
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
