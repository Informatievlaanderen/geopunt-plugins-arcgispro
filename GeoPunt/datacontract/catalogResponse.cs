using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoPunt.datacontract
{
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


        [JsonProperty("endpointUrl")]
        public string? EndpointUrl { get; set; }



        [JsonProperty("subject")]
        [JsonConverter(typeof(CustomArrayConverter<calatogRecordSubject>))]
        public List<calatogRecordSubject> Subject { get; set; }

        
    }


    

    public class calatogRecordSubject
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("inscheme")]
        public string Inscheme { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }




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

}
