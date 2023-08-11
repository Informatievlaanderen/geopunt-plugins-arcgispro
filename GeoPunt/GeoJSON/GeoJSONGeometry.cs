using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.GeoJSON
{
    internal abstract class GeoJSONGeometry
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; protected set; }
        [JsonProperty(PropertyName = "coordinates")]
        public object Coordinates { get; protected set; }

        public class GeoJSONPointGeometry : GeoJSONGeometry
        {
            public GeoJSONPointGeometry(double longitude, double latitude)
            {
                Type = "Point";
                Coordinates = new List<double> { longitude, latitude };
            }

        }

        public class GeoJSONLineStringGeometry : GeoJSONGeometry
        {
            public GeoJSONLineStringGeometry(List<List<double>> coordinates)
            {
                Type = "LineString";
                Coordinates = coordinates;
            }

        }

        public class GeoJSONPolygonGeometry : GeoJSONGeometry
        {
            public GeoJSONPolygonGeometry(List<List<List<double>>> coordinates)
            {
                Type = "Polygon";
                Coordinates = coordinates;
            }

        }


    }
}
