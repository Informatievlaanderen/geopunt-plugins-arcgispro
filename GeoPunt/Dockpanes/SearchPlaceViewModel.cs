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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Linq;

namespace GeoPunt.Dockpanes
{
    internal class SearchPlaceViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPlace";

        private string _textVoegAlle = "Voeg Alle";
        public string TextVoegAlle
        {
            get { return _textVoegAlle; }
            set
            {
                SetProperty(ref _textVoegAlle, value);
            }
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

        private ObservableCollection<DataRowSearchPlaats> _favouriteInteressantePlaatsList = new ObservableCollection<DataRowSearchPlaats>();
        public ObservableCollection<DataRowSearchPlaats> FavouriteInteressantePlaatsList
        {
            get { return _favouriteInteressantePlaatsList; }
            set
            {
                SetProperty(ref _favouriteInteressantePlaatsList, value);
            }
        }

        private DataRowSearchPlaats _selectedFavouriteInteressantePlaatsList;
        public DataRowSearchPlaats SelectedFavouriteInteressantePlaatsList
        {
            get { return _selectedFavouriteInteressantePlaatsList; }
            set
            {
                SetProperty(ref _selectedFavouriteInteressantePlaatsList, value);
                ActiveRemoveButton = true;
                ActiveSaveButton = false;
            }
        }

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
                //string var = _selectedInteressantePlaatsList.Straat;
                if(_selectedInteressantePlaatsList != null)
                {
                    string var = _selectedInteressantePlaatsList.Straat + ", " + _selectedInteressantePlaatsList.Gemeente;
                    updateCurrentMapPoint(var, 1);
                }

                ActiveRemoveButton = false;
                ActiveSaveButton = true;
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
            }
        }

        MapPoint MapPointSelectedAddress = null;
        DataHandler.adresLocation adresLocation;
        public void updateCurrentMapPoint(string query, int count)
        {
            double x = 0;
            double y = 0;

            adresLocation = new DataHandler.adresLocation(5000);

            List<datacontract.locationResult> loc = adresLocation.getAdresLocation(query, count);
            foreach (datacontract.locationResult item in loc)
            {
                x = item.Location.X_Lambert72;
                y = item.Location.Y_Lambert72;

            }
            MapPointSelectedAddress = MapPointBuilderEx.CreateMapPoint(x, y);
            //MessageBox.Show($@"update: {MapPointSelectedAddress.X} || {MapPointSelectedAddress.Y}");
        }

        DataHandler.poi poiDH;
        datacontract.municipalityList municipalities;

        public SearchPlaceViewModel() 
        {
            poiDH = new DataHandler.poi(5000);
            initGui();
            ActiveRemoveButton = false;
            //LoadCollectionData();
        }

        public void initGui()
        {
            //rows = new SortableBindingList<poiDataRow>();
            //resultGrid.DataSource = rows;

            DataHandler.capakey capa = new DataHandler.capakey(5000);

            municipalities = capa.getMunicipalities();
            List<string> cities = (from datacontract.municipality t in municipalities.municipalities
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
            ThemeListString = (from n in ThemeList orderby n.value select n.value).ToList<string>();
            ThemeListString.Insert(0, "");

            if(CategoriesList.Count > 0)
            {
                CategoriesListString = (from n in CategoriesList orderby n.value select n.value).ToList<string>();
                CategoriesListString.Insert(0, "");
            }

            if(TypesList.Count > 0) 
            {
                TypesListString = (from n in TypesList orderby n select n).ToList<string>();
                TypesListString.Insert(0, "");
            }

            //themeCbx.Items.Clear();
            //themeCbx.Items.AddRange(themeList.ToArray());
            //categoryCbx.Items.Clear();
            //categoryCbx.Items.AddRange(categoriesList.ToArray());
            //typeCbx.Items.Clear();
            //typeCbx.Items.AddRange(poiTypeList.ToArray());

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

        private List<datacontract.poiValueGroup> _themeList = new List<datacontract.poiValueGroup>();
        public List<datacontract.poiValueGroup> ThemeList
        {
            get { return _themeList; }
            set
            {
                SetProperty(ref _themeList, value);
            }
        }

        private String _selectedThemeListString;
        public String SelectedThemeListString
        {
            get { return _selectedThemeListString; }
            set
            {
                SetProperty(ref _selectedThemeListString, value);
                CategoriesList = poiDH.listCategories(_selectedThemeListString).categories;
                CategoriesListString = (from n in CategoriesList orderby n.value select n.value).ToList<string>();
                CategoriesListString.Insert(0, "");
                TypesListString = new List<string>();
            }
        }

        private String _selectedCategoriesListString;
        public String SelectedCategoriesListString
        {
            get { return _selectedCategoriesListString; }
            set
            {
                SetProperty(ref _selectedCategoriesListString, value);
                string themeCode = theme2code(SelectedThemeListString);
                string catCode = cat2code(SelectedCategoriesListString);

                datacontract.poiCategories qry = poiDH.listPOItypes(themeCode, catCode);
                List<string> poiTypeList = (from n in qry.categories orderby n.value select n.value).ToList<string>();
                poiTypeList.Insert(0, "");

                TypesListString = new List<string>();
                TypesListString = poiTypeList.ToList();
            }
        }

        private String _selectedTypesListString;
        public String SelectedTypesListString
        {
            get { return _selectedTypesListString; }
            set
            {
                SetProperty(ref _selectedTypesListString, value);                
            }
        }

        private List<String> _themeListString = new List<String>();
        public List<String> ThemeListString
        {
            get { return _themeListString; }
            set
            {
                SetProperty(ref _themeListString, value);
            }
        }

        private List<String> _categoriesListString = new List<String>();
        public List<String> CategoriesListString
        {
            get { return _categoriesListString; }
            set
            {
                SetProperty(ref _categoriesListString, value);
            }
        }

        private List<String> _typesListString = new List<String>();
        public List<String> TypesListString
        {
            get { return _typesListString; }
            set
            {
                SetProperty(ref _typesListString, value);
            }
        }

        private List<datacontract.poiValueGroup> _categoriesList = new List<datacontract.poiValueGroup>();
        public List<datacontract.poiValueGroup> CategoriesList
        {
            get { return _categoriesList; }
            set
            {
                SetProperty(ref _categoriesList, value);
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

        List<datacontract.poiMaxModel> listPois = new List<datacontract.poiMaxModel>();
        private void updateDataGrid(List<datacontract.poiMaxModel> pois)
        {
            InteressantePlaatsList = new ObservableCollection<DataRowSearchPlaats>();
            //parse results
            foreach (datacontract.poiMaxModel poi in pois)
            {
                DataRowSearchPlaats row = new DataRowSearchPlaats();
                List<string> qry;
                datacontract.poiAddress adres;

                row.id = poi.id;
                row.Omschrijving = "";
                if (poi.description != null)
                {
                    row.Omschrijving = poi.description.value;
                }

                qry = (from datacontract.poiValueGroup n in poi.categories
                       where n.type == "Type"
                       select n.value).ToList();
                if (qry.Count > 0) row.Type = qry[0];

                qry = (from datacontract.poiValueGroup n in poi.categories
                       where n.type == "Categorie"
                       select n.value).ToList();
                if (qry.Count > 0) row.Categorie = qry[0];
                //if (row.Categorie == null) row.Categorie = SelectedCategoriesListString;


                qry = (from datacontract.poiValueGroup n in poi.categories
                       where n.type == "Thema"
                       select n.value).ToList();
                if (qry.Count > 0) row.Thema = qry[0];

                qry = (
                    from datacontract.poiValueGroup n in poi.labels
                    select n.value).ToList();
                row.Label = string.Join(", ", qry.ToArray());

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

            var niscodes = (
                from n in municipalities.municipalities
                where n.municipalityName == muniName
                select n.municipalityCode);

            if (niscodes.Count() == 0) return "";

            return niscodes.First<string>();
        }

        private void zoomToQuery()
        {
            QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                var poly = GeometryEngine.Instance.Buffer(MapPointSelectedAddress, 50);
                mapView.ZoomTo(poly, new TimeSpan(0, 0, 0, 1));
            });
        }
        private string theme2code(string theme)
        {
            if (theme == null || theme == "") return "";

            var themeCodes = (from n in ThemeList
                              where n.value == theme
                              select n.term);
            if (themeCodes.Count() == 0) return "";

            return themeCodes.First<string>();
        }

        private string cat2code(string cat)
        {
            if (cat == null || cat == "") return "";

            var catCodes = (from n in poiDH.listCategories().categories
                            where n.value == cat
                            select n.term);
            if (catCodes.Count() == 0) return "";

            return catCodes.First<string>();
        }

        private string poitype2code(string poiType)
        {
            if (poiType == null || poiType == "") return "";

            var typeCodes = (from n in poiDH.listPOItypes().categories
                             where n.value == poiType
                             select n.term);
            if (typeCodes.Count() == 0) return "";

            return typeCodes.First<string>();
        }

        public ICommand CmdZoek
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    datacontract.poiMaxResponse poiData = null;

                    //input
                    string themeCode = theme2code(SelectedThemeListString);
                    string catCode = cat2code(SelectedCategoriesListString);
                    string poiTypeCode = poitype2code(SelectedTypesListString);
                    string keyWord = KeyWordString;
                    bool cluster = false;
                    string nis;

                    int count = 1000;
                    nis = municipality2nis(SelectedGemeenteList);

                    poiData = poiDH.getMaxmodel(keyWord, count, cluster, themeCode, catCode, poiTypeCode,
                    DataHandler.CRS.WGS84, null, nis, null);

                    List<datacontract.poiMaxModel> pois = poiData.pois;

                    TextVoegAlle = $@"Voeg alle ({pois.Count})";

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
                    DataRowSearchPlaats row = new DataRowSearchPlaats();

                    row.id = SelectedInteressantePlaatsList.id;
                    row.Thema = SelectedInteressantePlaatsList.Thema;
                    row.Categorie = SelectedInteressantePlaatsList.Categorie;
                    row.Type = SelectedInteressantePlaatsList.Type;
                    row.Label = SelectedInteressantePlaatsList.Label;
                    row.Omschrijving = SelectedInteressantePlaatsList.Omschrijving;
                    row.Straat = SelectedInteressantePlaatsList.Straat;
                    row.busnr = SelectedInteressantePlaatsList.busnr;
                    row.Gemeente = SelectedInteressantePlaatsList.Gemeente;
                    row.Postcode = SelectedInteressantePlaatsList.Postcode;
                    row.Huisnummer = SelectedInteressantePlaatsList.Huisnummer;

                    FavouriteInteressantePlaatsList.Add(row);
                });
            }
        }

        public ICommand CmdRemove
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    DataRowSearchPlaats plaatsToDelete = FavouriteInteressantePlaatsList.FirstOrDefault(m => m.id == SelectedFavouriteInteressantePlaatsList.id);
                    if (plaatsToDelete != null)
                    {
                        FavouriteInteressantePlaatsList.Remove(plaatsToDelete);
                    }
                });
            }
        }

        public ICommand CmdVoeg
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    foreach (datacontract.poiMaxModel poi in listPois)
                    {
                        DataRowSearchPlaats row = new DataRowSearchPlaats();
                        List<string> qry;
                        datacontract.poiAddress adres;

                        row.id = poi.id;
                        row.Omschrijving = "";
                        if (poi.description != null)
                        {
                            row.Omschrijving = poi.description.value;
                        }

                        qry = (from datacontract.poiValueGroup n in poi.categories
                               where n.type == "Type"
                               select n.value).ToList();
                        if (qry.Count > 0) row.Type = qry[0];

                        qry = (from datacontract.poiValueGroup n in poi.categories
                               where n.type == "Categorie"
                               select n.value).ToList();
                        if (qry.Count > 0) row.Categorie = qry[0];
                        //if (row.Categorie == null) row.Categorie = SelectedCategoriesListString;


                        qry = (from datacontract.poiValueGroup n in poi.categories
                               where n.type == "Thema"
                               select n.value).ToList();
                        if (qry.Count > 0) row.Thema = qry[0];

                        qry = (
                            from datacontract.poiValueGroup n in poi.labels
                            select n.value).ToList();
                        row.Label = string.Join(", ", qry.ToArray());

                        adres = poi.location.address;
                        if (adres != null)
                        {
                            row.Straat = adres.street;
                            row.Huisnummer = adres.streetnumber;
                            row.Postcode = adres.postalcode;
                            row.Gemeente = adres.municipality;
                        }


                        FavouriteInteressantePlaatsList.Add(row);
                    }
                });
            }
        }

        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    zoomToQuery();
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
