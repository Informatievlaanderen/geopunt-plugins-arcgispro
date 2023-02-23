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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DataHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.Dockpanes
{
    internal class PointMapDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_PointMapDockpane";

        DataHandler.adresSuggestion adresSuggestion;
        DataHandler.adresLocation adresLocation;

        private string _address = "koko";
        public string Address
        {
            get { return _address; }
            set
            {
                SetProperty(ref _address, value);
                //MessageBox.Show($@"vm a ::: {_address}");            
            }
        }

        private string _differenceMeters = "88";
        public string DifferenceMeters
        {
            get { return _differenceMeters; }
            set
            {
                SetProperty(ref _differenceMeters, value);
                //MessageBox.Show($@"vm m ::: {_differenceMeters}");
            }
        }

        public void refreshAddress(string address, double diff)
        {
            Address = address;
            DifferenceMeters = diff.ToString("0.00");
            updateCurrentMapPoint(address, 1);
            MessageBox.Show($@"Adres: {Address},   Difference: {DifferenceMeters}");
        }

        private ObservableCollection<MapPoint> _listStreetsFavouritePoint = new ObservableCollection<MapPoint>();
        public ObservableCollection<MapPoint> ListStreetsFavouritePoint
        {
            get { return _listStreetsFavouritePoint; }
            set
            {
                SetProperty(ref _listStreetsFavouritePoint, value);
            }
        }

        private ObservableCollection<string> _listStreetsFavouriteStringPoint = new ObservableCollection<string>();
        public ObservableCollection<string> ListStreetsFavouriteStringPoint
        {
            get { return _listStreetsFavouriteStringPoint; }
            set
            {
                SetProperty(ref _listStreetsFavouriteStringPoint, value);
            }
        }

        private string _selectedStreetFavouritePoint;
        public string SelectedStreetFavouritePoint
        {
            get { return _selectedStreetFavouritePoint; }
            set
            {
                SetProperty(ref _selectedStreetFavouritePoint, value);
                updateCurrentMapPoint(_selectedStreetFavouritePoint, 1);
            }
        }

        MapPoint MapPointSelectedAddress = null;
        public void updateCurrentMapPoint(string query, int count)
        {
            adresLocation = new DataHandler.adresLocation(5000);
            double x = 0;
            double y = 0;

            List<datacontract.locationResult> loc = adresLocation.getAdresLocation(query, count);
            foreach (datacontract.locationResult item in loc)
            {
                x = item.Location.X_Lambert72;
                y = item.Location.Y_Lambert72;
            }
            MapPointSelectedAddress = MapPointBuilderEx.CreateMapPoint(x, y);
        }

        public void updateListBoxFavouritePoint()
        {
            foreach (MapPoint mapPoint in ListStreetsFavouritePoint)
            {
                GeocodeUtils.UpdateMapOverlay(mapPoint, MapView.Active, true);
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

        public ICommand CmdPoint
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (!ListStreetsFavouritePoint.Contains(MapPointSelectedAddress))
                    {
                        ListStreetsFavouritePoint.Add(MapPointSelectedAddress);
                        MessageBox.Show($@"{MapPointSelectedAddress.Coordinate2D.X} || {MapPointSelectedAddress.Coordinate2D.Y}");
                        updateListBoxFavouritePoint();
                    }
                });
            }
        }

        public ICommand CmdSave
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (!ListStreetsFavouriteStringPoint.Contains(Address))
                    {
                        ListStreetsFavouriteStringPoint.Add(Address);
                    }
                });
            }
        }

        public PointMapDockpaneViewModel() { }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
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
    internal class PointMapDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            PointMapDockpaneViewModel.Show();
        }
    }
}
