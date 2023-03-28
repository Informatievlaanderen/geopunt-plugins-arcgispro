using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Core.Internal.Geometry;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace GeoPunt.Dockpanes
{
    internal class SearchPerceelViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPerceel";

        private string _textMarkeer;
        public string TextMarkeer
        {
            get { return _textMarkeer; }
            set
            {
                SetProperty(ref _textMarkeer, value);
            }
        }

        private ObservableCollection<DataRowParcel> _listSaveParceels = new ObservableCollection<DataRowParcel>();
        public ObservableCollection<DataRowParcel> ListSaveParceels
        {
            get { return _listSaveParceels; }
            set
            {
                SetProperty(ref _listSaveParceels, value);
            }
        }

        private DataRowParcel _selectedSaveParceel;
        public DataRowParcel SelectedSaveParceel
        {
            get { return _selectedSaveParceel; }
            set
            {
                SetProperty(ref _selectedSaveParceel, value);
                ActiveButtonMarkeer = true;
                updatePercelFromSelectedPerceelToSave(_selectedSaveParceel);
            }
        }

        private bool _activeButtonMarkeer;
        public bool ActiveButtonMarkeer
        {
            get { return _activeButtonMarkeer; }
            set
            {
                SetProperty(ref _activeButtonMarkeer, value);
            }
        }

        private bool _activeButtonSave;
        public bool ActiveButtonSave
        {
            get { return _activeButtonSave; }
            set
            {
                SetProperty(ref _activeButtonSave, value);
            }
        }

        private List<string> _listGemeente = new List<string>();
        public List<string> ListGemeente
        {
            get { return _listGemeente; }
            set
            {
                SetProperty(ref _listGemeente, value);
            }
        }

        private string _selectedListGemeente;
        public string SelectedListGemeente
        {
            get { return _selectedListGemeente; }
            set
            {
                SetProperty(ref _selectedListGemeente, value);
                gemeenteSelectionChange();
            }
        }

        private List<string> _listDepartments = new List<string>();
        public List<string> ListDepartments
        {
            get { return _listDepartments; }
            set
            {
                SetProperty(ref _listDepartments, value);
            }
        }

        private string _selectedListSecties;
        public string SelectedListSecties
        {
            get { return _selectedListSecties; }
            set
            {
                SetProperty(ref _selectedListSecties, value);
                sectieSelectionChange();
            }
        }

        private List<string> _listSecties = new List<string>();
        public List<string> ListSecties
        {
            get { return _listSecties; }
            set
            {
                SetProperty(ref _listSecties, value);
            }
        }

        private string _selectedListParcels;
        public string SelectedListParcels
        {
            get { return _selectedListParcels; }
            set
            {
                SetProperty(ref _selectedListParcels, value);
                parcelSelectionChange();
            }
        }

        private List<string> _listParcels = new List<string>();
        public List<string> ListParcels
        {
            get { return _listParcels; }
            set
            {
                SetProperty(ref _listParcels, value);
            }
        }

        private string _selectedListDepartments;
        public string SelectedListDepartments
        {
            get { return _selectedListDepartments; }
            set
            {
                SetProperty(ref _selectedListDepartments, value);
                departmentSelectionChange();
            }
        }

        List<datacontract.municipality> municipalities;
        List<datacontract.department> departments;
        List<datacontract.parcel> parcels;
        datacontract.parcel perceel;
        datacontract.parcel perceelToSave;

        DataHandler.capakey capakey;
        public SearchPerceelViewModel() 
        {
            capakey = new DataHandler.capakey(5000);
            perceel = null;
            municipalities = capakey.getMunicipalities().municipalities;
            ListGemeente = (from n in municipalities select n.municipalityName).ToList();
            ActiveButtonSave = false;
            ActiveButtonMarkeer = false;
            TextMarkeer = "Markeer";
        }
        private void gemeenteSelectionChange()
        {
            ActiveButtonSave = false;
            ActiveButtonMarkeer = false;
            ListDepartments = new List<string>();
            ListSecties = new List<string>();
            ListParcels = new List<string>();

            string gemeente = SelectedListGemeente;
            string niscode = municipality2nis(gemeente);

            if (niscode == "" || niscode == null) return;

            departments = capakey.getDepartments(int.Parse(niscode)).departments;
            ListDepartments = (from n in departments select n.departmentName).ToList();
        }

        public void departmentSelectionChange()
        {
            ActiveButtonSave = false;
            ActiveButtonMarkeer = false;
            ListSecties = new List<string>();
            ListParcels = new List<string>();

            string gemeente = SelectedListGemeente;
            string niscode = municipality2nis(gemeente);

            string department = SelectedListDepartments;
            string depCode = department2code(department);

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;

            List<datacontract.section> secties = capakey.getSecties(int.Parse(niscode), int.Parse(depCode)).sections;
            ListSecties = (from n in secties select n.sectionCode).ToList();
        }

        public void sectieSelectionChange()
        {
            ActiveButtonSave = false;
            ActiveButtonMarkeer = false;
            ListParcels = new List<string>();

            string gemeente = SelectedListGemeente;
            string niscode = municipality2nis(gemeente);

            string department = SelectedListDepartments;
            string depCode = department2code(department);

            string sectie = SelectedListSecties;

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;
            if (sectie == "" || sectie == null) return;

            parcels = capakey.getParcels(int.Parse(niscode), int.Parse(depCode), sectie).parcels;
            ListParcels = (from n in parcels select n.perceelnummer).ToList();
        }

        public void updatePercelFromSelectedPerceelToSave(DataRowParcel dataRowParcel)
        {
            if(dataRowParcel == null)
            {
                return;
            }

            ActiveButtonSave = false;
            ActiveButtonMarkeer = false;
            string gemeente = SelectedListGemeente;
            string niscode = municipality2nis(dataRowParcel.Gemeente);

            string department = SelectedListDepartments;
            string depCode = department2code(dataRowParcel.Department);

            string sectie = dataRowParcel.Sectie;
            string parcelNr = dataRowParcel.Perceel;

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;
            if (sectie == "" || sectie == null) return;
            if (parcelNr == "" || parcelNr == null) return;

            ActiveButtonMarkeer = true;

            

            if (ListStringPercel.Contains(parcelNr)){
                TextMarkeer = "Verwijder markering";
            }
            else
            {
                TextMarkeer = "Markeer";
            }
            

            perceelToSave = capakey.getParcel(int.Parse(niscode), int.Parse(depCode), sectie, parcelNr,
                                                   DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
        }

        public void parcelSelectionChange()
        {
            ActiveButtonSave = false;
            ActiveButtonMarkeer = false;
            string gemeente = SelectedListGemeente;
            string niscode = municipality2nis(gemeente);

            string department = SelectedListDepartments;
            string depCode = department2code(department);

            string sectie = SelectedListSecties;
            string parcelNr = SelectedListParcels;

            if (niscode == "" || niscode == null) return;
            if (depCode == "" || depCode == null) return;
            if (sectie == "" || sectie == null) return;
            if (parcelNr == "" || parcelNr == null) return;

            ActiveButtonSave = true;

            perceel = capakey.getParcel(int.Parse(niscode), int.Parse(depCode), sectie, parcelNr,
                                                   DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
        }

        private string municipality2nis(string muniName)
        {
            if (muniName == null || muniName == "") return "";

            var niscodes = (
                from n in municipalities
                where n.municipalityName == muniName
                select n.municipalityCode);

            if (niscodes.Count() == 0) return "";

            return niscodes.First<string>();
        }

        private string department2code(string depName)
        {
            if (depName == null || depName == "") return "";

            var depcodes = (
                from n in departments
                where n.departmentName == depName
                select n.departmentCode);

            if (depcodes.Count() == 0) return "";

            return depcodes.First<string>();
        }

        private ObservableCollection<MapPoint> LisPointsFromPolygones = new ObservableCollection<MapPoint>();
        private ObservableCollection<MapPoint> ListPointsFromPolygonesToMarkeer = new ObservableCollection<MapPoint>();
        private ObservableCollection<ObservableCollection<MapPoint>> ListPolygonesToMarkeer = new ObservableCollection<ObservableCollection<MapPoint>>();
        private ObservableCollection<string> ListStringPercel = new ObservableCollection<string>();

        public void updateListPointFromPolygone()
        {
            foreach (MapPoint mapPoint in LisPointsFromPolygones)
            {
                GeocodeUtils.UpdateMapOverlayMapPoint(mapPoint, MapView.Active, true);
            }
        }

        private CIMPointSymbol CreatePoinSymbol(System.Drawing.Color color, double size)
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
                _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
            }
        }

        private static ObservableCollection<System.IDisposable> _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
        private static ObservableCollection<System.IDisposable> _overlayObjectPerceelToMarkeer = new ObservableCollection<System.IDisposable>();

        
        
        
        ArcGIS.Core.Geometry.Polygon lastPoly;
        private async void createGrapicAndZoomTo(string capakeyResponse, datacontract.geojson Geom)
        {
            

            if (Geom.type == "MultiPolygon")
            {
         
                datacontract.geojsonMultiPolygon muniPolygons =
                                  JsonConvert.DeserializeObject<datacontract.geojsonMultiPolygon>(capakeyResponse);

                foreach (datacontract.geojsonPolygon poly in muniPolygons.toPolygonList())
                {
                    MessageBox.Show($@"Multipolygones :: {poly}");

                }
            }
            else if (Geom.type == "Polygon")
            {
                datacontract.geojsonPolygon municipalityPolygon =
                            JsonConvert.DeserializeObject<datacontract.geojsonPolygon>(capakeyResponse);
                MapPoint MapPointFromPolygone = null;
                LisPointsFromPolygones.Clear();


                foreach (var a in municipalityPolygon.coordinates)
                {
                    foreach (var b in a)
                    {

                        MapPointFromPolygone = MapPointBuilderEx.CreateMapPoint(b[0], b[1]);
                                                
                        LisPointsFromPolygones.Add(MapPointFromPolygone);

                    }
                }

                await QueuedTask.Run(() =>
                {
                    if (_overlayObjectPerceel.Count > 0)
                    {
   

                        foreach (var overlay in _overlayObjectPerceel)
                        {
                            overlay.Dispose();
                        }
                        _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
                        
                    }

                    

                    ArcGIS.Core.Geometry.Polygon poly = PolygonBuilderEx.CreatePolygon(LisPointsFromPolygones);

                    //Set symbolology, create and add element to layout
                    CIMStroke outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
                    CIMPolygonSymbol polySym = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlueRGB, SimpleFillStyle.ForwardDiagonal, outline);
                    

                    _overlayObjectPerceel.Add(MapView.Active.AddOverlay(poly,polySym.MakeSymbolReference()));
                    MapView.Active.ZoomTo(poly, new TimeSpan(0, 0, 0, 1));

                });
            }
        }
        private ObservableCollection<DataRowParcel> ListSavePerceel = new ObservableCollection<DataRowParcel>();
        public ICommand CmdSaveIcon
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    List<DataRowParcel> _data = new List<DataRowParcel>();
                    foreach (DataRowParcel item in ListSavePerceel)
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
        public ICommand CmdZoomGemeente
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    string gemeente = SelectedListGemeente;
                    string niscode = municipality2nis(gemeente);

                    if (niscode == "" || niscode == null) return;

                    try
                    {
                        datacontract.municipality municipality = capakey.getMunicipalitiy(int.Parse(niscode),
                                                                DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
                        datacontract.geojson municipalityGeom = JsonConvert.DeserializeObject<datacontract.geojson>(municipality.geometry.shape);

                        createGrapicAndZoomTo(municipality.geometry.shape, municipalityGeom);
                    }
                    catch (WebException wex)
                    {
                        if (wex.Status == WebExceptionStatus.Timeout)
                            MessageBox.Show("De connectie werd afgebroken." +
                                " Het duurde te lang voor de server een resultaat terug gaf.\n" +
                                "U kunt via de instellingen de 'timout'-tijd optrekken.", wex.Message);
                        else if (wex.Response != null)
                        {
                            string resp = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                            MessageBox.Show(resp, wex.Message);
                        }
                        else
                            MessageBox.Show(wex.Message, "Error");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + ": " + ex.StackTrace);
                    }
                });
            }
        }

        public ICommand CmdZoomDepartment
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    string gemeente = SelectedListGemeente;
                    string niscode = municipality2nis(gemeente);

                    string department = SelectedListDepartments;
                    string depCode = department2code(department);

                    if (niscode == "" || niscode == null) return;
                    if (depCode == "" || depCode == null) return;

                    datacontract.department dep = capakey.getDepartment(int.Parse(niscode), int.Parse(depCode),
                                                        DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
                    datacontract.geojson depGeom = JsonConvert.DeserializeObject<datacontract.geojson>(dep.geometry.shape);

                    createGrapicAndZoomTo(dep.geometry.shape, depGeom);
                });
            }
        }

        public ICommand CmdZoomSectie
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    string gemeente = SelectedListGemeente;
                    string niscode = municipality2nis(gemeente);

                    string department = SelectedListDepartments;
                    string depCode = department2code(department);

                    string sectie = SelectedListSecties;

                    if (niscode == "" || niscode == null) return;
                    if (depCode == "" || depCode == null) return;
                    if (sectie == "" || sectie == null) return;

                    datacontract.section sec = capakey.getSectie(int.Parse(niscode), int.Parse(depCode), sectie,
                                                        DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
                    datacontract.geojson secGeom = JsonConvert.DeserializeObject<datacontract.geojson>(sec.geometry.shape);

                    createGrapicAndZoomTo(sec.geometry.shape, secGeom);
                });
            }
        }

        public ICommand CmdZoomParcel
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    string gemeente = SelectedListGemeente;
                    string niscode = municipality2nis(gemeente);

                    string department = SelectedListDepartments;
                    string depCode = department2code(department);

                    string sectie = SelectedListSecties;

                    if (niscode == "" || niscode == null) return;
                    if (depCode == "" || depCode == null) return;
                    if (sectie == "" || sectie == null) return;

                    datacontract.geojson secGeom = JsonConvert.DeserializeObject<datacontract.geojson>(perceel.geometry.shape);
                    createGrapicAndZoomTo(perceel.geometry.shape, secGeom);
                });
            }
        }

        public ICommand CmdZoomParcelFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    string gemeente = SelectedSaveParceel.Gemeente;
                    string niscode = municipality2nis(gemeente);

                    string department = SelectedSaveParceel.Department;
                    string depCode = department2code(department);

                    string sectie = SelectedSaveParceel.Sectie;

                    if (niscode == "" || niscode == null) return;
                    if (depCode == "" || depCode == null) return;
                    if (sectie == "" || sectie == null) return;

                    datacontract.geojson secGeom = JsonConvert.DeserializeObject<datacontract.geojson>(perceelToSave.geometry.shape);
                    createGrapicAndZoomTo(perceelToSave.geometry.shape, secGeom);
                });
            }
        }

        public ICommand CmdSave
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    DataRowParcel row = new DataRowParcel();

                    row.Gemeente = SelectedListGemeente;
                    row.Department = SelectedListDepartments;
                    row.Sectie = SelectedListSecties;
                    row.Perceel = SelectedListParcels;

                   

                    if (ListSaveParceels.FirstOrDefault(m => m.Perceel == row.Perceel) == null)
                    {
                        ListSaveParceels.Add(row);
                        ListSavePerceel.Add(row);
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
                    methodButtonMarkeer(true);

                    

                    DataRowParcel percelToDelete = ListSaveParceels.FirstOrDefault(p => p.Perceel == SelectedSaveParceel.Perceel);
                    ListSaveParceels.Remove(percelToDelete);
                    ListSavePerceel.Remove(percelToDelete);

                });
            }
        }


        public async void methodButtonMarkeer(bool isRemove = false)
        {
            if (perceelToSave == null) return;


            datacontract.geojsonPolygon municipalityPolygon =
                    JsonConvert.DeserializeObject<datacontract.geojsonPolygon>(perceelToSave.geometry.shape);
            MapPoint MapPointFromPolygone = null;



            foreach (var a in municipalityPolygon.coordinates)
            {
                ListPointsFromPolygonesToMarkeer = new ObservableCollection<MapPoint>();
                foreach (var b in a)
                {

                    MapPointFromPolygone = MapPointBuilderEx.CreateMapPoint(b[0], b[1]);

                    ListPointsFromPolygonesToMarkeer.Add(MapPointFromPolygone);


                }


                double aX = ListPointsFromPolygonesToMarkeer[0].X;
                double aY = ListPointsFromPolygonesToMarkeer[0].Y;
                bool isExist = false;

                foreach (var polygones in ListPolygonesToMarkeer)
                {
                    foreach (var mp in polygones)
                    {
                        if (mp.X == aX && mp.Y == aY)
                        {
                            isExist = true;
                        }
                    }
                }

                if (isRemove)
                {
                    isExist = true;

                    datacontract.geojsonPolygon municipalityPolygon2 =
                            JsonConvert.DeserializeObject<datacontract.geojsonPolygon>(perceelToSave.geometry.shape);
                    MapPoint MapPointFromPolygone2 = null;
                    //LisPointsFromPolygones.Clear();


                    foreach (var aa in municipalityPolygon.coordinates)
                    {
                        foreach (var b in aa)
                        {

                            MapPointFromPolygone = MapPointBuilderEx.CreateMapPoint(b[0], b[1]);

                            if(LisPointsFromPolygones.Count == 0)
                            {
                                break;
                            }

                            if (LisPointsFromPolygones[0].X == MapPointFromPolygone.X)
                            {
                                await QueuedTask.Run(() =>
                                {
                                    if (_overlayObjectPerceel.Count > 0)
                                    {
                                        foreach (var overlay in _overlayObjectPerceel)
                                        {
                                            overlay.Dispose();
                                        }
                                        _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();

                                    }
                                });

                            }
                            break;
                            
                        }
                    }
                }

                //if (ListPolygonesToMarkeer.FirstOrDefault(m => m.FirstOrDefault(mp => mp.X == aX && mp.Y == aY) == null) == null)
                if (!isExist)
                {
                    
                    if (SelectedSaveParceel != null)
                    {
                        ListPolygonesToMarkeer.Add(ListPointsFromPolygonesToMarkeer);
                        ListStringPercel.Add(SelectedSaveParceel.Perceel);
                        TextMarkeer = "Verwijder markering";
                    }
                    
                }
                else
                {
                    TextMarkeer = "Markeer";
                    ObservableCollection<MapPoint> pointToDelete = null;

                    foreach (var polygones in ListPolygonesToMarkeer)
                    {
                        if (polygones.FirstOrDefault(m => m.X == aX && m.Y == aY) != null)
                        {
                            //MessageBox.Show("trouvé");
                            pointToDelete = polygones;
                        }
                    }

                    if (SelectedSaveParceel != null)
                    {
                        ListStringPercel.Remove(SelectedSaveParceel.Perceel);
                    }

                    

                    if (pointToDelete != null)
                    {
                        ListPolygonesToMarkeer.Remove(pointToDelete);
                    }


                    if (_overlayObjectPerceelToMarkeer != null)
                    {
                        foreach (var overlay in _overlayObjectPerceelToMarkeer)
                        {
                            overlay.Dispose();
                        }
                        _overlayObjectPerceelToMarkeer = new ObservableCollection<System.IDisposable>();
                    }
                }
            }

            await QueuedTask.Run(() =>
            {
                //if(ListPolygonesToMarkeer.Count > 1)
                //{
                ArcGIS.Core.Geometry.Polygon lastPolyMulti = null;
                //MessageBox.Show($@"polygon.count > 1 :: {ListPolygonesToMarkeer.Count}");
                foreach (var polygon in ListPolygonesToMarkeer)
                {
                    ArcGIS.Core.Geometry.Polygon polyMulti = PolygonBuilderEx.CreatePolygon(polygon);
                    lastPolyMulti = polyMulti;
                    //Set symbolology, create and add element to layout
                    CIMStroke outlineMulti = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.GreenRGB, 2.0, SimpleLineStyle.Solid);
                    CIMPolygonSymbol polySymMulti = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreenRGB, SimpleFillStyle.ForwardDiagonal, outlineMulti);


                    _overlayObjectPerceelToMarkeer.Add(MapView.Active.AddOverlay(polyMulti, polySymMulti.MakeSymbolReference()));

                }

                if (lastPolyMulti != null)
                {
                    if (TextMarkeer == "Markeer")
                    {
                        return;
                    }
                    MapView.Active.ZoomTo(lastPolyMulti, new TimeSpan(0, 0, 0, 1));
                }

            });

        }



        public ICommand CmdMarkeer
        {
            get 
            {
                return new RelayCommand(async () =>
                {
                    methodButtonMarkeer();
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
    internal class SearchPerceel_ShowButton : Button
    {
        protected override void OnClick()
        {
            SearchPerceelViewModel.Show();
        }
    }
}
