using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Geometry.Exceptions;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Core.Utilities;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using GeoPunt.Helpers;
using geopunt4Arcgis;
using Newtonsoft.Json.Linq;
using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
// using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.DataFormats;

namespace GeoPunt.Dockpanes.ElevationProfile
{
    internal class ElevationProfileViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_ElevationProfile";
        private dhm dhm;
        private Helpers.Utils utils = new Helpers.Utils();

        private int LastHighlightedIndex = -1;

        private List<IDisposable> highLightDisposables = new List<IDisposable>();

        private Dictionary<int, CRS> mapCrs = new Dictionary<int, CRS>() {
                { 31370, CRS.Lambert72 },
                { 4326, CRS.WGS84 },
                { 3857, CRS.WEBMERCATOR },
                { 4258, CRS.ETRS89 },
                { 32631, CRS.WGS84UTM31N } };



        protected ElevationProfileViewModel()
        {
            Module1.ElevationProfileViewModel = this;

            dhm = new dhm(timeout: 8000);

            PlotControl = new WpfPlot();
            PlotControl.Plot.XLabel("Afstand (m)");
            PlotControl.Plot.YLabel("Hoogte (m)");
            PlotControl.Plot.Title("Hoogteprofiel");
            PlotControl.MouseMove += PlotControl_MouseMove;
            PlotControl.Refresh();

            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            CheckMapViewIsActive();
        }

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("ActiveMapViewChangedTriggered");

            CheckMapViewIsActive();

        }

        private void CheckMapViewIsActive()
        {
            if (MapView.Active != null)
            {
                MapViewIsActive = true;
            }
            else
            {
                MapViewIsActive = false;
            }
        }

        private bool _mapViewIsActive;
        public bool MapViewIsActive
        {
            get { return _mapViewIsActive; }
            set
            {
                SetProperty(ref _mapViewIsActive, value);
            }
        }

        protected override void OnShow(bool isVisible)
        {
            if (!isVisible)
            {
                ClearDisposables();
                utils.UpdateMarking(new List<Polyline>());

            }
        }

        public void ClearDisposables()
        {
            if (highLightDisposables != null)
            {
                foreach (IDisposable markDisposable in highLightDisposables)
                {
                    markDisposable.Dispose();
                }
                highLightDisposables = new List<IDisposable>();
            }
        }

        public ICommand CmdActiveDraw
        {
            get
            {
                return new RelayCommand(async () =>
                {


                    utils.ClearMarking();
                    ClearDisposables();

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
                        utils.UpdateMarking(new List<Polyline>() { ProfileLine });
                    });


                    GetElevationDataAsync(_profileLine);

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


        private RasterLayer _selectedWCSRaster;
        public RasterLayer SelectedWCSRaster
        {
            get { return _selectedWCSRaster; }
            set
            {
                SetProperty(ref _selectedWCSRaster, value);
            }
        }


        private List<Graphic> _elevationData;
        public List<Graphic> ElevationData
        {
            get { return _elevationData; }
            set
            {
                SetProperty(ref _elevationData, value);
                if (_elevationData != null)
                {
                    
                    UpdatePlot();
                }
            }
        }




        private async Task GetElevationDataAsync(Polyline profileLine)
        {
            try
            {


                if (SelectedWCSRaster == null || !MapView.Active.Map.Layers.Contains(SelectedWCSRaster))
                {
                    await AddWCSRasterAsync();
                }

                if (SelectedWCSRaster == null || !MapView.Active.Map.Layers.Contains(SelectedWCSRaster) )
                {
                    MessageBox.Show("WCS raster niet gevonden.", "Error raster");
                    return;
                }

                //get geometry and length
                var origPolyLine = profileLine;
                var origLength = GeometryEngine.Instance.Length(origPolyLine);
                
                //List of mappoint geometries for the split
                var splitPoints = new List<MapPoint>();

                double profilePointsMinusOne = NumberProfilePoints - 1;
                double enteredValue = origLength / profilePointsMinusOne;
                double splitAtDistance = 0; // to include first point
                double baseNumber = GeometryEngine.Instance.GeodesicLength(profileLine, LinearUnit.Meters) / profilePointsMinusOne;
                double length = 0;

                List<Graphic> graphics = new List<Graphic>();

                await QueuedTask.Run(() =>
                {


                    Raster raster = SelectedWCSRaster.GetRaster();
                    raster.SetSpatialReference(MapView.Active.Map.SpatialReference);


                    while (splitAtDistance <= origLength)
                    {
                        //create a mapPoint at splitDistance and add to splitpoint list
                        MapPoint pt = null;

                        pt = GeometryEngine.Instance.MovePointAlongLine(origPolyLine, splitAtDistance, false, 0, SegmentExtensionType.ExtendTangents);

                        if (pt != null)
                        {
                            splitPoints.Add(pt);



                            var pixels = raster.MapToPixel(pt.X, pt.Y);

                            var objPixelValue = raster.GetPixelValue(0, pixels.Item1, pixels.Item2);

                            if (objPixelValue != null)
                            {
                                bool isDouble = double.TryParse(objPixelValue.ToString(), out double pixelValue);

                                if (isDouble)
                                {
                                    graphics.Add(new Graphic(new Dictionary<string, object>
                                {
                                    {"Meters", length},
                                    {"Height", pixelValue},
                                    }, pt));
                                }
                                else
                                {
                                    graphics.Add(new Graphic(new Dictionary<string, object>
                                {
                                    {"Meters", length},
                                    {"Height", 0},
                                    }, pt));
                                }
                            }
                            else
                            {
                                graphics.Add(new Graphic(new Dictionary<string, object>
                                {
                                    {"Meters", length},
                                    {"Height", 0},
                                    }, pt));
                            }




                        }
                        splitAtDistance += enteredValue;
                        length += baseNumber;
                    }

                    Debug.Write(splitPoints);
                });

                ElevationData = graphics;


            }
            catch (Exception ex)
            {

                MessageBox.Show("Er is een fout opgetreden tijdens de berekening van het hoogteprofiel:" + ex.Message, "Error elevation calculation");
            }

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


        private void UpdatePlot()
        {
            if (ElevationData != null)
            {
                double maxH = ElevationData.Select(c => ((IConvertible)c.Attributes["Height"]).ToDouble(null)).Max();
                double minH = ElevationData.Where(c => ((IConvertible)c.Attributes["Height"]).ToDouble(null) > -999).Select(c => ((IConvertible)c.Attributes["Height"]).ToDouble(null)).Min();
                double maxD = ElevationData.Select(c => ((IConvertible)c.Attributes["Meters"]).ToDouble(null)).Max();

                double[] dataY = (from records in ElevationData select ((IConvertible)records.Attributes["Height"]).ToDouble(null)).ToArray();
                double[] dataX = (from records in ElevationData select ((IConvertible)records.Attributes["Meters"]).ToDouble(null)).ToArray();

                PlotControl.Plot.Clear();
                PlotControl.Plot.SetAxisLimits(yMin: minH, yMax: maxH, xMax: maxD);
                ScatterPlot = PlotControl.Plot.AddScatter(dataX, dataY);
                ScatterPlot.MarkerShape = SelectedMarkerShape;
                AddHighlightPlot();
                PlotControl.Refresh();
            }
        }


        private ObservableCollection<MarkerShape> _listMarkerShape = new ObservableCollection<MarkerShape>(Enum.GetValues(typeof(MarkerShape)).Cast<MarkerShape>().ToList());
        public ObservableCollection<MarkerShape> ListMarkerShape
        {
            get { return _listMarkerShape; }
            set
            {
                SetProperty(ref _listMarkerShape, value);
            }
        }




        private MarkerShape _selectedMarkerShape = MarkerShape.filledCircle;
        public MarkerShape SelectedMarkerShape
        {
            get { return _selectedMarkerShape; }
            set
            {
                SetProperty(ref _selectedMarkerShape, value);
                if (SelectedMarkerShape != null && ScatterPlot != null)
                {
                    ScatterPlot.MarkerShape = SelectedMarkerShape;
                    PlotControl.Refresh();
                }
            }
        }


        // Need to occur if we clear the Plot
        private void AddHighlightPlot()
        {
            HighlightPlot = PlotControl.Plot.AddPoint(0, 0);
            HighlightPlot.Color = System.Drawing.Color.Red;
            HighlightPlot.MarkerSize = 10;
            HighlightPlot.MarkerShape = MarkerShape.openCircle;
            HighlightPlot.IsVisible = false;
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


        private ScatterPlot _scatterPlot;
        public ScatterPlot ScatterPlot
        {
            get { return _scatterPlot; }
            set
            {
                SetProperty(ref _scatterPlot, value);
            }
        }

        private MarkerPlot _highlightPlot;
        public MarkerPlot HighlightPlot
        {
            get { return _highlightPlot; }
            set
            {
                SetProperty(ref _highlightPlot, value);
            }
        }



        private int _numberProfilePoints = 50;
        public int NumberProfilePoints
        {
            get { return _numberProfilePoints; }
            set
            {
                SetProperty(ref _numberProfilePoints, value);

                if (_numberProfilePoints != null && _numberProfilePoints > 0 && _profileLine != null)
                {
                    GetElevationDataAsync(_profileLine);
                }
            }
        }


        private async Task AddWCSRasterAsync()
        {
            if (MapView.Active == null)
            {
                MessageBox.Show("No map view active.");
                return;
            }



            string dtm_url = "https://geo.api.vlaanderen.be/el-dtm/wcs";
            CIMInternetServerConnection serverConnection = new CIMInternetServerConnection
            {
                URL = dtm_url
            };

            CIMWCSServiceConnection serviceConnection = new CIMWCSServiceConnection
            {
                CoverageName = "EL.GridCoverage.DTM",
                Version = "2.0.1",
                ServerConnection = serverConnection,
            };

            RasterLayerCreationParams rasterLyrCreationParams = new RasterLayerCreationParams(serviceConnection);
            // rasterLyrCreationParams.Name = "Test";

            await QueuedTask.Run(() =>
            {


                if (SelectedWCSRaster != null && MapView.Active.Map.Layers.Contains(SelectedWCSRaster))
                {
                    MapView.Active.Map.RemoveLayer(SelectedWCSRaster);
                    SelectedWCSRaster = null;


                }

                try
                {
                    SelectedWCSRaster = LayerFactory.Instance.CreateLayer<RasterLayer>(rasterLyrCreationParams, MapView.Active.Map);
                    SelectedWCSRaster.SetTransparency(40);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, $@"Error trying to add raster layer");
                }
            });
        }





        #region Command


        public ICommand CmdAddWCSRaster
        {
            get
            {
                return new RelayCommand(() =>
                {

                    AddWCSRasterAsync();


                });
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

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, $@"Error trying to add layer");
                        }
                    });
                });
            }
        }


        public ICommand CmdSavePoints
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    if (ElevationData != null)
                    {
                        utils.ExportToGeoJson(ElevationData);
                    }

                });
            }
        }

        public ICommand CmdSaveLine
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (ProfileLine != null)
                    {
                        List<Graphic> graphicList = new List<Graphic> { new Graphic(new Dictionary<string, object>(), ProfileLine) };
                        utils.ExportToGeoJson(graphicList);
                    }

                });
            }
        }

        public ICommand CmdSaveDiagram
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (PlotControl != null)
                    {
                        PlotControl.SaveAsImage();
                    }

                });
            }
        }


        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    pane.Hide();
                });
            }
        }

        #endregion

        #region Events

        private void PlotControl_MouseMove(object sender, MouseEventArgs e)
        {

            if (ScatterPlot != null && HighlightPlot != null)
            {
                // determine point nearest the cursor
                (double mouseCoordX, double mouseCoordY) = PlotControl.GetMouseCoordinates();
                double xyRatio = PlotControl.Plot.XAxis.Dims.PxPerUnit / PlotControl.Plot.YAxis.Dims.PxPerUnit;
                (double pointX, double pointY, int pointIndex) = ScatterPlot.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);

                // place the highlight over the point of interest
                HighlightPlot.X = pointX;
                HighlightPlot.Y = pointY;
                HighlightPlot.IsVisible = true;

                // render if the highlighted point chnaged
                if (LastHighlightedIndex != pointIndex)
                {
                    LastHighlightedIndex = pointIndex;
                    HighLightDisposable(LastHighlightedIndex);
                    PlotControl.Render();
                }
            }
        }

        private void HighLightDisposable(int index)
        {

            QueuedTask.Run(() =>
            {
                ClearDisposables();

                Graphic graphicToHighligh = ElevationData[index];


                ArcGIS.Core.Geometry.Polygon buffer = GeometryEngine.Instance.Buffer(graphicToHighligh.Geometry, 10) as ArcGIS.Core.Geometry.Polygon;

                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 2.0, SimpleLineStyle.Solid);
                CIMPolygonSymbol polygonSym = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.NoColor(), SimpleFillStyle.ForwardDiagonal, outline);

                highLightDisposables.Add(MapView.Active.AddOverlay(buffer, polygonSym.MakeSymbolReference()));

            });

        }

        #endregion


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
