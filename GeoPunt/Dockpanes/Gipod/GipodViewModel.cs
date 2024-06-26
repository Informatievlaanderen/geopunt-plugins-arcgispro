﻿using ActiproSoftware.Windows.Extensions;
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
using ArcGIS.Desktop.Mapping.Events;
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using GeoPunt.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.Dockpanes.Gipod
{
    internal class GipodViewModel : DockPane, IMarkedGraphicDisplayer
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_Gipod";

        private Utils utils = new Utils();
        private capakey capakey = new capakey(8000);
        private gipod gipod = new gipod(8000);
        private municipalityList municipality;
        private SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);
        private ObservableCollection<IDisposable> overlays = new ObservableCollection<IDisposable>();

        protected GipodViewModel()
        {
            initGUI();
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            TextMarkeer = "Markeer";
            TextMarkeerAlles = "Markeer alles";
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
                TextMarkeerAlles = "Markeer alles";
                updateListMarkeer();
            }
        }


        private void initGUI()
        {

            municipality = capakey.getMunicipalities();

            List<string> provincies = new List<string>() { "", "Antwerpen", "Limburg", "Vlaams-Brabant", "Oost-Vlaanderen", "West-Vlaanderen" };
            List<string> municipalities = (from municipality t in municipality.municipalities select t.municipalityName).ToList();
            municipalities.Insert(0, "");
            List<string> owners = gipod.getReferencedata(gipodReferencedata.owner);
            owners.Insert(0, "");
            List<string> eventTypes = gipod.getReferencedata(gipodReferencedata.eventtype);
            eventTypes.Insert(0, "");

            ListProvincie = new ObservableCollection<string>(provincies);
            ListGemeente = new ObservableCollection<string>(municipalities);
            ListEigenaar = new ObservableCollection<string>(owners);
            ListManifestatie = new ObservableCollection<string>(eventTypes);

        }

        public string ComparasionString(Graphic graphic)
        {
            return $"{graphic.Attributes["gipodId"]}";
        }


        public ICommand ChangeTypeManifestation
        {
            get
            {
                return new RelayCommand((param) =>
                {
                    GipodType = (string)param;
                }, () => true);
            }
        }


        private string _gipodType;
        public string GipodType
        {
            get { return _gipodType; }
            set
            {
                SetProperty(ref _gipodType, value);
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

        private string _textMarkeerAlles;
        public string TextMarkeerAlles
        {
            get { return _textMarkeerAlles; }
            set
            {
                SetProperty(ref _textMarkeerAlles, value);
            }
        }



        private bool _manifestionChecked = false;
        public bool ManifestionChecked
        {
            get { return _manifestionChecked; }
            set
            {
                SetProperty(ref _manifestionChecked, value);
            }
        }

        private ObservableCollection<string> _listProvincie = new ObservableCollection<string>();
        public ObservableCollection<string> ListProvincie
        {
            get { return _listProvincie; }
            set
            {
                SetProperty(ref _listProvincie, value);
            }
        }


        private string _selectedProvincie;
        public string SelectedProvincie
        {
            get { return _selectedProvincie; }
            set
            {
                SetProperty(ref _selectedProvincie, value);
                ProvincieSelectionChange();
            }
        }

        private void ProvincieSelectionChange()
        {
            List<string> cities;


            Dictionary<string, string> mapProvincie = new Dictionary<string, string>() {
                { "Antwerpen", "1" },
                { "Limburg", "7" },
                { "Vlaams-Brabant", "2" },
                { "Oost-Vlaanderen", "4" },
                { "West-Vlaanderen", "3" } };


            if (!mapProvincie.ContainsKey(SelectedProvincie))
            {
                cities = (from municipality t in municipality.municipalities
                          select t.municipalityName).ToList();
            }
            else
            {
                cities = (from municipality t in municipality.municipalities
                          where t.municipalityCode.StartsWith(mapProvincie[SelectedProvincie])
                          select t.municipalityName).ToList();
            }

            // cities.Insert(0, "");
            ListGemeente = new ObservableCollection<string>(cities);
        }

        private ObservableCollection<string> _listGemeente = new ObservableCollection<string>();
        public ObservableCollection<string> ListGemeente
        {
            get { return _listGemeente; }
            set
            {
                SetProperty(ref _listGemeente, value);
            }
        }



        private string _selectedGemeente;
        public string SelectedGemeente
        {
            get { return _selectedGemeente; }
            set
            {
                SetProperty(ref _selectedGemeente, value);
            }
        }


        private ObservableCollection<string> _listEigenaar = new ObservableCollection<string>();
        public ObservableCollection<string> ListEigenaar
        {
            get { return _listEigenaar; }
            set
            {
                SetProperty(ref _listEigenaar, value);
            }
        }



        private string _selectedEigenaar;
        public string SelectedEigenaar
        {
            get { return _selectedEigenaar; }
            set
            {
                SetProperty(ref _selectedEigenaar, value);
            }
        }


        private ObservableCollection<string> _listManifestatie = new ObservableCollection<string>();
        public ObservableCollection<string> ListManifestatie
        {
            get { return _listManifestatie; }
            set
            {
                SetProperty(ref _listManifestatie, value);
            }
        }



        private string _selectedManifestatie;
        public string SelectedManifestatie
        {
            get { return _selectedManifestatie; }
            set
            {
                SetProperty(ref _selectedManifestatie, value);
            }
        }

        public ICommand CmdChangeDateToday
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdChangeDateToday");
                    SelectedStartDate = DateTime.Today;
                    SelectedEndDate = DateTime.Today.AddSeconds(86399); // 24h = 36400
                });
            }
        }

        public ICommand CmdChangeDateTomorrow
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdChangeDateTomorrow");
                    SelectedStartDate = DateTime.Today.AddDays(1);
                    SelectedEndDate = DateTime.Today.AddDays(1).AddSeconds(86399); // 24h = 36400
                });
            }
        }

        public ICommand CmdChangeDate30Days
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdChangeDate30Days");
                    SelectedStartDate = DateTime.Today;
                    SelectedEndDate = DateTime.Today.AddDays(30);
                });
            }
        }




        private DateTime _selectedStartDate = DateTime.Today;
        public DateTime SelectedStartDate
        {
            get { return _selectedStartDate; }
            set
            {
                SetProperty(ref _selectedStartDate, value);
            }
        }

        private DateTime _selectedEndDate = DateTime.Today.AddMonths(1);
        public DateTime SelectedEndDate
        {
            get { return _selectedEndDate; }
            set
            {
                SetProperty(ref _selectedEndDate, value);
            }
        }


        private bool _isBeperk;
        public bool IsBeperk
        {
            get { return _isBeperk; }
            set
            {
                SetProperty(ref _isBeperk, value);
            }
        }

        public ICommand CmdSearch
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    GraphicsList = new ObservableCollection<Graphic>();

                    await QueuedTask.Run(() =>
                    {
                        try
                        {


                            gipodParam param = getGipodParam();

                            List<gipodResponse> gipodRecords = fetchGipod(param);


                            updateDataGrid(gipodRecords);

                        }
                        catch (Exception ex)
                        {
                            // MessageBox.Show(ex.Message + " : " + ex.StackTrace);
                            MessageBox.Show("Er is een fout opgetreden bij het zoeken naar Gipod-gegevens.Probeer het opnieuw of controleer de ingestelde parameters.");

                        }
                    });
                });
            }
        }


        private gipodParam getGipodParam()
        {
            //get parameters form GUI
            gipodParam param = new gipodParam();
            param.city = SelectedGemeente;
            param.province = SelectedProvincie;
            param.owner = SelectedEigenaar;
            param.startdate = SelectedStartDate;
            param.enddate = SelectedEndDate;

            param.crs = CRS.Lambert72;

            if (ManifestionChecked)
            {
                param.gipodType = gipodtype.manifestation;
                param.eventtype = SelectedManifestatie;
            }
            else
            {
                param.gipodType = gipodtype.workassignment;
                param.eventtype = "";
            }
            //set bounds
            if (IsBeperk)
            {

                Envelope env4326 = MapView.Active.Extent;
                env4326 = GeometryEngine.Instance.Project(env4326, SpatialReferenceBuilder.CreateSpatialReference((int)CRS.Lambert72)) as Envelope;
                string extentBeforeTransform = env4326.XMin.ToString().Replace(',', '.') + "," + env4326.YMin.ToString().Replace(',', '.') + "|" + env4326.XMax.ToString().Replace(',', '.') + "," + env4326.YMax.ToString().Replace(',', '.');
                param.bbox = extentBeforeTransform;
            }
            else
            {
                param.bbox = null;
            }
            return param;
        }


        private List<gipodResponse> fetchGipod(gipodParam param)
        {
            //get parameters form GUI
            string city = param.city;
            string province = param.province;
            string owner = param.owner;

            string eventtype = param.eventtype;

            DateTime startdate = param.startdate;
            DateTime enddate = param.enddate;

            string bbox = param.bbox;

            CRS crs = param.crs;

            //get data from gipod
            List<gipodResponse> response;

            if (param.gipodType == gipodtype.manifestation)
            {
                response = gipod.allManifestations(startdate, enddate, city, province, owner, eventtype, crs, bbox);
                return response;
            }
            else if (param.gipodType == gipodtype.workassignment)
            {
                response = gipod.allWorkassignments(startdate, enddate, city, province, owner, crs, bbox);
                return response;
            }
            else return null;
        }

        private void updateDataGrid(List<gipodResponse> gipodResponses)
        {

            List<Graphic> graphics = new List<Graphic>();
            if (gipodResponses != null && gipodResponses.Count > 0)
            {


                foreach (gipodResponse gipodResponse in gipodResponses)
                {
                    // need attributes and geometry
                    PropertyInfo[] properties = gipodResponse.GetType().GetProperties();

                    Dictionary<string, object> attributes = properties.Where(p => p.Name != "coordinate").ToDictionary(propInfo => propInfo.Name, propInfo => propInfo.GetValue(gipodResponse, null));

                    List<double> coordinates = gipodResponse.coordinate.coordinates;
                    double x = coordinates[0];
                    double y = coordinates[1];

                    graphics.Add(new Graphic(attributes, utils.CreateMapPoint(x, y, lambertSpatialReference)));


                }
            }
            GraphicsList = new ObservableCollection<Graphic>(graphics);

        }



        private ObservableCollection<Graphic> _graphicsList = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> GraphicsList
        {
            get { return _graphicsList; }
            set
            {
                SetProperty(ref _graphicsList, value);
                MarkedGraphicsList.Clear();
                updateListMarkeer();
                TextMarkeerAlles = "Markeer alles";
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
                    if (MarkedGraphicsList.Any(saveGraphic => ComparasionString(saveGraphic) == ComparasionString(_selectedGraphic)))
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
                //ActiveButtonMarkeer = true;
                //updatePercelFromSelectedPerceelToSave(_GIPODData);
            }
        }

        private bool _selectedGraphicIsSelected = false;
        public bool SelectedGraphicIsSelected
        {
            get { return _selectedGraphicIsSelected; }
            set
            {
                SetProperty(ref _selectedGraphicIsSelected, value);
                //ActiveButtonMarkeer = true;
                //updatePercelFromSelectedPerceelToSave(_GIPODData);
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

        public void MarkGraphic(Graphic SelectedGraphic)
        {
            if (!MarkedGraphicsList.Any(saveGraphic => ComparasionString(saveGraphic) == ComparasionString(SelectedGraphic)))
            {
                MarkedGraphicsList.Add(SelectedGraphic);
                updateListMarkeer();
                TextMarkeer = "Verwijder markering";
            }
            else
            {

                Graphic pointToDelete = MarkedGraphicsList.Where(saveGraphic => ComparasionString(saveGraphic) == ComparasionString(SelectedGraphic)).FirstOrDefault();
                MarkedGraphicsList.Remove(pointToDelete);
                updateListMarkeer();
                TextMarkeer = "Markeer";
            }
        }

        public void updateListMarkeer()
        {

            utils.UpdateMarking((from markedGraphic in MarkedGraphicsList select markedGraphic.Geometry).ToList());

        }


        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdZoom");
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


        public ICommand CmdMarkeerAll
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdMarkeerAll");

                    //await QueuedTask.Run(() =>
                    //{
                    //    if (overlays.Count > 0)
                    //    {
                    //        foreach (IDisposable overlay in overlays)
                    //        {
                    //            overlay.Dispose();
                    //        }
                    //        overlays = new ObservableCollection<IDisposable>();


                    //    }

                    //    CIMPointSymbol symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                    //    CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                    //    foreach (Graphic GIPODData in GraphicsList)
                    //    {
                    //        overlays.Add(MapView.Active.AddOverlay(GIPODData.Geometry, symbolReference));
                    //    }

                    //    foreach (Graphic GIPODData in GraphicsList)
                    //    {
                    //        MarkedGraphicsList.Add(SelectedGraphic);
                    //    }

                    //    MarkedGraphicsList.Clear();

                    //    updateListMarkeer();


                    //});



                    if (!(MarkedGraphicsList.Count > 10))
                    {
                        MarkedGraphicsList = new ObservableCollection<Graphic>();
                        foreach (Graphic GIPODData in GraphicsList)
                        {
                            MarkedGraphicsList.Add(GIPODData);
                        }
                        updateListMarkeer();
                        TextMarkeerAlles = "Verwijder alles";


                    }
                    else
                    {

                        MarkedGraphicsList = new ObservableCollection<Graphic>();
                        updateListMarkeer();
                        TextMarkeerAlles = "Markeer alles";
                    }
                });
            }
        }



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
        private string _heading = "Bevraag GIPOD";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Gipod_ShowButton : Button
    {
        protected override void OnClick()
        {
            GipodViewModel.Show();
        }
    }
}
