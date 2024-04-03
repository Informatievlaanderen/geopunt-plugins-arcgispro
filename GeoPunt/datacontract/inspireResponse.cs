using System.Collections.Generic;
using Newtonsoft.Json;

namespace GeoPunt.datacontract
{
    public class InspireResponse
    {
        [JsonProperty("register")]
        public InspireRegister Register { get; set; }
    }

    public class InspireRegister
    {
        [JsonProperty("containeditems")]
        public List<InspireItem> Containeditems { get; set; }
    }

    public class InspireItem
    {
        [JsonProperty("theme")]
        public InspireTheme Theme { get; set; }
    }



    public class InspireTheme
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public InspireLabel Label { get; set; }
    }


    public class InspireLabel
    {

        [JsonProperty("text")]
        public string Text { get; set; }
    }










}
