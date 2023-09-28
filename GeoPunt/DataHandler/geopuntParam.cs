using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.DataHandler
{
    public enum capakeyGeometryType { no, bbox, full }
    public enum CRS { Lambert72 = 31370, WGS84 = 4326, WEBMERCATOR = 3857, ETRS89 = 4258, WGS84UTM31N = 32631 }

   

    public class geopuntParam
    {
        public CRS crs;
    }


    public enum gipodtype { workassignment, manifestation }

    public enum gipodReferencedata { city, province, eventtype, owner }

    public struct gipodParam
    {
        public gipodtype gipodType;
        public string bbox;
        public string city;
        public string province;
        public string owner;
        public string eventtype;
        public DateTime startdate;
        public DateTime enddate;
        public CRS crs;
        public string shapePath;
    }
}
