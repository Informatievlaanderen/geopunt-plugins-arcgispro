using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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


namespace GeoPunt.Dockpanes
{
    internal class StartDockpaneViewModel : DockPane
    {   
      private const string _dockPaneID = "GeoPunt_Dockpanes_StartDockpane";

        private bool _isCheckedProvinces = true;
        public bool IsCheckedProvinces
        {
            get { return _isCheckedProvinces; }
            set
            {
                SetProperty(ref _isCheckedProvinces, value, () => IsCheckedProvinces);
                QueuedTask.Run(() => RefreshLayerProvinces(IsCheckedProvinces));
            }
        }

        private bool _isCheckedCities = true;
        public bool IsCheckedCities
        {
            get { return _isCheckedCities; }
            set
            {
                SetProperty(ref _isCheckedCities, value, () => IsCheckedCities);
                QueuedTask.Run(() => RefreshLayerCities(IsCheckedCities));
            }
        }

        private bool _isCheckedOther = false;
        public bool IsCheckedOther
        {
            get { return _isCheckedOther; }
            set
            {
                SetProperty(ref _isCheckedOther, value, () => _isCheckedOther);
                QueuedTask.Run(() => RefreshLayerOther(IsCheckedOther));
            }
        }


        private void RefreshLayerProvinces(bool val)
        {
            var map = MapView.Active?.Map;
            var layerProvinces = "RefarrG100";
            var searchLayerProvinces = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerProvinces, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            searchLayerProvinces.SetVisibility(val);
        }

        private void RefreshLayerCities(bool val)
        {
            var map = MapView.Active?.Map;
            var layerCities = "RefgemG100";
            var searchLayerCities = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerCities, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            searchLayerCities.SetVisibility(val);
        }

        private void RefreshLayerOther(bool val)
        {
            var map = MapView.Active?.Map;
            var layerOther = "WMS GRB-basiskaart";
            var searchLayerOther = map.GetLayersAsFlattenedList().OfType<WMSLayer>().Where(l => l.Name.Equals(layerOther, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            var list = map.GetLayersAsFlattenedList().ToList();

            if (searchLayerOther == null)
            {
                MessageBox.Show($@"not found");
            }else
            {
                searchLayerOther.SetVisibility(val);
            }
            
        }

        private void RefreshLayerHomeNumber(bool val)
        {
            var map = MapView.Active?.Map;
            var layerProvinces = "GRB - TBLADPADR - huisnummer van een administratief perceel";
            var searchLayerProvinces = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerProvinces, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            searchLayerProvinces.SetVisibility(val);
        }
        protected StartDockpaneViewModel() { }  

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

      /// <summary>
      /// Text shown near the top of the DockPane.
      /// </summary>
		  private string _heading = "Welcome in Geopunt plug-in";
      public string Heading
      {
        get { return _heading; }
        set
        {
          SetProperty(ref _heading, value, () => Heading);
        }
      }
    }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class StartDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			StartDockpaneViewModel.Show();
		}
  }	
}
