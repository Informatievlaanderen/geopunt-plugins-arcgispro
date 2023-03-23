﻿using ArcGIS.Core.CIM;
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
using System.IO;
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
            DifferenceMeters = diff.ToString("0.00")+" meter";
            updateCurrentMapPoint(address, 1,true);
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

            if(isSimple) 
            {
                MapPointSelectedAddressSimple = MapPointBuilderEx.CreateMapPoint(x, y);
                return;
            }

            MapPointSelectedAddress = MapPointBuilderEx.CreateMapPoint(x, y);
        }

        public void updateListBoxFavouritePoint()
        {
            foreach (MapPoint mapPoint in ListStreetsFavouritePoint)
            {
                GeocodeUtils.UpdateMapOverlayMapPoint(mapPoint, MapView.Active, true);
            }
        }

        private void zoomToQuery(MapPoint mapPoint)
        {
            QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                var poly = GeometryEngine.Instance.Buffer(mapPoint, 50);
                mapView.ZoomTo(poly, new TimeSpan(0, 0, 0, 1));
            });
        }
        private ObservableCollection<SaveMapPoint> ListSaveMapPoint = new ObservableCollection<SaveMapPoint>();
        public ICommand CmdSaveIcon
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    List<SaveMapPoint> _data = new List<SaveMapPoint>();
                    foreach (SaveMapPoint item in ListSaveMapPoint)
                    {
                        _data.Add(item);
                    }

                    System.Windows.Forms.SaveFileDialog oSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    oSaveFileDialog.Filter = "Json files (*.json) | *.json";
                    if (oSaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string fileName = oSaveFileDialog.FileName;

                        await using FileStream createStream = File.Create(fileName);
                        await System.Text.Json.JsonSerializer.SerializeAsync(createStream, _data);
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

        public ICommand CmdRemoveFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    MapPoint pointToDelete = ListStreetsFavouritePoint.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                    SaveMapPoint savePointToDelete = ListSaveMapPoint.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
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
                    zoomToQuery(MapPointSelectedAddressSimple);
                });
            }
        }

        public ICommand CmdZoomFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    zoomToQuery(MapPointSelectedAddress);
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
                    }
                    else
                    {
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
                        ListSaveMapPoint.Add(new SaveMapPoint(Address, MapPointSelectedAddress));
                    }
                });
            }
        }

        public PointMapDockpaneViewModel() 
        {
            Module1.vmSearchPlace = this;
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
