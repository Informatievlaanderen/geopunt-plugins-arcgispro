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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GeoPunt.Dockpanes
{
    internal class SearchPlaceViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPlace";

        public List<DataRowSearchPlaats> LoadCollectionData()
        {
            List<DataRowSearchPlaats> authors = new List<DataRowSearchPlaats>();

            authors.Add(new DataRowSearchPlaats()
            {
                id = 101,
                Theme = "test",
                Category = "test",
                Type = "test",
                Label = "test",
                Omschrijving = "test",
                Straat = "test",
                Huisnummer = "test",
                busnr = "test",
                Gemeente = "test",
                Postcode = "test",
            });

            authors.Add(new DataRowSearchPlaats()
            {
                id = 101,
                Theme = "test",
                Category = "test",
                Type = "test",
                Label = "test",
                Omschrijving = "test",
                Straat = "test",
                Huisnummer = "test",
                busnr = "test",
                Gemeente = "test",
                Postcode = "test",
            });

            return authors;
        }

        DataHandler.poi poiDH;
        datacontract.municipalityList municipalities;

        public SearchPlaceViewModel() 
        {
            poiDH = new DataHandler.poi(5000);
            initGui();
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
