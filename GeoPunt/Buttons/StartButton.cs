using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Buttons
{
    internal class StartButton : Button
    {
        protected async override void OnClick()
        {
            var map = MapView.Active?.Map;
            if (map == null)
            {
                MessageBox.Show("No MapView currently active. Please open a MapView to use GeoPunt");
                return;
            }

            var pane = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_StartDockpane");


            if (Caption == "Start")
            {
                await Geoprocessing.ExecuteToolAsync("SelectLayerByAttribute_management", new string[] {
                "RefgewG100", "NEW_SELECTION", "NAAM = 'Vlaams Gewest'" });
                Caption = "Stop";
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_selectByRectangleTool");
                pane.Activate();
            }
            else
            {
                await Geoprocessing.ExecuteToolAsync("SelectLayerByAttribute_management", new string[] {
                "RefgewG100", "NEW_SELECTION", "NAAM = 'Vlaams Gewest'" });
                Caption = "Start";
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                ClosePanes();
                //pane.Hide();
            }

            ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe(t =>
            {
                MapView.Active.ZoomToSelectedAsync(new TimeSpan(0, 0, 1), true);
            });

            ArcGIS.Desktop.Framework.Events.ApplicationClosingEvent.Subscribe(OnApplicationClosing);
          



            await QueuedTask.Run(() =>
            {
                var layerProvinces = "RefarrG100";
                var layerCities = "RefgemG100";

                var searchLayerProvinces = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerProvinces, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var searchLayerCities = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerCities, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (searchLayerProvinces == null || searchLayerCities == null )
                {
                    MessageBox.Show($@"Cannot find these layers: {layerProvinces} or {layerCities}");
                    return;
                }
                else
                {
                    if (Caption != "Start")
                    {
                        //active
                        searchLayerProvinces.SetVisibility(true);
                        searchLayerCities.SetVisibility(true);
                    }
                    else
                    {
                        //desactive
                        searchLayerProvinces.SetVisibility(false);
                        searchLayerCities.SetVisibility(false);
                    }
                }



                MapView.Active.ZoomToSelectedAsync(new TimeSpan(0, 0, 1), true);
                MapView.Active.Map.ClearSelection();

            });


        }

        private void ClosePanes()
        {
            var paneStart = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_StartDockpane");
            var paneAddress = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_SearchAddressDockpane");
            var panePointMap = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_PointMapDockpane");
            paneStart.Hide();
            paneAddress.Hide();
            panePointMap.Hide();
        }
        private Task OnApplicationClosing(System.ComponentModel.CancelEventArgs args)
        {
            ClosePanes();
            return Task.FromResult(0);
        }
    }
}
