using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core.Utilities;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json;


namespace GeoPunt.Dockpanes
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public AsyncObservableCollection()
        {
        }

        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        private void ExecuteOnSyncContext(Action action)
        {
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                action();
            }
            else
            {
                _synchronizationContext.Send(_ => action(), null);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            ExecuteOnSyncContext(() => base.InsertItem(index, item));
        }

        protected override void RemoveItem(int index)
        {
            ExecuteOnSyncContext(() => base.RemoveItem(index));
        }

        protected override void SetItem(int index, T item)
        {
            ExecuteOnSyncContext(() => base.SetItem(index, item));
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            ExecuteOnSyncContext(() => base.MoveItem(oldIndex, newIndex));
        }

        protected override void ClearItems()
        {
            ExecuteOnSyncContext(() => base.ClearItems());
        }
    }
    internal class SearchAddressDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchAddressDockpane";
        


        private ObservableCollection<string> _listCities = new ObservableCollection<string>(new List<string>() {
             "",
            "Aalst",
            "Aalter",
            "Aarschot",
            "Aartselaar",
            "Affligem",
            "Alken",
            "Alveringem",
            "Anderlecht",
            "Antwerpen",
            "Anzegem",
            "Ardooie",
            "Arendonk",
            "As",
            "Asse",
            "Assenede",
            "Avelgem",
            "Baarle-Hertog",
            "Balen",
            "Beernem",
            "Beerse",
            "Beersel",
            "Begijnendijk",
            "Bekkevoort",
            "Beringen",
            "Berlaar",
            "Berlare",
            "Bertem",
            "Bever",
            "Beveren",
            "Bierbeek",
            "Bilzen",
            "Blankenberge",
            "Bocholt",
            "Boechout",
            "Bonheiden",
            "Boom",
            "Boortmeerbeek",
            "Borgloon",
            "Bornem",
            "Borsbeek",
            "Boutersem",
            "Brakel",
            "Brasschaat",
            "Brecht",
            "Bredene",
            "Bree",
            "Brugge",
            "Brussel",
            "Buggenhout",
            "Damme",
            "De Haan",
            "De Panne",
            "De Pinte",
            "Deerlijk",
            "Deinze",
            "Denderleeuw",
            "Dendermonde",
            "Dentergem",
            "Dessel",
            "Destelbergen",
            "Diepenbeek",
            "Diest",
            "Diksmuide",
            "Dilbeek",
            "Dilsen-Stokkem",
            "Drogenbos",
            "Duffel",
            "Edegem",
            "Eeklo",
            "Elsene",
            "Erpe-Mere",
            "Essen",
            "Etterbeek",
            "Evere",
            "Evergem",
            "Galmaarden",
            "Ganshoren",
            "Gavere",
            "Geel",
            "Geetbets",
            "Genk",
            "Gent",
            "Geraardsbergen",
            "Gingelom",
            "Gistel",
            "Glabbeek",
            "Gooik",
            "Grimbergen",
            "Grobbendonk",
            "Haacht",
            "Haaltert",
            "Halen",
            "Halle",
            "Ham",
            "Hamme",
            "Hamont-Achel",
            "Harelbeke",
            "Hasselt",
            "Hechtel-Eksel",
            "Heers",
            "Heist-op-den-Berg",
            "Hemiksem",
            "Herent",
            "Herentals",
            "Herenthout",
            "Herk-de-Stad",
            "Herne",
            "Herselt",
            "Herstappe",
            "Herzele",
            "Heusden-Zolder",
            "Heuvelland",
            "Hoegaarden",
            "Hoeilaart",
            "Hoeselt",
            "Holsbeek",
            "Hooglede",
            "Hoogstraten",
            "Horebeke",
            "Houthalen-Helchteren",
            "Houthulst",
            "Hove",
            "Huldenberg",
            "Hulshout",
            "Ichtegem",
            "Ieper",
            "Ingelmunster",
            "Izegem",
            "Jabbeke",
            "Jette",
            "Kalmthout",
            "Kampenhout",
            "Kapellen",
            "Kapelle-op-den-Bos",
            "Kaprijke",
            "Kasterlee",
            "Keerbergen",
            "Kinrooi",
            "Kluisbergen",
            "Knesselare",
            "Knokke-Heist",
            "Koekelare",
            "Koekelberg",
            "Koksijde",
            "Kontich",
            "Kortemark",
            "Kortenaken",
            "Kortenberg",
            "Kortessem",
            "Kortrijk",
            "Kraainem",
            "Kruibeke",
            "Kruishoutem",
            "Kuurne",
            "Laakdal",
            "Laarne",
            "Lanaken",
            "Landen",
            "Langemark-Poelkapelle",
            "Lebbeke",
            "Lede",
            "Ledegem",
            "Lendelede",
            "Lennik",
            "Leopoldsburg",
            "Leuven",
            "Lichtervelde",
            "Liedekerke",
            "Lier",
            "Lierde",
            "Lille",
            "Linkebeek",
            "Lint",
            "Linter",
            "Lochristi",
            "Lokeren",
            "Lommel",
            "Londerzeel",
            "Lo-Reninge",
            "Lovendegem",
            "Lubbeek",
            "Lummen",
            "Maarkedal",
            "Maaseik",
            "Maasmechelen",
            "Machelen",
            "Maldegem",
            "Malle",
            "Mechelen",
            "Meerhout",
            "Meeuwen-Gruitrode",
            "Meise",
            "Melle",
            "Menen",
            "Merchtem",
            "Merelbeke",
            "Merksplas",
            "Mesen",
            "Meulebeke",
            "Middelkerke",
            "Moerbeke",
            "Mol",
            "Moorslede",
            "Mortsel",
            "Nazareth",
            "Neerpelt",
            "Nevele",
            "Niel",
            "Nieuwerkerken",
            "Nieuwpoort",
            "Nijlen",
            "Ninove",
            "Olen",
            "Oostende",
            "Oosterzele",
            "Oostkamp",
            "Oostrozebeke",
            "Opglabbeek",
            "Opwijk",
            "Oudenaarde",
            "Oudenburg",
            "Oudergem",
            "Oud-Heverlee",
            "Oud-Turnhout",
            "Overijse",
            "Overpelt",
            "Peer",
            "Pepingen",
            "Pittem",
            "Poperinge",
            "Putte",
            "Puurs",
            "Ranst",
            "Ravels",
            "Retie",
            "Riemst",
            "Rijkevorsel",
            "Roeselare",
            "Ronse",
            "Roosdaal",
            "Rotselaar",
            "Ruiselede",
            "Rumst",
            "Schaarbeek",
            "Schelle",
            "Scherpenheuvel-Zichem",
            "Schilde",
            "Schoten",
            "Sint-Agatha-Berchem",
            "Sint-Amands",
            "Sint-Genesius-Rode",
            "Sint-Gillis",
            "Sint-Gillis-Waas",
            "Sint-Jans-Molenbeek",
            "Sint-Joost-ten-Node",
            "Sint-Katelijne-Waver",
            "Sint-Lambrechts-Woluwe",
            "Sint-Laureins",
            "Sint-Lievens-Houtem",
            "Sint-Martens-Latem",
            "Sint-Niklaas",
            "Sint-Pieters-Leeuw",
            "Sint-Pieters-Woluwe",
            "Sint-Truiden",
            "Spiere-Helkijn",
            "Stabroek",
            "Staden",
            "Steenokkerzeel",
            "Stekene",
            "Temse",
            "Ternat",
            "Tervuren",
            "Tessenderlo",
            "Tielt",
            "Tielt-Winge",
            "Tienen",
            "Tongeren",
            "Torhout",
            "Tremelo",
            "Turnhout",
            "Ukkel",
            "Veurne",
            "Vilvoorde",
            "Vleteren",
            "Voeren",
            "Vorselaar",
            "Vorst",
            "Vosselaar",
            "Waarschoot",
            "Waasmunster",
            "Wachtebeke",
            "Waregem",
            "Watermaal-Bosvoorde",
            "Wellen",
            "Wemmel",
            "Wervik",
            "Westerlo",
            "Wetteren",
            "Wevelgem",
            "Wezembeek-Oppem",
            "Wichelen",
            "Wielsbeke",
            "Wijnegem",
            "Willebroek",
            "Wingene",
            "Wommelgem",
            "Wortegem-Petegem",
            "Wuustwezel",
            "Zandhoven",
            "Zaventem",
            "Zedelgem",
            "Zele",
            "Zelzate",
            "Zemst",
            "Zingem",
            "Zoersel",
            "Zomergem",
            "Zonhoven",
            "Zonnebeke",
            "Zottegem",
            "Zoutleeuw",
            "Zuienkerke",
            "Zulte",
            "Zutendaal",
            "Zwalm",
            "Zwevegem",
            "Zwijndrecht",
            "Oudsbergen",
            "Pelt",
            "Puurs-Sint-Amands",
            "Lievegem",
            "Kruisem"
        });
        public ObservableCollection<string> ListCities
        {
            get { return _listCities; }
            set  {
                SetProperty(ref _listCities, value);
                
            }
        }

        private ObservableCollection<string> _listStreets = new AsyncObservableCollection<string>();
        public ObservableCollection<string> ListStreets
        {
            get { return _listStreets; }
            set
            {
                SetProperty(ref _listStreets, value);

            }
        }

        private MapPoint _selectedMapPoint;


        private string _selectedStreet;
        public string SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                SetProperty(ref _selectedStreet, value);
                QueuedTask.Run(() =>
                {
                    //ListStreets.Clear();
                    var map = MapView.Active?.Map;
                    var layerCities = "GRB - TBLADPADR - huisnummer van een administratief perceel";
                    var searchLayerProvinces = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerCities, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (searchLayerProvinces == null)
                    {
                        MessageBox.Show($@"NOT FOUND : {layerCities}");
                        return;
                    }

                    QueryFilter queryFilter = new QueryFilter
                    {
                        //WhereClause = "POSTCODE = 9200"
                        WhereClause = $@"STRAATNM = '{_selectedStreet}'"
                        //WhereClause = "GEMEENTE = 'Meise'"
                    };

                    searchLayerProvinces.Select(queryFilter);

                    //Getting the first selected feature layer of the map view
                    var flyr = (FeatureLayer)MapView.Active.GetSelectedLayers()
                                      .OfType<FeatureLayer>().FirstOrDefault();
                    using (RowCursor rows = searchLayerProvinces.Search(queryFilter)) //execute
                    {
                        //Looping through to count
                        while (rows.MoveNext())
                        {
                            using (Row row = rows.Current)
                            {
                                

                                //long oid = rows.Current.GetObjectID();

                                //ArcGIS.Core.Data.Feature feature = rows.Current as ArcGIS.Core.Data.Feature;


                                //_selectedMapPoint = feature.GetShape() as MapPoint;

                                //MessageBox.Show($@"{_selectedMapPoint.X}  //  {_selectedMapPoint.Y}  :: {_selectedMapPoint.SpatialReference}");




                            }
                        }
                    }
                });

            }
        }

        List<string> suggestions = new List<string>() { "koko" };
        private void sugCallback(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                datacontract.crabSuggestion sug = JsonConvert.DeserializeObject<datacontract.crabSuggestion>(e.Result);
                suggestions = sug.SuggestionResult;
                MessageBox.Show($@"callback!!");
                //suggestionList.DataSource = suggestions;
                //infoLabel.Text = "";
            }
            else
            {
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message);
                }
            }
        }

        DataHandler.adresSuggestion adresSuggestion;
        private void updateSuggestions()
        {
            adresSuggestion = new DataHandler.adresSuggestion(sugCallback, 5000);
            //if (adresSuggestion.client.IsBusy) { return; }

            //string searchString = zoekText.Text + ", " + gemeenteBox.Text;
            //adresSuggestion.getAdresSuggestionAsync(searchString, 25);
            adresSuggestion.getAdresSuggestionAsync("wemmel", 80);
            MessageBox.Show($@"end {suggestions.Count}");
            foreach (var t in suggestions)
            {
                MessageBox.Show($@"{t}");
                Debug.WriteLine(t.ToString());
                Debug.WriteLine($@"!!! {t} !!!");
            }
            
        }


        private string _selectedCity;
        public string SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                SetProperty(ref _selectedCity, value);
                
                QueuedTask.Run(() =>
                {
                    ListStreets.Clear();

                    updateSuggestions();




                    //var map = MapView.Active?.Map;
                    //var layerCities = "GRB - TBLADPADR - huisnummer van een administratief perceel";
                    //var searchLayerProvinces = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(layerCities, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    //if (searchLayerProvinces == null )
                    //{
                    //    MessageBox.Show($@"NOT FOUND : {layerCities}");
                    //    return;
                    //}

                    //QueryFilter queryFilter = new QueryFilter
                    //{
                    //    //WhereClause = "POSTCODE = 9200"
                    //    WhereClause = $@"GEMEENTE = '{_selectedCity}'"
                    //    //WhereClause = "GEMEENTE = 'Meise'"
                    //};

                    //searchLayerProvinces.Select(queryFilter);

                    ////Getting the first selected feature layer of the map view
                    //var flyr = (FeatureLayer)MapView.Active.GetSelectedLayers()
                    //                  .OfType<FeatureLayer>().FirstOrDefault();
                    //using (RowCursor rows = searchLayerProvinces.Search(queryFilter)) //execute
                    //{
                    //    //Looping through to count
                    //    while (rows.MoveNext())
                    //    {
                    //        using (Row row = rows.Current)
                    //        {
                    //            string streetName = Convert.ToString(row["STRAATNM"]);
                    //            string houseNumber = Convert.ToString(row["HUISNR"]);
                    //            //ListStreets.Add(streetName + " " + houseNumber);
                    //            if (!ListStreets.Contains(streetName))
                    //            {
                    //                ListStreets.Add(streetName);
                    //            }

                    //            //long oid = rows.Current.GetObjectID();

                    //            //ArcGIS.Core.Data.Feature feature = rows.Current as ArcGIS.Core.Data.Feature;


                    //            //MapPoint mapPoint = feature.GetShape() as MapPoint;

                    //            //MessageBox.Show($@"{mapPoint.X}  //  {mapPoint.Y}  :: {mapPoint.SpatialReference}");




                    //        }  
                    //    }
                    //}

                });
            }
        }

        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await Geoprocessing.ExecuteToolAsync("SelectLayerByAttribute_management", new string[] {
                    //"GRB - TBLADPADR - huisnummer van een administratief perceel", "NEW_SELECTION", $@"OID = 1" });
                    "GRB - TBLADPADR - huisnummer van een administratief perceel", "NEW_SELECTION", $@"STRAATNM = '{_selectedStreet}'" + " AND " + $@"GEMEENTE = '{_selectedCity}'" });
                    await MapView.Active.ZoomToSelectedAsync(new TimeSpan(0, 0, 2), false);
                });
            }
        }

        public ICommand CmdPoint
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    MessageBox.Show($@"222) {_selectedMapPoint.X}  //  {_selectedMapPoint.Y}  :: {_selectedMapPoint.SpatialReference}");
                    MapPoint mapPoint = MapPointBuilderEx.CreateMapPoint(_selectedMapPoint.X, _selectedMapPoint.Y, _selectedMapPoint.SpatialReference);


                    //await QueuedTask.Run(() =>
                    //{
                    //    var pointSymbol = SymbolFactory.Instance.DefaultPointSymbol;
                    //    return pointSymbol;
                    //});

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
        private string _textFilterStreet;
        public string TextFilterStreet
        {
            get { return _textFilterStreet; }
            set
            {
                SetProperty(ref _textFilterStreet, value);
                //ListStreets = ListStreets.Where(s => s.Contains(value));
                //MessageBox.Show($@"{ListStreets.Count}");
            }
        }

        public CollectionViewSource CollViewSource { get; set; }

        private string _searchFilter;
        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                SetProperty(ref _searchFilter, value);
                if (!string.IsNullOrEmpty(_searchFilter))
                    AddFilter();

                CollViewSource.View.Refresh(); // important to refresh your View
            }
        }

        protected SearchAddressDockpaneViewModel() {
            CollViewSource = new CollectionViewSource();//onload of your VM class
            CollViewSource.Source = ListStreets;//after ini YourCollection
        }

        private void AddFilter()
        {
            CollViewSource.Filter -= new FilterEventHandler(Filter);
            CollViewSource.Filter += new FilterEventHandler(Filter);

        }

        private void Filter(object sender, FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as string;
            if (src == null)
                e.Accepted = false;
            else if (src != null && !src.ToUpper().Contains(SearchFilter.ToUpper()))// here is FirstName a Property in my YourCollectionItem
                e.Accepted = false;
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
	internal class SearchAddressDockpane_ShowButton : Button
	{
		protected async override void OnClick()
		{
			SearchAddressDockpaneViewModel.Show();

        }
  }	
}
