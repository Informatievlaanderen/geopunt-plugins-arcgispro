using System.Collections.Generic;
using ArcGIS.Core.Geometry;
using GeoPunt.datacontract;

namespace geopunt4Arcgis
{
    // Some methods needs to be migrate for ArcGIS between the original geopunthelper en this file
    static class geopuntHelper
    {
        public static geojsonLine esri2geojsonLine(Polyline esriLine)
        {
            int epsg;
            string epsgUri;

            if (esriLine.SpatialReference == null)
            {
                epsgUri = "";
            }
            else if (esriLine.SpatialReference.Wkid == 900913 || esriLine.SpatialReference.Wkid == 102100)
            {
                epsg = 3857;//google mercator
                epsgUri = string.Format("http://www.opengis.net/def/crs/EPSG/0/{0}", epsg);
            }
            else
            {
                epsg = esriLine.SpatialReference.Wkid;
                epsgUri = string.Format("http://www.opengis.net/def/crs/EPSG/0/{0}", epsg);
            }

            geojsonCRS JScrs = new geojsonCRS()
            {
                type = "link",
                properties = new Dictionary<string, string>() { { "href", epsgUri } }
            };

            geojsonLine JSline = new geojsonLine() { type = "Polyline", crs = JScrs };
            List<List<double>> coords = new List<List<double>>();

            ReadOnlyPointCollection nodes = esriLine.Points;


            for (int n = 0; n < nodes.Count; n++)
            {
                MapPoint node = nodes[n];
                List<double> pt = new List<double>() { node.X, node.Y };
                coords.Add(pt);
            }
            JSline.coordinates = coords;

            return JSline;
        }

    }
}
