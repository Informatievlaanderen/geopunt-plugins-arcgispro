using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Dockpanes.PointMap
{
    class SaveMapPoint
    {
        public string Adres { get; set; }
        public MapPoint MapPoint { get; set; }

        public SaveMapPoint(string adres, MapPoint mapPoint)
        {
            if (mapPoint == null) return;
            Adres = adres;
            MapPoint = mapPoint;
            //wkid = mapPoint.SpatialReference.Wkid;
        }


        public override string ToString()
        {
            return Adres;
        }
    }
}
