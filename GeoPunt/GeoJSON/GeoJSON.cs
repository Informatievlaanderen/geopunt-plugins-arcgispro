using ArcGIS.Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.GeoJSON
{
    internal class GeoJSONClass
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; } = "FeatureCollection";

        [JsonProperty(PropertyName = "features")]
        public List<GeoJSONFeature> Features { get; set; }

        [JsonProperty("crs", NullValueHandling = NullValueHandling.Ignore)]
        public object Crs { get; set; }


        public GeoJSONClass( List<GeoJSONFeature> features)
        {
            Features = features;

        }
        public GeoJSONClass(List<GeoJSONFeature> features,int crsWkid)
        {
            Features = features;
            Crs = new
            {
                type = "name",
                properties = new
                {
                    name = $@"EPSG:{crsWkid}"
                }
            };
        }
    }
}
