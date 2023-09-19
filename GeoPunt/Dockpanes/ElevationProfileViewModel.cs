using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DrawTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

                if(_profileLine != null)
                {

                    QueuedTask.Run(() =>
                    {
                        ClearDisposable();

                        //Set symbolology, create and add element to layout
                        // CIMStroke outlineMulti = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.GreenRGB, 2.0, SimpleLineStyle.Solid);
                        CIMLineSymbol lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.GreenRGB, 2);
                        _profileLineDisposable = MapView.Active.AddOverlay(_profileLine, lineSymbol.MakeSymbolReference());
                    });


                }
            }
        }

        public ICommand CmdAddLayer
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    if(MapView.Active == null)
                    {
                        MessageBox.Show("No map view active.");
                        return;
                    }

                    // Create a connection to the WMS server
                    var serverConnection = new CIMInternetServerConnection { URL = "https://geo.api.vlaanderen.be/DHMV/wms" };
                    var connection = new CIMWMSServiceConnection { ServerConnection = serverConnection };

                    // Add a new layer to the map
                    var layerParams = new LayerCreationParams(connection);

                    if(hoogteWMS != null)
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


        private void ClearDisposable() {
            if(_profileLineDisposable != null)
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
