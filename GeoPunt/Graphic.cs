using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt
{
    public class Graphic
    {
        public Dictionary<string, object> Attributes { get; set; }
        public Geometry Geometry { get; set; }

        public Graphic(Dictionary<string, object> attributes, Geometry geometry)
        {
            Attributes = attributes;
            Geometry = geometry;
        }
    }
}
