using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DataHandler;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
// using System.Windows.Forms;
using System.Windows.Input;
using static System.Net.WebRequestMethods;

namespace GeoPunt.Dockpanes
{
    internal class ElevationProfileViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_ElevationProfile";
        private IDisposable _profileLineDisposable;
        private dhm dhm;


        protected ElevationProfileViewModel()
        {
            Module1.ElevationProfileViewModel = this;

            dhm = new dhm(timeout: 8000);

            PlotControl = new WpfPlot();
            PlotControl.Plot.XLabel("Hoogte (m)");
            PlotControl.Plot.YLabel("Afstand (m)");
            PlotControl.Refresh();
            PlotControl.Plot.Title("Hoogte profiel");

        }

        public ICommand CmdActiveDraw
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    ClearDisposable();

                    ICommand ccmd = FrameworkApplication.GetPlugInWrapper("GeoPunt_DrawTools_Drawline") as ICommand;
                    if ((ccmd != null) && ccmd.CanExecute(null)) // --> CanExecute results to false
                    {
                        ccmd.Execute(null);
                    }
                });
            }
        }

        private Polyline _profileLine;
        public Polyline ProfileLine
        {
            get { return _profileLine; }
            set
            {
                SetProperty(ref _profileLine, value);

                if (_profileLine != null)
                {

                    QueuedTask.Run(() =>
                    {
                        ClearDisposable();
                        DisplayDisposable(_profileLine);
                    });



                    UpdatePlot(_profileLine);



                }
            }
        }

        private WMSLayer _hoogteWMS;
        public WMSLayer HoogteWMS
        {
            get { return _hoogteWMS; }
            set
            {
                SetProperty(ref _hoogteWMS, value);


            }
        }




        private void DisplayDisposable(Polyline profileLine)
        {
            //Set symbolology, create and add element to layout
            // CIMStroke outlineMulti = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.GreenRGB, 2.0, SimpleLineStyle.Solid);
            CIMLineSymbol lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.GreenRGB, 2);
            _profileLineDisposable = MapView.Active.AddOverlay(profileLine, lineSymbol.MakeSymbolReference());
        }

        private void UpdatePlot(Polyline profileLine)
        {
            ReadOnlyPointCollection lineVertices = profileLine.Points;
            double baseNumber = GeometryEngine.Instance.GeodesicLength(profileLine, LinearUnit.Meters) / lineVertices.Count;

            // to make work
            //dhm.getDataAlongLine((List<List<double>>)profileLine.Copy2DCoordinatesToList()));

            double[] dataX = new double[lineVertices.Count];
            // Populate the array with multiples
            for (int i = 0; i < lineVertices.Count; i++)
            {
                dataX[i] = (i) * baseNumber;
            }

            double[] dataY = (from lineVertex in lineVertices select lineVertex.Z).ToArray();



            PlotControl.Plot.Clear();
            // PlotControl.Plot.SetAxisLimits(yMin: 0);
            PlotControl.Plot.AddScatter(dataX, dataY);
            PlotControl.Refresh();
        }

        private WpfPlot _plotControl;
        public WpfPlot PlotControl
        {
            get { return _plotControl; }
            set
            {
                SetProperty(ref _plotControl, value);
            }
        }



        public ICommand CmdAddLayer
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    if (MapView.Active == null)
                    {
                        MessageBox.Show("No map view active.");
                        return;
                    }

                    // Create a connection to the WMS server
                    // todo change with this: https://geo.api.vlaanderen.be/el-dtm/wcs?SERVICE=WCS&REQUEST=GetCoverage&VERSION=2.0.1&COVERAGEID=EL.GridCoverage.DTM&SUBSET=x%282.4612179129281961,2.4612453886097638%29&SUBSET=y%2851.559456774845174,51.559484250526744%29


                    // I was unable to use it so i use this one: https://geo.api.vlaanderen.be/el/wms?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&FORMAT=image/png&TRANSPARENT=true&layers=EL.GridCoverage.DTM&STYLES=default&CRS=EPSG:31370&WIDTH=975&HEIGHT=563&BBOX=15700,126936,265300,271064
                    // Found here: https://www.geopunt.be/?service=https%3A%2F%2Fgeo.api.vlaanderen.be%2Fel%2Fwms%3Flayers%3DEL.GridCoverage.DTM

                    // var serverConnection = new CIMInternetServerConnection { URL = "https://geo.api.vlaanderen.be/DHMV/wms" };
                    // var serverConnection = new CIMInternetServerConnection { URL = "https://geo.api.vlaanderen.be/el-dtm/wcs" };



                    //? SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&FORMAT=image/png&TRANSPARENT=true&layers=EL.GridCoverage.DTM&STYLES=default&CRS=EPSG:31370&WIDTH=1131&HEIGHT=563&BBOX=-4251.083386067883,134599.08338606794,285284.9166139321,278727.08338606794
                    //var test = new CIMWCSServiceConnection
                    //{
                    //    ServerConnection = serverConnection,
                    //    Version = "2.0.1",
                    //    CapabilitiesParameters = new Dictionary<string, object>
                    //    {
                    //         {"REQUEST","GetCoverage"},
                    //        {"VERSION","2.0.1"},
                    //        {"COVERAGEID","EL.GridCoverage.DTM"},

                    //        {"REQUEST","GetCoverage"},
                    //        {"VERSION","2.0.1"},
                    //        {"COVERAGEID","EL.GridCoverage.DTM"},
                    //        {"SUBSET","x(2.4612179129281961,2.4612453886097638)y(51.559456774845174,51.559484250526744)"},

                    //    }
                    //};


                    var serverConnection = new CIMInternetServerConnection { URL = "https://geo.api.vlaanderen.be/el/wms" };
                    var connection = new CIMWMSServiceConnection { ServerConnection = serverConnection, LayerName = "EL.GridCoverage.DTM" };



                    // Add a new layer to the map
                    var layerParams = new LayerCreationParams(connection);


                    await QueuedTask.Run(() =>
                    {

                        if (HoogteWMS != null)
                        {
                            MapView.Active.Map.RemoveLayer(HoogteWMS);
                            HoogteWMS = null;


                        }

                        try
                        {
                            HoogteWMS = LayerFactory.Instance.CreateLayer<WMSLayer>(layerParams, MapView.Active.Map);
                            HoogteWMS.SetTransparency(40);

                            //Debug.WriteLine(HoogteWMS);
                            //Debug.WriteLine(HoogteWMS.DisplayCacheType);
                            //Debug.WriteLine(HoogteWMS.MapLayerType);
                            //Debug.WriteLine(HoogteWMS.SceneLayerType);
                            //Debug.WriteLine(HoogteWMS.GetType());
                            //Debug.WriteLine(HoogteWMS.Layers);
                            //foreach (var layer in HoogteWMS.Layers)
                            //{
                            //    Debug.WriteLine($@"Layer: {layer.Name}, Layer type: {layer.GetType()}");

                            //}

                            //if (HoogteWMS.Layers.Count > 0)
                            //{
                            //    var firstLayer = HoogteWMS.Layers[0];
                            //    Debug.WriteLine(firstLayer.GetType());

                            //    WMSSubLayer wMSSubLayer = firstLayer as WMSSubLayer;
                            //    Debug.WriteLine(wMSSubLayer);
                            //}

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, $@"Error trying to add layer");
                        }
                    });
                });
            }
        }

        //public void IdentifyPixelValue(RasterLayer rasterlayer, string xMap, string yMap)
        //{

        //    bool isLongitudeDouble = double.TryParse(xMap, out double longitude);
        //    bool isLatitudeDouble = double.TryParse(yMap, out double latitude);

        //    if (isLongitudeDouble && isLatitudeDouble && rasterlayer != null && MapView.Active != null)
        //    {
        //        QueuedTask.Run(() =>
        //        {
        //            try
        //            {
        //                Raster raster = rasterlayer.GetRaster();

        //                var pixels = raster.MapToPixel(longitude, latitude);

        //                var objPixelValue = raster.GetPixelValue(0, pixels.Item1, pixels.Item2);

        //                if (objPixelValue != null)
        //                {
        //                    bool isDouble = double.TryParse(objPixelValue.ToString(), out double pixelValue);

        //                    if (isDouble)
        //                    {
        //                        DNGAltitude = pixelValue.ToString();
        //                    }
        //                    else
        //                    {
        //                        DNGAltitude = "0";
        //                    }
        //                }
        //                else
        //                {
        //                    DNGAltitude = "0";
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception(ex.ToString());
        //            }

        //        });
        //    }
        //    else
        //    {
        //        DNGAltitude = "";
        //    }
        //}



        private void ClearDisposable()
        {
            if (_profileLineDisposable != null)
                _profileLineDisposable.Dispose();

        }


        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class ElevationProfile_ShowButton : Button
    {
        protected override void OnClick()
        {
            ElevationProfileViewModel.Show();
        }
    }
}
