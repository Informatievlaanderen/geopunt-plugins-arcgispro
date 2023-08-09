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

        private static ObservableCollection<System.IDisposable> _overlayObject = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectMarkeer = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectMapPoint = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectCSV = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectTemp = new ObservableCollection<System.IDisposable>();

        public static async void AddToMapOverlay(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                if (isFavourite)
                {
                    symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Star);
                }

                CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                if (!isRemove)
                {
                    _overlayObject.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }
                RemoveFromMapOverlay(mapView);
            });
        }

        public static async void AddToMapOverlayCSV(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreyRGB, 10.0, SimpleMarkerStyle.X);
                CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                if (!isRemove)
                {
                    _overlayObjectCSV.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }
                RemoveFromMapOverlayCSV(mapView);
            });
        }

        public static async void AddToMapOverlayMarkeer(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                if (!isRemove)
                {
                    _overlayObjectMarkeer.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }
                RemoveFromMapOverlayMarkeer(mapView);
            });
        }

        public static async void AddToMapOverlayMapPoint(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            await QueuedTask.Run(() =>
            {
                symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 10.0, SimpleMarkerStyle.Circle);
                CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                if (!isRemove)
                {
                    _overlayObjectMapPoint.Add(mapView.AddOverlay(point, symbolReference));
                    return;
                }
                RemoveFromMapOverlayMapPoint(mapView);
            });
        }

        public static async void AddToMapOverlayMapPerceel(ArcGIS.Core.Geometry.Polygon polygon, CIMPolygonSymbol polygonSym)
        {
            await QueuedTask.Run(() =>
            {
                _overlayObjectPerceel.Add(MapView.Active.AddOverlay(polygon, polygonSym.MakeSymbolReference()));
            });
        }


        public static async void AddToMapOverlayTemp(ArcGIS.Core.Geometry.MapPoint point, MapView mapView)
        {
            await QueuedTask.Run(() =>
            {

                RemoveFromMapOverlayTemp();

                ArcGIS.Core.CIM.CIMPointSymbol symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreyRGB, 10.0, SimpleMarkerStyle.Circle);
                CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                _overlayObjectTemp.Add(mapView.AddOverlay(point, symbolReference));

            });
        }


        public static void UpdateMapOverlay(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlay(point, mapView, isFavourite, isRemove);
        }

        public static void UpdateMapOverlayCSV(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlayCSV(point, mapView, isFavourite, isRemove);
        }

        public static void UpdateMapOverlayMarkeer(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlay(point, mapView, isFavourite, isRemove);
        }

        public static void UpdateMapOverlayMapPoint(ArcGIS.Core.Geometry.MapPoint point, MapView mapView, bool isFavourite = false, bool isRemove = false)
        {
            AddToMapOverlayMapPoint(point, mapView, isFavourite, isRemove);
        }

        public static void UpdateMapOverlayTemp(ArcGIS.Core.Geometry.MapPoint point, MapView mapView)
        {
            AddToMapOverlayTemp(point, mapView);
        }

        public static void RemoveFromMapOverlay(MapView mapView)
        {
            if (_overlayObject != null)
            {
                foreach (var overlay in _overlayObject)
                {
                    overlay.Dispose();
                }
                _overlayObject = new ObservableCollection<System.IDisposable>();
            }
        }

        public static void RemoveFromMapOverlayCSV(MapView mapView)
        {
            if (_overlayObjectCSV != null)
            {
                foreach (var overlay in _overlayObjectCSV)
                {
                    overlay.Dispose();
                }
                _overlayObjectCSV = new ObservableCollection<System.IDisposable>();
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
            }
        }

        public static void RemoveFromMapOverlayPerceel()
        {
            if (_overlayObjectPerceel != null)
            {
                foreach (var overlay in _overlayObjectPerceel)
                {
                    overlay.Dispose();
                }
                _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
            }
        }

        public static void RemoveFromMapOverlayTemp()
        {
            if (_overlayObjectTemp != null)
            {
                foreach (var overlay in _overlayObjectTemp)
                {
                    overlay.Dispose();
                }
                _overlayObjectTemp = new ObservableCollection<System.IDisposable>();
            }
        }

    }
}
