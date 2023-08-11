using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.GeoJSON
{
    internal class GeoJSONFeature
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; } = "Feature";
        [JsonProperty(PropertyName = "geometry")]
        public GeoJSONGeometry Geometry { get; set; }
        [JsonProperty(PropertyName = "properties")]
        public Dictionary<string, object> Properties { get; set; }

        public GeoJSONFeature(GeoJSONGeometry geometry, Dictionary<string, object> properties)
        {
            Geometry = geometry;
            Properties = properties;
        }
    }
}
