using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Dockpanes
{
    internal static class GeocodeUtils
    {

        //private static Dictionary<int, CIMPointGraphicHelper> _lookup = new Dictionary<int, CIMPointGraphicHelper>();
        /// <summary>
        /// Do a search for the contents of the specified URL
        /// </summary>
        /// <param name="text"></param>
        /// <param name="numResults"></param>
        /// <returns></returns>
        //public async static Task<CandidateResponse> SearchFor(string text, int numResults = 2)
        //{
        //    //WebClient wc = new WebClient();
        //    HttpClient hc = new HttpClient();
        //    hc.DefaultRequestHeaders.Add("user-agent", "GeocodeExample");
        //    hc.DefaultRequestHeaders.Add("referer", "GeocodeExample");

        //    //wc.Headers.Add("user-agent", "GeocodeExample");
        //    //wc.Headers.Add("referer", "GeocodeExample");
        //    //wc.Encoding = System.Text.Encoding.UTF8;

        //    CandidateResponse geocodeResult = null;
        //    var uri = new Geocode.GeocodeURI(text, numResults).Uri;


        //    using (var httpResponse = await hc.GetStreamAsync(uri).ConfigureAwait(false))
        //    {
        //        using (StreamReader sr = new StreamReader(httpResponse, System.Text.Encoding.UTF8, true))
        //        {
        //            string response = sr.ReadToEnd();
        //            if (ResponseIsError(response))
        //            {
        //                //throw
        //                throw new System.ApplicationException(response);
        //            }
        //            geocodeResult = ObjectSerialization.JsonToObject<CandidateResponse>(response);
        //        }
        //    }
        //    return geocodeResult;
        //}

        /// <summary>
        /// Check if the returned response is an error
        /// message from the AGS
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        //private static bool ResponseIsError(string response)
        //{
        //    return response.Substring(0, "{\"error\":".Length).CompareTo("{\"error\":") == 0;
        //}

        #region Display
        /// <summary>
        /// Zoom to the specified location
        /// </summary>
        /// <param name="extent">The extent of the geocoded candidate</param>
        /// <returns></returns>
        //public static Task ZoomToLocation(CandidateExtent extent)
        //{
        //    return QueuedTask.Run(() =>
        //    {
        //        ArcGIS.Core.Geometry.SpatialReference spatialReference = SpatialReferenceBuilder.CreateSpatialReference(extent.WKID);
        //        ArcGIS.Core.Geometry.Envelope envelope = EnvelopeBuilderEx.CreateEnvelope(extent.XMin, extent.YMin, extent.XMax, extent.YMax, spatialReference);

        //        //apply extent
        //        MapView.Active.ZoomTo(GeometryEngine.Instance.Expand(envelope, 3, 3, true));
        //    });
        //}

        #endregion Display

        #region Graphics Support

        private static ObservableCollection<System.IDisposable> _overlayObject = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectMarkeer = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectMapPoint = new ObservableCollection<System.IDisposable>();

        /// <summary>
        /// Add a point to the specified mapview
        /// </summary>
        /// <param name="point">The location of the graphic</param>
        /// <param name="mapView">The mapview to whose overlay the graphic will be added</param>
        /// <returns></returns>
        public static async void AddToMapOverlay(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                // Construct point symbol
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                if(isFavourite)
                {
                    symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Star);
                }
            });

            //Get symbol reference from the symbol 
            CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

            await QueuedTask.Run(() =>
            {

               
                if (!isRemove)
                {
                    //MessageBox.Show("drawing");
                    _overlayObject.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }

                //MessageBox.Show("removing");
                RemoveFromMapOverlay(mapView);

            });

        }



        public static async void AddToMapOverlayMarkeer(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                // Construct point symbol
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                //if (isFavourite)
                //{
                //    symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Star);
                //}
            });

            //Get symbol reference from the symbol 
            CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

            await QueuedTask.Run(() =>
            {


                if (!isRemove)
                {
                    //MessageBox.Show("drawing");
                    _overlayObjectMarkeer.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }

                //MessageBox.Show("removing");
                RemoveFromMapOverlayMarkeer(mapView);

            });

        }

        public static async void AddToMapOverlayMapPoint(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                // Construct point symbol
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 10.0, SimpleMarkerStyle.Circle);
                //if (isFavourite)
                //{
                //    symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Star);
                //}
            });

            //Get symbol reference from the symbol 
            CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

            await QueuedTask.Run(() =>
            {


                if (!isRemove)
                {
                    //MessageBox.Show("drawing");
                    _overlayObjectMapPoint.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }

                //MessageBox.Show("removing");
                RemoveFromMapOverlayMapPoint(mapView);

            });

        }

        /// <summary>
        /// All-in-one. Update the graphic on the overlay if it was previously added
        /// otherwise, make it and add it
        /// </summary>
        /// <param name="point">The new location to be added to the map</param>
        /// <param name="mapView"></param>
        /// <returns></returns>
        public static void UpdateMapOverlay(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlay(point, mapView, isFavourite, isRemove);
        }

        public static void UpdateMapOverlayMarkeer(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlay(point, mapView, isFavourite, isRemove);
        }

        public static void UpdateMapOverlayMapPoint(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlayMapPoint(point, mapView, isFavourite, isRemove);
        }

        /// <summary>
        /// Remove the Point Graphic from the specified mapview
        /// </summary>
        /// <param name="mapView"></param>
        public static void RemoveFromMapOverlay(MapView mapView)
        {
            if (_overlayObject != null)
            {
                foreach(var overlay in _overlayObject)
                {
                    overlay.Dispose();
                }
                _overlayObject = new ObservableCollection<System.IDisposable>();
                //MessageBox.Show($@"{_overlayObject.Count}");
                //_overlayObject.Dispose();
                //_overlayObject = null;
            }
        }

        public static void RemoveFromMapOverlayMarkeer(MapView mapView)
        {
            if (_overlayObjectMarkeer != null)
            {
                foreach (var overlay in _overlayObjectMarkeer)
                {
                    overlay.Dispose();
                }
                _overlayObjectMarkeer = new ObservableCollection<System.IDisposable>();
                //MessageBox.Show($@"{_overlayObject.Count}");
                //_overlayObject.Dispose();
                //_overlayObject = null;
            }
        }

        public static void RemoveFromMapOverlayMapPoint(MapView mapView)
        {
            if (_overlayObjectMapPoint != null)
            {
                foreach (var overlay in _overlayObjectMapPoint)
                {
                    overlay.Dispose();
                }
                _overlayObjectMapPoint = new ObservableCollection<System.IDisposable>();
                //MessageBox.Show($@"{_overlayObject.Count}");
                //_overlayObject.Dispose();
                //_overlayObject = null;
            }
        }

        #endregion Graphics Support
    }
}
