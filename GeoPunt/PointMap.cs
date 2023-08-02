using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DataHandler;
using GeoPunt.Dockpanes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GeoPunt
{
    internal class PointMap : MapTool
    {
        public string adresString;
        public List<datacontract.locationResult> addresses;

        DataHandler.adresLocation adresLocation;
        public PointMap()
        {
            IsSketchTool = true;
            SketchOutputMode = SketchOutputMode.Map;

            var pane = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_PointMapDockpane");
            pane.Activate();
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            //On mouse down check if the mouse button pressed is the left mouse button. If it is handle the event.
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                e.Handled = true;
        }

        //PointMapDockpaneViewModel vm = new PointMapDockpaneViewModel();
        public double xClick = 0;
        public double yClick = 0;
        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            return QueuedTask.Run(() =>
            {
                var mapPoint = ActiveMapView.ClientToMap(e.ClientPoint);

                Module1.vmSearchPlace.AddTempGraphic(mapPoint);

                var coords = GeometryEngine.Instance.Project(mapPoint, SpatialReferenceBuilder.CreateSpatialReference(31370, 5710)) as MapPoint;
                if (coords == null) return;

                xClick = coords.X;
                yClick = coords.Y;

                adresLocation = new DataHandler.adresLocation(locCallback,5000);
                adresLocation.getXYadresLocationAsync(xClick, yClick, 1);
            });
        }

        private void locCallback(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                double diff = 0;
                double xAdres = 0;
                double yAdres = 0;

                datacontract.crabLocation loc = JsonConvert.DeserializeObject<datacontract.crabLocation>(e.Result);
                addresses = loc.LocationResult;
                foreach(var address in addresses)
                {
                    adresString = address.FormattedAddress;
                    xAdres = address.Location.X_Lambert72;
                    yAdres = address.Location.Y_Lambert72;
                }

                diff = Math.Sqrt(Math.Pow(xAdres - xClick, 2) + Math.Pow(yAdres - yClick, 2));

                Module1.vmSearchPlace.refreshAddress(adresString, diff);
            }
            else
            {
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message);
                }
            }
        }        
        protected override Task OnToolActivateAsync(bool active)
        {
            Module1.vmSearchPlace.Showw();
            return base.OnToolActivateAsync(active);
        }
    }
}
