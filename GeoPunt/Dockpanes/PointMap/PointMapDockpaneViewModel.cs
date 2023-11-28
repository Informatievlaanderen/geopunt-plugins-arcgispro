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
using ArcGIS.Desktop.Mapping.Events;
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.Dockpanes.PointMap
{
    internal class PointMapDockpaneViewModel : DockPane, IMarkedGraphicDisplayer
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_PointMapDockpane";
        private Helpers.Utils utils = new Helpers.Utils();

        private ArcGIS.Core.Geometry.SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);

        DataHandler.adresSuggestion adresSuggestion;
        DataHandler.adresLocation adresLocation;

        public PointMapDockpaneViewModel()
        {
            Module1.PointMapDockpaneViewModel = this;
            TextMarkeer = "Markeer";
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
                GeocodeUtils.RemoveFromMapOverlayTemp();

                if(FrameworkApplication.ActiveTool != null && 
                    FrameworkApplication.ActiveTool.ID == "GeoPunt_PointMap")
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            }
        }

        private string _address = "Klik op de kaart";
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


        

        private Graphic _graphicSelectedAddress;
        public Graphic GraphicSelectedAddress
        {

            get { return _graphicSelectedAddress; }
            set
            {
                SetProperty(ref _graphicSelectedAddress, value);
                if (_graphicSelectedAddress != null)
                {
                    GraphicSelectedAddressIsSelected = true;
                }
                else
                {
                    
                    GraphicSelectedAddressIsSelected = false;
                }
            }
        }

        private bool _graphicSelectedAddressIsSelected = false;
        public bool GraphicSelectedAddressIsSelected
        {
            get { return _graphicSelectedAddressIsSelected; }
            set
            {
                SetProperty(ref _graphicSelectedAddressIsSelected, value);
            }
        }

        public void updateCurrentMapPoint(string query, int count, bool isSimple = false)
        {
            GraphicSelectedAddress = null;
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
                GraphicSelectedAddress = new Graphic(new Dictionary<string, object>
                                {
                                    {"adres", query},
                                }, utils.CreateMapPoint(x, y, lambertSpatialReference));

                return;
            }


           
        }

        //public void updateListBoxFavouritePoint()
        //{
        //    foreach (MapPoint mapPoint in ListStreetsFavouritePoint)
        //    {
        //        GeocodeUtils.UpdateMapOverlayMapPoint(mapPoint, MapView.Active, true);
        //    }
        //}




        private ObservableCollection<Graphic> _markedGraphicsList = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> MarkedGraphicsList
        {
            get { return _markedGraphicsList; }
            set
            {
                SetProperty(ref _markedGraphicsList, value);
            }
        }

        private ObservableCollection<Graphic> _graphicsList = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> GraphicsList
        {
            get { return _graphicsList; }
            set
            {
                SetProperty(ref _graphicsList, value);
            }
        }


        private Graphic _selectedGraphic;
        public Graphic SelectedGraphic
        {

            get { return _selectedGraphic; }
            set
            {
                SetProperty(ref _selectedGraphic, value);
                if (_selectedGraphic != null)
                {
                    if (MarkedGraphicsList.Any(saveGraphic => saveGraphic.Attributes["adres"].ToString() == _selectedGraphic.Attributes["adres"].ToString()))
                    {

                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {
                        TextMarkeer = "Markeer";
                    }

                    SelectedGraphicIsSelected = true;
                }
                else
                {
                    TextMarkeer = "Markeer";
                    SelectedGraphicIsSelected = false;
                }
                // isRemoveMarkeer = false;
                // IsSelectedFavouriteList = false;
            }
        }

        private bool _selectedGraphicIsSelected = false;
        public bool SelectedGraphicIsSelected
        {
            get { return _selectedGraphicIsSelected; }
            set
            {
                SetProperty(ref _selectedGraphicIsSelected, value);
            }
        }


        public void MarkGraphic(Graphic SelectedGraphic)
        {
            if (!MarkedGraphicsList.Any(saveGraphic => saveGraphic.Attributes["adres"] == SelectedGraphic.Attributes["adres"]))
            {
                MarkedGraphicsList.Add(SelectedGraphic);
                updateListBoxMarkeer();
                TextMarkeer = "Verwijder markering";
            }
            else
            {

                Graphic pointToDelete = MarkedGraphicsList.Where(saveGraphic => saveGraphic.Attributes["adres"] == SelectedGraphic.Attributes["adres"]).First();
                MarkedGraphicsList.Remove(pointToDelete);
                updateListBoxMarkeer();
                TextMarkeer = "Markeer";
            }
        }

        public void updateListBoxMarkeer()
        {

            utils.UpdateMarking((from markedGraphic in MarkedGraphicsList select markedGraphic.Geometry).ToList());

        }


        #region Command
        public ICommand CmdExport
        {
            get
            {
                return new RelayCommand(async () =>
                {
                        utils.ExportToGeoJson(GraphicsList.ToList());
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

        public ICommand CmdRemove
        {
            get
            {
                return new RelayCommand(async () =>
                {
                  
                    Graphic graphic = GraphicsList.Where(saveGraphic => saveGraphic.Attributes["adres"] == SelectedGraphic.Attributes["adres"]).FirstOrDefault();
                    Graphic graphicMarked = MarkedGraphicsList.Where(saveGraphicMarked => saveGraphicMarked.Attributes["adres"] == SelectedGraphic.Attributes["adres"]).FirstOrDefault();

                    if (graphic != null)
                    {
                        GraphicsList.Remove(graphic);
                    }

                    if (graphicMarked != null)
                    {
                        MarkedGraphicsList.Remove(graphicMarked);
                           GeocodeUtils.UpdateMapOverlayMarkeer(graphicMarked.Geometry as MapPoint, MapView.Active, true, true);

                    }

                    updateListBoxMarkeer();
                });
            }
        }

        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ZoomTo(GraphicSelectedAddress.Geometry);
                });
            }
        }

        public ICommand CmdZoomFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ZoomTo(SelectedGraphic.Geometry);
                });
            }
        }

        public ICommand CmdMark
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (SelectedGraphic != null)
                    {
                        MarkGraphic(SelectedGraphic);
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
                    if (!GraphicsList.Any(saveGraphic => saveGraphic.Attributes["adres"].ToString() == GraphicSelectedAddress.Attributes["adres"].ToString()))
                    {

                        GraphicsList.Add(GraphicSelectedAddress);
                    }
                });
            }
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

            //FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            pane.Activate();
        }


        

        public void ShowDockPane()
        {
            Show();
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
