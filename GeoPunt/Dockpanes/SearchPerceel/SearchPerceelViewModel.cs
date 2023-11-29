using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Input;



namespace GeoPunt.Dockpanes.SearchPerceel
{
    internal class SearchPerceelViewModel : DockPane, IMarkedGraphicDisplayer
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPerceel";
        private Helpers.Utils utils = new Helpers.Utils();
        private SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);

        List<municipality> municipalities;
        List<department> departments;
        List<parcel> parcels;


        Geometry TempGeometry = null;

        private ObservableCollection<MapPoint> LisPointsFromPolygones = new ObservableCollection<MapPoint>();
        private ObservableCollection<MapPoint> ListPointsFromPolygonesToMarkeer = new ObservableCollection<MapPoint>();
        private ObservableCollection<ObservableCollection<MapPoint>> ListPolygonesToMarkeer = new ObservableCollection<ObservableCollection<MapPoint>>();
        private ObservableCollection<string> ListStringPercel = new ObservableCollection<string>();

        capakey capakey;
        public SearchPerceelViewModel()
        {
            capakey = new capakey(5000);
            municipalities = capakey.getMunicipalities().municipalities;
            ListGemeente = new ObservableCollection<municipality>(municipalities);

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
                MarkedGraphicsList = new ObservableCollection<Graphic>();
                TextMarkeer = "Markeer";
                updateParcelMarkeer();
            }
        }


        private parcel _perceel;
        public parcel Perceel
        {
            get { return _perceel; }
            set
            {
                SetProperty(ref _perceel, value);
                if (_perceel != null)
                {
                    PerceelExist = true;
                }
                else
                {
                    PerceelExist = false;
                }
            }
        }


        private bool _perceelExist = false;
        public bool PerceelExist
        {
            get { return _perceelExist; }
            set
            {
                SetProperty(ref _perceelExist, value);
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
                //updatePercelFromSelectedPerceelToSave(_selectedGraphic);

                if (_selectedGraphic != null)
                {

                    if (MarkedGraphicsList.Any(markedGraphic => markedGraphic.Attributes["Perceel"] == SelectedGraphic.Attributes["Perceel"]))
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



        private bool _nisCodeChecked;
        public bool NISCodeChecked
        {
            get { return _nisCodeChecked; }
            set
            {
                SetProperty(ref _nisCodeChecked, value);
                NISCodeCheckedChanged();
            }
        }

        private void NISCodeCheckedChanged()
        {
            if (NISCodeChecked == true)
            {
                GemeenteDisplayMember = "municipalityCode";
                DepartmentNameDisplayMember = "departmentCode";
            }
            else
            {
                GemeenteDisplayMember = "municipalityName";
                DepartmentNameDisplayMember = "departmentName";
            }
        }


        private ObservableCollection<municipality> _listGemeente;
        public ObservableCollection<municipality> ListGemeente
        {
            get { return _listGemeente; }
            set
            {
                SetProperty(ref _listGemeente, value);
            }
        }

        private string _gemeenteDisplayMember = "municipalityName";
        public string GemeenteDisplayMember
        {
            get { return _gemeenteDisplayMember; }
            set
            {
                SetProperty(ref _gemeenteDisplayMember, value);
            }
        }

        private municipality _selectedListGemeente;
        public municipality SelectedListGemeente
        {
            get { return _selectedListGemeente; }
            set
            {
                SetProperty(ref _selectedListGemeente, value);
                if (_selectedListGemeente != null)
                {
                    gemeenteSelectionChange();
                }
            }
        }

        private ObservableCollection<department> _listDepartments;
        public ObservableCollection<department> ListDepartments
        {
            get { return _listDepartments; }
            set
            {
                SetProperty(ref _listDepartments, value);
            }
        }

        private string _departmentNameDisplayMember = "departmentName";
        public string DepartmentNameDisplayMember
        {
            get { return _departmentNameDisplayMember; }
            set
            {
                SetProperty(ref _departmentNameDisplayMember, value);
            }
        }

        private department _selectedListDepartments;
        public department SelectedListDepartments
        {
            get { return _selectedListDepartments; }
            set
            {
                SetProperty(ref _selectedListDepartments, value);
                if (_selectedListDepartments != null)
                {
                    departmentSelectionChange();
                }
            }
        }




        private ObservableCollection<section> _listSecties;
        public ObservableCollection<section> ListSecties
        {
            get { return _listSecties; }
            set
            {
                SetProperty(ref _listSecties, value);
            }
        }

        private section _selectedListSecties;
        public section SelectedListSecties
        {
            get { return _selectedListSecties; }
            set
            {
                SetProperty(ref _selectedListSecties, value);

                if (_selectedListSecties != null)
                {
                    sectieSelectionChange();
                }
            }
        }



        private ObservableCollection<parcel> _listParcels;
        public ObservableCollection<parcel> ListParcels
        {
            get { return _listParcels; }
            set
            {
                SetProperty(ref _listParcels, value);
            }
        }

        private parcel _selectedListParcels;
        public parcel SelectedListParcels
        {
            get { return _selectedListParcels; }
            set
            {
                SetProperty(ref _selectedListParcels, value);
                if (_selectedListParcels != null)
                {
                    parcelSelectionChange();
                }
            }
        }


        private void gemeenteSelectionChange()
        {
            PerceelExist = false;
            ListDepartments = new ObservableCollection<department>();
            ListSecties = new ObservableCollection<section>();
            ListParcels = new ObservableCollection<parcel>();

            string niscode = SelectedListGemeente.municipalityCode;

            if (niscode == "" || niscode == null) return;

            departments = capakey.getDepartments(int.Parse(niscode)).departments;
            ListDepartments = new ObservableCollection<department>(departments);
        }

        public void departmentSelectionChange()
        {
            PerceelExist = false;
            ListSecties = new ObservableCollection<section>();
            ListParcels = new ObservableCollection<parcel>();

            string niscode = SelectedListGemeente.municipalityCode;
            string depCode = SelectedListDepartments.departmentCode;

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;

            List<section> secties = capakey.getSecties(int.Parse(niscode), int.Parse(depCode)).sections;
            ListSecties = new ObservableCollection<section>(secties);
        }

        public void sectieSelectionChange()
        {
            PerceelExist = false;
            ListParcels = new ObservableCollection<parcel>();

            string niscode = SelectedListGemeente.municipalityCode;
            string depCode = SelectedListDepartments.departmentCode;
            string sectie = SelectedListSecties.sectionCode;

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;
            if (sectie == "" || sectie == null) return;

            parcels = capakey.getParcels(int.Parse(niscode), int.Parse(depCode), sectie).parcels;
            ListParcels = new ObservableCollection<parcel>(parcels);
        }

        public void parcelSelectionChange()
        {
            PerceelExist = false;

            string niscode = SelectedListGemeente.municipalityCode;

            string depCode = SelectedListDepartments.departmentCode;

            string sectie = SelectedListSecties.sectionCode;
            string parcelNr = SelectedListParcels.perceelnummer;

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;
            if (sectie == "" || sectie == null) return;
            if (parcelNr == "" || parcelNr == null) return;


            Perceel = capakey.getParcel(int.Parse(niscode), int.Parse(depCode), sectie, parcelNr,
                                                   CRS.Lambert72, capakeyGeometryType.full);


        }

        private string municipality2nis(string muniName)
        {
            if (muniName == null || muniName == "") return "";

            var niscodes =
                from n in municipalities
                where n.municipalityName == muniName
                select n.municipalityCode;

            if (niscodes.Count() == 0) return "";

            return niscodes.First();
        }

        private string department2code(string depName)
        {
            if (depName == null || depName == "") return "";

            var depcodes =
                from n in departments
                where n.departmentName == depName
                select n.departmentCode;

            if (depcodes.Count() == 0) return "";

            return depcodes.First();
        }



        public void updateListPointFromPolygone()
        {
            foreach (MapPoint mapPoint in LisPointsFromPolygones)
            {
                GeocodeUtils.UpdateMapOverlayMapPoint(mapPoint, MapView.Active, true);
            }
        }

        private CIMPointSymbol CreatePoinSymbol(Color color, double size)
        {
            var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.CreateColor(color), size, SimpleMarkerStyle.Square);
            pointSymbol.UseRealWorldSymbolSizes = true;
            var marker = pointSymbol.SymbolLayers[0] as CIMVectorMarker;
            var polygonSymbol = marker.MarkerGraphics[0].Symbol as CIMPolygonSymbol;
            polygonSymbol.SymbolLayers[0] = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 0, SimpleLineStyle.Null);
            return pointSymbol;
        }

        public static void RemoveFromMapOverlayPerceel()
        {
            if (_overlayObjectPerceel != null)
            {
                foreach (var overlay in _overlayObjectPerceel)
                {
                    overlay.Dispose();
                }
                _overlayObjectPerceel = new ObservableCollection<IDisposable>();
            }
        }

        private static ObservableCollection<IDisposable> _overlayObjectPerceel = new ObservableCollection<IDisposable>();
        private static ObservableCollection<IDisposable> _overlayObjectPerceelToMarkeer = new ObservableCollection<IDisposable>();


        private Polygon CreateParcelPolygon(string capakeyResponse, geojson Geom)
        {


            if (Geom.type == "MultiPolygon")
            {

                geojsonMultiPolygon muniPolygons =
                                  JsonConvert.DeserializeObject<geojsonMultiPolygon>(capakeyResponse);

                foreach (geojsonPolygon poly in muniPolygons.toPolygonList())
                {
                    MessageBox.Show($@"Multipolygones :: {poly}");

                }
                return null;
            }
            else if (Geom.type == "Polygon")
            {
                geojsonPolygon municipalityPolygon =
                            JsonConvert.DeserializeObject<geojsonPolygon>(capakeyResponse);
                MapPoint MapPointFromPolygone = null;
                LisPointsFromPolygones.Clear();


                foreach (var a in municipalityPolygon.coordinates)
                {
                    foreach (var b in a)
                    {

                        MapPointFromPolygone = utils.CreateMapPoint(b[0], b[1], lambertSpatialReference);

                        LisPointsFromPolygones.Add(MapPointFromPolygone);

                    }
                }

                Polygon poly = utils.CreatePolygon(LisPointsFromPolygones, LisPointsFromPolygones[0].SpatialReference);
                return poly;

            }
            else
            {
                return null;
            }
        }


        public ICommand CmdZoomParcel
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    if (Perceel != null)
                    {

                        geojson geojson = JsonConvert.DeserializeObject<geojson>(Perceel.geometry.shape);
                        Polygon polygon = CreateParcelPolygon(Perceel.geometry.shape, geojson);
                        utils.ZoomTo(polygon);

                    }

                });
            }
        }


        public ICommand CmdSaveIcon
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ExportToGeoJson(GraphicsList.ToList());
                });
            }
        }


        public ICommand CmdZoomParcelFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    utils.ZoomTo(SelectedGraphic.Geometry);
                });
            }
        }

        public ICommand CmdSave
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (Perceel != null)
                    {

                        geojson geojson = JsonConvert.DeserializeObject<geojson>(Perceel.geometry.shape);
                        Polygon polygon = CreateParcelPolygon(Perceel.geometry.shape, geojson);


                        Graphic graphic = new Graphic(new Dictionary<string, object>
                                {
                                    {"Gemeente", SelectedListGemeente != null ? SelectedListGemeente.municipalityName: ""},
                                    {"Department", SelectedListDepartments != null ? SelectedListDepartments.departmentName: ""},
                                    {"Sectie", SelectedListSecties !=null?SelectedListSecties.sectionCode:""},
                                    {"Perceel", SelectedListParcels !=null?SelectedListParcels.perceelnummer:""},
                                }, polygon);

                        if (GraphicsList.FirstOrDefault(m => m.Attributes["Perceel"] == graphic.Attributes["Perceel"]) == null)
                        {
                            GraphicsList.Add(graphic);
                        }
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

                    Graphic graphic = GraphicsList.Where(graphic => graphic.Attributes["Perceel"] == SelectedGraphic.Attributes["Perceel"]).FirstOrDefault();
                    Graphic graphicMarked = MarkedGraphicsList.Where(markedGraphic => markedGraphic.Attributes["Perceel"] == SelectedGraphic.Attributes["Perceel"]).FirstOrDefault();


                    if (graphic != null)
                    {
                        GraphicsList.Remove(graphic);
                    }

                    if (graphicMarked != null)
                    {
                        MarkedGraphicsList.Remove(graphicMarked);
                    }

                    updateParcelMarkeer();

                });
            }
        }

        private void updateParcelMarkeer()
        {

            utils.UpdateMarking((from markedGraphic in MarkedGraphicsList select markedGraphic.Geometry).ToList());
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


        public void MarkGraphic(Graphic SelectedGraphic)
        {


            if (!MarkedGraphicsList.Any(markedGraphic => markedGraphic.Attributes["Perceel"] == SelectedGraphic.Attributes["Perceel"]))
            {
                MarkedGraphicsList.Add(SelectedGraphic);
                updateParcelMarkeer();
                TextMarkeer = "Verwijder markering";
            }
            else
            {

                Graphic pointToDelete = MarkedGraphicsList.Where(markedGraphic => markedGraphic.Attributes["Perceel"] == SelectedGraphic.Attributes["Perceel"]).First();
                MarkedGraphicsList.Remove(pointToDelete);
                updateParcelMarkeer();
                TextMarkeer = "Markeer";
            }


        }






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

        private ObservableCollection<Graphic> _markedGraphicsList = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> MarkedGraphicsList
        {
            get { return _markedGraphicsList; }
            set
            {
                SetProperty(ref _markedGraphicsList, value);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SearchPerceel_ShowButton : Button
    {
        protected override void OnClick()
        {
            SearchPerceelViewModel.Show();
        }
    }
}
