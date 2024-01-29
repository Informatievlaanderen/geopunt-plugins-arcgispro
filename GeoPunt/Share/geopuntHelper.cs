using System;
using System.Collections.Generic;
using System.Net;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
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


        #region "Internet available?"
        /// <summary>check if internet available </summary>
        public static bool IsConnectedToInternet
        {
            get
            {
                try
                {
                    HttpWebRequest hwebRequest = (HttpWebRequest)WebRequest.Create("https://www.google.be/");
                    hwebRequest.Timeout = 5000; //5 seconds timeout to process the request.
                    HttpWebResponse hWebResponse = (HttpWebResponse)hwebRequest.GetResponse(); //Process the request.
                    if (hWebResponse.StatusCode == HttpStatusCode.OK) //Get the response.
                    {
                        return true; //If true, the user is connected to the internet.
                    }
                    else return false; //Else it is not.
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Geen internet connectie", ex.Message);
                    return false;
                }
            }
        }
        /// <summary>Check if a url refers to a existing site </summary>
        /// <param name="url">the url</param>
        /// <param name="isWMS">indicate if the url is wms</param>
        /// <returns>true if exists</returns>
        public static bool websiteExists(string url, bool isWMS)
        {
            HttpWebResponse response = null;

            if (isWMS)
            {
                url = url.Split('?')[0] + "?request=GetCapabilities&service=WMS";
            }

            var hwebRequest = (HttpWebRequest)WebRequest.Create(url);

            hwebRequest.Timeout = 8000;
            //hwebRequest.Method = "HEAD";

            try
            {
                response = (HttpWebResponse)hwebRequest.GetResponse();
                return true;
            }
            catch (WebException)
            {
                return false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }
        /// <summary>Check if a url refers to a existing site </summary>
        /// <param name="url">the url</param>
        /// <returns>true if exists</returns>
        public static bool websiteExists(string url)
        {
            return websiteExists(url, false);
        }
        #endregion




    }
}
