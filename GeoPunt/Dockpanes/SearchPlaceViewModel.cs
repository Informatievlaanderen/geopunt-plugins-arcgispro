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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace GeoPunt.Dockpanes
{
    internal class SearchPlaceViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPlace";

        //List<DataRowSearchPlaats> authors = new List<DataRowSearchPlaats>();

        private ObservableCollection<DataRowSearchPlaats> _interessantePlaatsList = new ObservableCollection<DataRowSearchPlaats>();
        public ObservableCollection<DataRowSearchPlaats> InteressantePlaatsList
        {
            get { return _interessantePlaatsList; }
            set
            {
                SetProperty(ref _interessantePlaatsList, value);
                MessageBox.Show("add plaats");
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
                string var = _selectedInteressantePlaatsList.Straat + ", " + _selectedInteressantePlaatsList.Gemeente;
                MessageBox.Show($@"selected ip: {var}");
                updateCurrentMapPoint(var,1);
                
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
                MessageBox.Show("categories > 0");
                CategoriesListString = (from n in CategoriesList orderby n.value select n.value).ToList<string>();
                CategoriesListString.Insert(0, "");
            }

            if(TypesList.Count > 0) 
            {
                MessageBox.Show("types > 0");
                TypesListString = (from n in TypesList orderby n.value select n.value).ToList<string>();
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
                //MessageBox.Show($@"Selected: {_selectedThemeListString} || count: {CategoriesList.Count}");
            }
        }

        private String _selectedCategoriesListString;
        public String SelectedCategoriesListString
        {
            get { return _selectedCategoriesListString; }
            set
            {
                SetProperty(ref _selectedCategoriesListString, value);
                TypesList = poiDH.listPOItypes(_selectedCategoriesListString).categories;
                TypesListString = (from n in TypesList orderby n.value select n.value).ToList<string>();
                TypesListString.Insert(0, "");
                //MessageBox.Show($@"Selected: {_selectedThemeListString} || count: {CategoriesList.Count}");
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

        private List<datacontract.poiValueGroup> _typesList = new List<datacontract.poiValueGroup>();
        public List<datacontract.poiValueGroup> TypesList
        {
            get { return _typesList; }
            set
            {
                SetProperty(ref _typesList, value);
            }
        }

        private void updateDataGrid(List<datacontract.poiMaxModel> pois)
        {
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

        private void zoomToQuery()
        {
            QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                var poly = GeometryEngine.Instance.Buffer(MapPointSelectedAddress, 50);
                mapView.ZoomTo(poly, new TimeSpan(0, 0, 0, 3));
            });
        }

        public ICommand CmdZoek
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    datacontract.poiMaxResponse poiData = null;

                    //input
                    string themeCode = SelectedThemeListString;
                    string catCode = SelectedCategoriesListString;
                    string poiTypeCode = "";
                    string keyWord = "";
                    bool cluster = false;

                    //boundingBox extent;
                    //if (extentCkb.Checked)
                    //{
                    //    IEnvelope env = view.Extent;
                    //    IEnvelope prjEnv = geopuntHelper.Transform((IGeometry)env, wgs).Envelope;
                    //    extent = new boundingBox(prjEnv);
                    //    nis = null;
                    //}
                    //else
                    //{
                    //    nis = municipality2nis(gemeenteCbx.Text);
                    //    extent = null;
                    //}
                    int count = 132;

                    poiData = poiDH.getMaxmodel(keyWord, count, cluster, themeCode, catCode, poiTypeCode,
                    DataHandler.CRS.WGS84, null, null, null);

                    List<datacontract.poiMaxModel> pois = poiData.pois;

                    MessageBox.Show($@"count pois: {pois.Count}");

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
                    
                });
            }
        }

        public ICommand CmdVoeg
        {
            get
            {
                return new RelayCommand(async () =>
                {

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
