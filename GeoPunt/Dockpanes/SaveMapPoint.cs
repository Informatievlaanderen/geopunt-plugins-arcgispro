using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Dockpanes
{
    class SaveMapPoint
    {
        public string Adres { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        //public int wkid { get; set; }


        public SaveMapPoint(string adres, MapPoint mapPoint)
        {
            if (mapPoint == null) return;
            Adres = adres;
            X = mapPoint.X;
            Y = mapPoint.Y;
            //wkid = mapPoint.SpatialReference.Wkid;
        }

       

        public override string ToString()
        {
            return Adres;
        }
    }
}
