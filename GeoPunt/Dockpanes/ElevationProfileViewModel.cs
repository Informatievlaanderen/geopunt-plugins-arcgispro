using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ScottPlot;
using System;
using System.Diagnostics;
using System.Linq;
// using System.Windows.Forms;
using System.Windows.Input;
using static System.Net.WebRequestMethods;

namespace GeoPunt.Dockpanes
{
    internal class ElevationProfileViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_ElevationProfile";
        private IDisposable _profileLineDisposable;
        private Layer hoogteWMS;

        protected ElevationProfileViewModel()
        {
            Module1.ElevationProfileViewModel = this;


            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };

            PlotControl = new WpfPlot();
            PlotControl.Plot.AddScatter(dataX, dataY);
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
            double[] dataX = new double[lineVertices.Count];
            // Populate the array with multiples
            for (int i = 0; i < lineVertices.Count; i++)
            {
                dataX[i] = (i) * baseNumber;
            }

            double[] dataY = (from lineVertex in lineVertices select lineVertex.Z).ToArray();


            PlotControl = new WpfPlot();
            PlotControl.Plot.AddScatter(dataX, dataY);
            PlotControl.Refresh();
            PlotControl.Plot.Title("Hoogte profiel");
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
                    // todo change with this // https://geo.api.vlaanderen.be/DHMV/wcs // DHMV2DTM
                    var serverConnection = new CIMInternetServerConnection { URL = "https://geo.api.vlaanderen.be/DHMV/wms" };

                    // var serverConnection = new CIMInternetServerConnection { URL = "https://geo.api.vlaanderen.be/el-dtm/wcs" };

                    // var test = new CIMWCSServiceConnection { ServerConnection = serverConnection, CoverageName = "EL.GridCoverage.DTM", };
                    var connection = new CIMWMSServiceConnection { ServerConnection = serverConnection };

                    // Add a new layer to the map
                    var layerParams = new LayerCreationParams(connection);

                    if (hoogteWMS != null)
                    {
                        MapView.Active.Map.RemoveLayer(hoogteWMS);
                        hoogteWMS = null;
                    }

                    await QueuedTask.Run(() =>
                    {
                        try
                        {
                            hoogteWMS = LayerFactory.Instance.CreateLayer<Layer>(layerParams, MapView.Active.Map);
                            hoogteWMS.SetTransparency(40);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, $@"Error trying to add layer");
                        }
                    });
                });
            }
        }


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
