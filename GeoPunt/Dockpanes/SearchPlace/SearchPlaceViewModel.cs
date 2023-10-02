using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Linq;

namespace GeoPunt.Dockpanes.SearchPlace
{
    internal class SearchPlaceViewModel : DockPane, IMarkedGraphicDisplayer
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPlace";
        private Helpers.Utils utils = new Helpers.Utils();

        private SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);

        poi poiDH;
        municipalityList municipalities;
        adresLocation adresLocation;

        public SearchPlaceViewModel()
        {
            poiDH = new poi(5000);
            adresLocation = new adresLocation(5000);
            initGui();
            ActiveRemoveButton = false;
            TextMarkeer = "Markeer";
            //LoadCollectionData();
        }

        public void initGui()
        {
            //rows = new SortableBindingList<poiDataRow>();
            //resultGrid.DataSource = rows;

            capakey capa = new capakey(5000);

            municipalities = capa.getMunicipalities();
            List<string> cities = (from municipality t in municipalities.municipalities
                                   orderby t.municipalityName
                                   select t.municipalityName).ToList();
            cities.Insert(0, "");
            GemeenteList = cities;

            ThemeList = poiDH.listThemes().categories;
            //CategoriesList = null;
            //CategoriesList = poiDH.listCategories().categories;
            //TypesList = null;
            //TypesList = poiDH.listPOItypes().categories;

            populateFilters();
        }


        private void populateFilters()
        {
            ThemeListString = (from n in ThemeList orderby n.value select n.value).ToList();
            ThemeListString.Insert(0, "");

            if (CategoriesList.Count > 0)
            {
                CategoriesListString = (from n in CategoriesList orderby n.value select n.value).ToList();
                CategoriesListString.Insert(0, "");
            }

            if (TypesList.Count > 0)
            {
                TypesListString = (from n in TypesList orderby n select n).ToList();
                TypesListString.Insert(0, "");
            }

            //themeCbx.Items.Clear();
            //themeCbx.Items.AddRange(themeList.ToArray());
            //categoryCbx.Items.Clear();
            //categoryCbx.Items.AddRange(categoriesList.ToArray());
            //typeCbx.Items.Clear();
            //typeCbx.Items.AddRange(poiTypeList.ToArray());

        }

        private string _textVoegAlle = "Voeg Alle";
        public string TextVoegAlle
        {
            get { return _textVoegAlle; }
            set
            {
                SetProperty(ref _textVoegAlle, value);
            }
        }

        private bool _isEnabledGemeente = true;
        public bool IsEnabledGemeente
        {
            get { return _isEnabledGemeente; }
            set
            {
                SetProperty(ref _isEnabledGemeente, value);
            }
        }

        private bool _isEnableButtonZoek = true;
        public bool IsEnableButtonZoek
        {
            get { return _isEnableButtonZoek; }
            set
            {
                SetProperty(ref _isEnableButtonZoek, value);
            }
        }

        private bool _isBeperk;
        public bool IsBeperk
        {
            get { return _isBeperk; }
            set
            {
                SetProperty(ref _isBeperk, value);
                QueuedTask.Run(() =>
                {
                    IsEnabledGemeente = !value;
                    ButtonFreeze();
                });
            }
        }

        public async void ButtonFreeze()
        {
            IsEnableButtonZoek = false;
            await Task.Delay(3500);
            IsEnableButtonZoek = true;
        }

        private bool _activeRemoveButton;
        public bool ActiveRemoveButton
        {
            get { return _activeRemoveButton; }
            set
            {
                SetProperty(ref _activeRemoveButton, value);
            }
        }

        private bool _activeSaveButton = true;
        public bool ActiveSaveButton
        {
            get { return _activeSaveButton; }
            set
            {
                SetProperty(ref _activeSaveButton, value);
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
                ActiveRemoveButton = true;
                ActiveSaveButton = false;

                if (_selectedGraphic != null)
                {

                    if (MarkedGraphicsList.Any(markedGraphic => markedGraphic.Attributes["Straat"] + ", " + markedGraphic.Attributes["Gemeente"] == _selectedGraphic.Attributes["Straat"] + ", " + _selectedGraphic.Attributes["Gemeente"]))
                    {

                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {
                        TextMarkeer = "Markeer";
                    }
                }

            }
        }

        MapPoint MapPointSelectedAddressSimple = null;
        private ObservableCollection<DataRowSearchPlaats> _interessantePlaatsList = new ObservableCollection<DataRowSearchPlaats>();
        public ObservableCollection<DataRowSearchPlaats> InteressantePlaatsList
        {
            get { return _interessantePlaatsList; }
            set
            {
                SetProperty(ref _interessantePlaatsList, value);
            }
        }

        private DataRowSearchPlaats _selectedInteressantePlaatsList;
        public DataRowSearchPlaats SelectedInteressantePlaatsList
        {
            get { return _selectedInteressantePlaatsList; }
            set
            {
                SetProperty(ref _selectedInteressantePlaatsList, value);

                double x = 0;
                double y = 0;
                string var = _selectedInteressantePlaatsList.Straat + ", " + _selectedInteressantePlaatsList.Gemeente;


                List<locationResult> loc = adresLocation.getAdresLocation(var, 1);
                foreach (locationResult item in loc)
                {
                    x = item.Location.X_Lambert72;
                    y = item.Location.Y_Lambert72;
                }

                MapPointSelectedAddressSimple = utils.CreateMapPoint(x, y, lambertSpatialReference);

                ActiveRemoveButton = false;
                ActiveSaveButton = true;
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


        private string _keyWordString;
        public string KeyWordString
        {
            get { return _keyWordString; }
            set
            {
                SetProperty(ref _keyWordString, value);
            }
        }

        private string _selectedGemeenteList;
        public string SelectedGemeenteList
        {
            get { return _selectedGemeenteList; }
            set
            {
                SetProperty(ref _selectedGemeenteList, value);
                ThemeListString = new List<string>();
                ThemeListString = (from n in ThemeList orderby n.value select n.value).ToList();
                ThemeListString.Insert(0, "");
                CategoriesListString = new List<string>();
                TypesListString = new List<string>();
                KeyWordString = "";
                ButtonFreeze();
            }
        }





        private List<string> _gemeenteList = new List<string>();
        public List<string> GemeenteList
        {
            get { return _gemeenteList; }
            set
            {
                SetProperty(ref _gemeenteList, value);
            }
        }

        private List<poiValueGroup> _themeList = new List<poiValueGroup>();
        public List<poiValueGroup> ThemeList
        {
            get { return _themeList; }
            set
            {
                SetProperty(ref _themeList, value);
            }
        }

        private string _selectedThemeListString;
        public string SelectedThemeListString
        {
            get { return _selectedThemeListString; }
            set
            {
                SetProperty(ref _selectedThemeListString, value);
                CategoriesList = poiDH.listCategories(_selectedThemeListString).categories;
                CategoriesListString = new List<string>();
                CategoriesListString = (from n in CategoriesList orderby n.value select n.value).ToList();
                CategoriesListString.Insert(0, "");
                TypesListString = new List<string>();
                KeyWordString = "";
                ButtonFreeze();
            }
        }

        private string _selectedCategoriesListString;
        public string SelectedCategoriesListString
        {
            get { return _selectedCategoriesListString; }
            set
            {
                SetProperty(ref _selectedCategoriesListString, value);
                string themeCode = theme2code(SelectedThemeListString);
                string catCode = cat2code(SelectedCategoriesListString);

                poiCategories qry = poiDH.listPOItypes(themeCode, catCode);
                List<string> poiTypeList = (from n in qry.categories orderby n.value select n.value).ToList();
                poiTypeList.Insert(0, "");

                KeyWordString = "";

                TypesListString = new List<string>();
                TypesListString = poiTypeList.ToList();
                ButtonFreeze();
            }
        }

        private string _selectedTypesListString;
        public string SelectedTypesListString
        {
            get { return _selectedTypesListString; }
            set
            {
                SetProperty(ref _selectedTypesListString, value);
                KeyWordString = "";
                ButtonFreeze();
            }
        }

        private List<string> _themeListString = new List<string>();
        public List<string> ThemeListString
        {
            get { return _themeListString; }
            set
            {
                SetProperty(ref _themeListString, value);
            }
        }

        private List<string> _categoriesListString = new List<string>();
        public List<string> CategoriesListString
        {
            get { return _categoriesListString; }
            set
            {
                SetProperty(ref _categoriesListString, value);
            }
        }

        private List<string> _typesListString = new List<string>();
        public List<string> TypesListString
        {
            get { return _typesListString; }
            set
            {
                SetProperty(ref _typesListString, value);
            }
        }

        private List<poiValueGroup> _categoriesList = new List<poiValueGroup>();
        public List<poiValueGroup> CategoriesList
        {
            get { return _categoriesList; }
            set
            {
                SetProperty(ref _categoriesList, value);
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

        private List<string> _typesList = new List<string>();
        public List<string> TypesList
        {
            get { return _typesList; }
            set
            {
                SetProperty(ref _typesList, value);
            }
        }

        List<poiMaxModel> listPois = new List<poiMaxModel>();
        private void updateDataGrid(List<poiMaxModel> pois)
        {
            InteressantePlaatsList = new ObservableCollection<DataRowSearchPlaats>();
            //parse results
            foreach (poiMaxModel poi in pois)
            {
                DataRowSearchPlaats row = new DataRowSearchPlaats();
                List<string> qry;
                poiAddress adres;

                row.id = poi.id;
                //row.Omschrijving = "";
                //if (poi.description != null)
                //{
                //    row.Omschrijving = poi.description.value;
                //}

                qry = (from poiValueGroup n in poi.categories
                       where n.type == "Type"
                       select n.value).ToList();
                if (qry.Count > 0) row.Type = qry[0];

                qry = (from poiValueGroup n in poi.categories
                       where n.type == "Categorie"
                       select n.value).ToList();
                if (qry.Count > 0) row.Categorie = qry[0];
                //if (row.Categorie == null) row.Categorie = SelectedCategoriesListString;


                qry = (from poiValueGroup n in poi.categories
                       where n.type == "Thema"
                       select n.value).ToList();
                if (qry.Count > 0) row.Thema = qry[0];

                qry = (
                    from poiValueGroup n in poi.labels
                    select n.value).ToList();
                //row.Naam = string.Join(", ", qry.ToArray());
                row.Naam = qry[0];





                adres = poi.location.address;
                if (adres != null)
                {
                    row.Straat = adres.street;
                    row.Huisnummer = adres.streetnumber;
                    row.Postcode = adres.postalcode;
                    row.Gemeente = adres.municipality;
                }


                InteressantePlaatsList.Add(row);

            }


        }

        private string municipality2nis(string muniName)
        {
            if (muniName == null || muniName == "") return "";

            var niscodes = 
                from n in municipalities.municipalities
                where n.municipalityName == muniName
                select n.municipalityCode;

            if (niscodes.Count() == 0) return "";

            return niscodes.First();
        }

        private string theme2code(string theme)
        {
            if (theme == null || theme == "") return "";

            var themeCodes = from n in ThemeList
                              where n.value == theme
                              select n.term;
            if (themeCodes.Count() == 0) return "";

            return themeCodes.First();
        }

        private string cat2code(string cat)
        {
            if (cat == null || cat == "") return "";

            var catCodes = from n in poiDH.listCategories().categories
                            where n.value == cat
                            select n.term;
            if (catCodes.Count() == 0) return "";

            return catCodes.First();
        }

        private string poitype2code(string poiType)
        {
            if (poiType == null || poiType == "") return "";

            var typeCodes = from n in poiDH.listPOItypes().categories
                             where n.value == poiType
                             select n.term;
            if (typeCodes.Count() == 0) return "";

            return typeCodes.First();
        }

        public void updatePOIMarkeer()
        {
            foreach (Graphic markedGraphic in MarkedGraphicsList)
            {
                GeocodeUtils.UpdateMapOverlay(markedGraphic.Geometry as MapPoint, MapView.Active, true);
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

        public void MarkGraphic(Graphic SelectedGraphic)
        {

            if (!MarkedGraphicsList.Any(markedGraphic => markedGraphic.Attributes["Straat"] + ", " + markedGraphic.Attributes["Gemeente"] == SelectedGraphic.Attributes["Straat"] + ", " + SelectedGraphic.Attributes["Gemeente"]))
            {
                MarkedGraphicsList.Add(SelectedGraphic);
                updatePOIMarkeer();
                TextMarkeer = "Verwijder markering";
            }
            else
            {

                Graphic pointToDelete = MarkedGraphicsList.Where(markedGraphic => markedGraphic.Attributes["Straat"] + ", " + markedGraphic.Attributes["Gemeente"] == SelectedGraphic.Attributes["Straat"] + ", " + SelectedGraphic.Attributes["Gemeente"]).First();
                MarkedGraphicsList.Remove(pointToDelete);
                GeocodeUtils.UpdateMapOverlay(pointToDelete.Geometry as MapPoint, MapView.Active, true, true);
                updatePOIMarkeer();
                TextMarkeer = "Markeer";
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

        public ICommand CmdZoek
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    poiMaxResponse poiData = null;
                    InteressantePlaatsList = new ObservableCollection<DataRowSearchPlaats>();

                    //input
                    string themeCode = theme2code(SelectedThemeListString);
                    string catCode = cat2code(SelectedCategoriesListString);
                    string poiTypeCode = poitype2code(SelectedTypesListString);
                    string keyWord = KeyWordString;
                    bool cluster = false;
                    string nis;
                    string extent;

                    if (IsBeperk)
                    {


                        Envelope env4326 = MapView.Active.Extent;
                        env4326 = GeometryEngine.Instance.Project(env4326, SpatialReferenceBuilder.CreateSpatialReference(4326)) as Envelope;
                        string extentBeforeTransform = env4326.XMin + "|" + env4326.YMin + "|" + env4326.XMax + "|" + env4326.YMax;
                        extent = extentBeforeTransform.Replace(',', '.');
                        nis = null;
                    }
                    else
                    {
                        extent = null;
                        nis = municipality2nis(SelectedGemeenteList);

                    }
                    int count = 1000;

                    poiData = poiDH.getMaxmodel(keyWord, count, cluster, themeCode, catCode, poiTypeCode,
                    CRS.WGS84, null, nis, extent);

                    List<poiMaxModel> pois = poiData.pois;

                    if (pois.Count == 0 || pois == null)
                    {
                        MessageBox.Show("Geen poi gevonden");
                    }

                    //TextVoegAlle = $@"Voeg alle toe ({pois.Count})";
                    TextVoegAlle = $@"Voeg ({pois.Count})";

                    //MessageBox.Show($@"{pois.Count} interesting places found in {SelectedGemeenteList}");

                    listPois = pois;
                    updateDataGrid(pois);
                });
            }
        }
        public ICommand CmdSave
        {
            get
            {
                return new RelayCommand(async () =>
                {


                    Dictionary<string, object> attributes = new Dictionary<string, object>();

                    attributes["id"] = SelectedInteressantePlaatsList.id;
                    attributes["Thema"] = SelectedInteressantePlaatsList.Thema;
                    attributes["Categorie"] = SelectedInteressantePlaatsList.Categorie;
                    attributes["Type"] = SelectedInteressantePlaatsList.Type;
                    attributes["Naam"] = SelectedInteressantePlaatsList.Naam;
                    //attributes["Omschrijving"] = SelectedInteressantePlaatsList.Omschrijving;
                    attributes["Straat"] = SelectedInteressantePlaatsList.Straat;
                    attributes["busnr"] = SelectedInteressantePlaatsList.busnr;
                    attributes["Gemeente"] = SelectedInteressantePlaatsList.Gemeente;
                    attributes["Postcode"] = SelectedInteressantePlaatsList.Postcode;
                    attributes["Huisnummer"] = SelectedInteressantePlaatsList.Huisnummer;

                    Graphic graphic = new Graphic(attributes, MapPointSelectedAddressSimple);

                    GraphicsList.Add(graphic);
                });
            }
        }

        public ICommand CmdRemove
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    Graphic graphic = GraphicsList.Where(graphic => graphic.Attributes["Straat"] + ", " + graphic.Attributes["Gemeente"] == SelectedGraphic.Attributes["Straat"] + ", " + SelectedGraphic.Attributes["Gemeente"]).FirstOrDefault();
                    Graphic graphicMarked = MarkedGraphicsList.Where(markedGraphic => markedGraphic.Attributes["Straat"] + ", " + markedGraphic.Attributes["Gemeente"] == SelectedGraphic.Attributes["Straat"] + ", " + SelectedGraphic.Attributes["Gemeente"]).FirstOrDefault();

                    if (graphic != null)
                    {
                        GraphicsList.Remove(graphic);
                    }

                    if (graphicMarked != null)
                    {
                        MarkedGraphicsList.Remove(graphicMarked);
                        GeocodeUtils.UpdateMapOverlay(graphicMarked.Geometry as MapPoint, MapView.Active, true, true);
                    }

                    updatePOIMarkeer();

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
                    utils.ZoomTo(SelectedGraphic.Geometry);
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
    internal class SearchPlace_ShowButton : Button
    {
        protected override void OnClick()
        {
            SearchPlaceViewModel.Show();
        }
    }
}
