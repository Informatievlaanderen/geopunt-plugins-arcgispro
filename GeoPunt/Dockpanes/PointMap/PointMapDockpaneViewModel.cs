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
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.Dockpanes.PointMap
{
    internal class PointMapDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_PointMapDockpane";
        private Helpers.Utils utils = new Helpers.Utils();

        private ArcGIS.Core.Geometry.SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);

        DataHandler.adresSuggestion adresSuggestion;
        DataHandler.adresLocation adresLocation;

        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                SetProperty(ref _address, value);
                IsSelectedFavouriteList = false;
            }
        }

        private string _textMarkeer;
        public string TextMarkeer
        {
            get { return _textMarkeer; }
            set
            {
                SetProperty(ref _textMarkeer, value);
            }
        }

        private bool _isInverseSelectedFavouriteList;
        public bool IsInverseSelectedFavouriteList
        {
            get { return _isInverseSelectedFavouriteList; }
            set
            {
                SetProperty(ref _isInverseSelectedFavouriteList, value);
            }
        }


        private bool _isSelectedFavouriteList;
        public bool IsSelectedFavouriteList
        {
            get { return _isSelectedFavouriteList; }
            set
            {
                SetProperty(ref _isSelectedFavouriteList, value);
                IsInverseSelectedFavouriteList = !_isSelectedFavouriteList;
            }
        }

        private string _differenceMeters;
        public string DifferenceMeters
        {
            get { return _differenceMeters; }
            set
            {
                SetProperty(ref _differenceMeters, value);
            }
        }

        public void refreshAddress(string address, double diff)
        {
            Address = address;
            DifferenceMeters = diff.ToString("0.00") + " meter";
            updateCurrentMapPoint(address, 1, true);
        }



        public void AddTempGraphic(MapPoint mapPoint)
        {
            GeocodeUtils.UpdateMapOverlayTemp(mapPoint, MapView.Active);
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
                IsSelectedFavouriteList = true;
            }
        }

        MapPoint MapPointSelectedAddress = null;
        MapPoint MapPointSelectedAddressSimple = null;
        public void updateCurrentMapPoint(string query, int count, bool isSimple = false)
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

            if (isSimple)
            {
                MapPointSelectedAddressSimple = utils.CreateMapPoint(x, y, lambertSpatialReference);
                return;
            }

            MapPointSelectedAddress = utils.CreateMapPoint(x, y, lambertSpatialReference);

            if (ListStreetsFavouritePoint.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y) != null)
            {
                TextMarkeer = "Verwijder markering";
            }
            else
            {
                TextMarkeer = "Markeer";
            }
        }

        public void updateListBoxFavouritePoint()
        {
            foreach (MapPoint mapPoint in ListStreetsFavouritePoint)
            {
                GeocodeUtils.UpdateMapOverlayMapPoint(mapPoint, MapView.Active, true);
            }
        }

        private ObservableCollection<Graphic> ListSaveMapPoint = new ObservableCollection<Graphic>();
        public ICommand CmdSaveIcon
        {
            get
            {
                return new RelayCommand(async () =>
                {
                        utils.ExportToGeoJson(ListSaveMapPoint.ToList());
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

        public ICommand CmdRemoveFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    MapPoint pointToDelete = ListStreetsFavouritePoint.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                    Graphic savePointToDelete = ListSaveMapPoint.FirstOrDefault(m => (m.Geometry as MapPoint).X == MapPointSelectedAddress.X && (m.Geometry as MapPoint).Y == MapPointSelectedAddress.Y);
                    ListStreetsFavouriteStringPoint.Remove(SelectedStreetFavouritePoint);
                    ListSaveMapPoint.Remove(savePointToDelete);
                    ListStreetsFavouritePoint.Remove(pointToDelete);
                    GeocodeUtils.UpdateMapOverlayMapPoint(pointToDelete, MapView.Active, true, true);

                    updateListBoxFavouritePoint();
                });
            }
        }

        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ZoomTo(MapPointSelectedAddressSimple);
                });
            }
        }

        public ICommand CmdZoomFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ZoomTo(MapPointSelectedAddress);
                });
            }
        }

        public ICommand CmdPoint
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (ListStreetsFavouritePoint.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y) == null)
                    {
                        ListStreetsFavouritePoint.Add(MapPointSelectedAddress);
                        updateListBoxFavouritePoint();
                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {
                        TextMarkeer = "Markeer";
                        MapPoint pointToDelete = ListStreetsFavouritePoint.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                        ListStreetsFavouritePoint.Remove(pointToDelete);
                        GeocodeUtils.UpdateMapOverlayMapPoint(pointToDelete, MapView.Active, true, true);
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
                        ListSaveMapPoint.Add(new Graphic(new Dictionary<string, object>
                                {
                                    {"adres", Address},
                                }, MapPointSelectedAddressSimple));
                    }
                });
            }
        }

        public PointMapDockpaneViewModel()
        {
            Module1.vmSearchPlace = this;
            TextMarkeer = "Markeer";
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            //FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            pane.Activate();
        }

        public void Showw()
        {
            Show();
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
