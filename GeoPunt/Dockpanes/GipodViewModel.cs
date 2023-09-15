using ActiproSoftware.Windows.Extensions;
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

namespace GeoPunt.Dockpanes
{
    internal class GipodViewModel : DockPane
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
        }



        private void initGUI()
        {

            municipality = capakey.getMunicipalities();

            List<string> provincies = new List<string>() { "", "Antwerpen", "Limburg", "Vlaams-Brabant", "Oost-Vlaanderen", "West-Vlaanderen" };
            List<string> municipalities = (from datacontract.municipality t in municipality.municipalities select t.municipalityName).ToList();
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

        private void provinceCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

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
            switch (SelectedProvincie)
            {
                case "Antwerpen":
                    cities = (from datacontract.municipality t in municipality.municipalities
                              where t.municipalityCode.StartsWith("1")
                              select t.municipalityName).ToList();
                    break;
                case "Limburg":
                    cities = (from datacontract.municipality t in municipality.municipalities
                              where t.municipalityCode.StartsWith("7")
                              select t.municipalityName).ToList();
                    break;
                case "Vlaams-Brabant":
                    cities = (from datacontract.municipality t in municipality.municipalities
                              where t.municipalityCode.StartsWith("2")
                              select t.municipalityName).ToList();
                    break;
                case "Oost-Vlaanderen":
                    cities = (from datacontract.municipality t in municipality.municipalities
                              where t.municipalityCode.StartsWith("4")
                              select t.municipalityName).ToList();
                    break;
                case "West-Vlaanderen":
                    cities = (from datacontract.municipality t in municipality.municipalities
                              where t.municipalityCode.StartsWith("3")
                              select t.municipalityName).ToList();
                    break;
                default:
                    cities = (from datacontract.municipality t in municipality.municipalities
                              select t.municipalityName).ToList();
                    break;
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

                    ListGIPODData = new ObservableCollection<Graphic>();

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
                            MessageBox.Show(ex.Message + " : " + ex.StackTrace);
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

            Debug.WriteLine(bbox);
            Console.WriteLine(bbox.ToString());

            CRS crs = param.crs;

            //get data from gipod
            List<datacontract.gipodResponse> response;

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
            ListGIPODData = new ObservableCollection<Graphic>(graphics);

        }



        private ObservableCollection<Graphic> _listGIPODData = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> ListGIPODData
        {
            get { return _listGIPODData; }
            set
            {
                SetProperty(ref _listGIPODData, value);
                if (_listGIPODData.Count > 0)
                {
                    ListGIPODHaveData = true;
                }
                else
                {
                    ListGIPODHaveData = false;
                }
            }
        }

        private bool _listGIPODHaveData = false;
        public bool ListGIPODHaveData
        {
            get { return _listGIPODHaveData; }
            set
            {
                SetProperty(ref _listGIPODHaveData, value);
            }
        }


        private Graphic _selectedGIPODData;
        public Graphic SelectedGIPODData
        {
            get { return _selectedGIPODData; }
            set
            {
                SetProperty(ref _selectedGIPODData, value);
                if (_selectedGIPODData != null)
                {
                    GIPODDataIsSelected = true;
                }
                else
                {
                    GIPODDataIsSelected = false;
                }
                //ActiveButtonMarkeer = true;
                //updatePercelFromSelectedPerceelToSave(_GIPODData);
            }
        }

        private bool _gipodDataIsSelected = false;
        public bool GIPODDataIsSelected
        {
            get { return _gipodDataIsSelected; }
            set
            {
                SetProperty(ref _gipodDataIsSelected, value);
                //ActiveButtonMarkeer = true;
                //updatePercelFromSelectedPerceelToSave(_GIPODData);
            }
        }



        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdZoom");
                    utils.ZoomTo(SelectedGIPODData.Geometry);
                });
            }
        }


        public ICommand CmdMarkeer
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Debug.WriteLine("CmdMarkeer");

                    await QueuedTask.Run(() =>
                    {
                        if (overlays.Count > 0)
                        {
                            foreach (IDisposable overlay in overlays)
                            {
                                overlay.Dispose();
                            }
                            overlays = new ObservableCollection<IDisposable>();

                        }

                        CIMPointSymbol symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                        CIMSymbolReference symbolReference = symbol.MakeSymbolReference();
                        overlays.Add(MapView.Active.AddOverlay(SelectedGIPODData.Geometry, symbolReference));
                        
                    });
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

                    await QueuedTask.Run(() =>
                    {
                        if (overlays.Count > 0)
                        {
                            foreach (IDisposable overlay in overlays)
                            {
                                overlay.Dispose();
                            }
                            overlays = new ObservableCollection<IDisposable>();


                        }

                        CIMPointSymbol symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 10.0, SimpleMarkerStyle.Diamond);
                        CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

                        foreach (Graphic GIPODData in ListGIPODData)
                        {
                            overlays.Add(MapView.Active.AddOverlay(GIPODData.Geometry, symbolReference));
                        }
                    });
                });
            }
        }



        public ICommand CmdExport
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ExportToGeoJson(ListGIPODData.ToList());
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
