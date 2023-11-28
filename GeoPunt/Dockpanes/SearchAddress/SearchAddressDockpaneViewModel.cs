using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DataHandler;
using Newtonsoft.Json;
using GeoPunt.Helpers;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping.Events;

namespace GeoPunt.Dockpanes.SearchAddress
{
    internal class SearchAddressDockpaneViewModel : DockPane, IMarkedGraphicDisplayer
    {

        
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchAddress_SearchAddressDockpane";

        private adresSuggestion adresSuggestion;
        private adresLocation adresLocation;

        private Utils utils = new Utils();
        // public bool isRemoveMarkeer = false;
        private SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);

        protected SearchAddressDockpaneViewModel()
        {
            // IsSelectedFavouriteList = true;
            adresSuggestion = new adresSuggestion(sugCallback, 5000);
            adresLocation = new adresLocation(5000);
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




        // when pane is closed reset (Need to work)
        //protected override void OnShow(bool isVisible)
        //{

        //    if (!isVisible)
        //    {

        //        adresSuggestion = new adresSuggestion(sugCallback, 5000);
        //        adresLocation = new adresLocation(5000);
        //        SelectedCity = ListCities.FirstOrDefault();
        //        SearchStringCityPart = null;
        //        GraphicsList = new ObservableCollection<Graphic>();
        //        MarkedGraphicsList = new ObservableCollection<Graphic>();
        //        updateListBoxMarkeer();
        //    }
        //}

        #region City search
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
            set
            {
                SetProperty(ref _listCities, value);
            }
        }

        private string _selectedCity;
        public string SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                SetProperty(ref _selectedCity, value);

                SearchAddressSearcher.City = SelectedCity;
                QueuedTask.Run(() =>
                {

                    ListStreets.Clear();
                    updateSuggestions();
                });
            }
        }
        private string _searchStringCityPart;
        public string SearchStringCityPart
        {
            get { return _searchStringCityPart; }
            set
            {
                SetProperty(ref _searchStringCityPart, value);
            }
        }
        

        private List<string> _listStreets = new List<string>();
        public List<string> ListStreets
        {
            get { return _listStreets; }
            set
            {
                SetProperty(ref _listStreets, value);
            }
        }


       
        private string _selectedStreet;
        public string SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                SetProperty(ref _selectedStreet, value);
                SelectedStreetMapPoint = null;

                

                if (_selectedStreet != null)
                {
                    SelectedStreetIsSelected = true;
                    double x = 0;
                    double y = 0;

                    List<datacontract.locationResult> loc = adresLocation.getAdresLocation(_selectedStreet, 1);
                    foreach (datacontract.locationResult item in loc)
                    {
                        x = item.Location.X_Lambert72;
                        y = item.Location.Y_Lambert72;

                    }
                    SelectedStreetMapPoint = utils.CreateMapPoint(x, y, lambertSpatialReference);


                    // isRemoveMarkeer = false;
                    // IsSelectedFavouriteList = true;

                    SearchFilter = _selectedStreet.Split($@", ")[0];


                }
                else
                {
                    SelectedStreetIsSelected = false;
                }
            }
        }


       

        private bool _selectedStreetIsSelected = false;
        public bool SelectedStreetIsSelected
        {
            get { return _selectedStreetIsSelected; }
            set
            {
                SetProperty(ref _selectedStreetIsSelected, value);
            }
        }

        private MapPoint _selectedStreetMapPoint;
        public MapPoint SelectedStreetMapPoint
        {
            get { return _selectedStreetMapPoint; }
            set
            {
                SetProperty(ref _selectedStreetMapPoint, value);
                if (_selectedStreetMapPoint != null)
                {
                    SelectedStreetMapPointExist = true;
                }
                else
                {
                    SelectedStreetMapPointExist = false;
                }
            }
        }


        private bool _selectedStreetMapPointExist = false;
        public bool SelectedStreetMapPointExist
        {
            get { return _selectedStreetMapPointExist; }
            set
            {
                SetProperty(ref _selectedStreetMapPointExist, value);
            }
        }

        #endregion


        #region Favorite list

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

        private ObservableCollection<Graphic> _markedGraphicsList = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> MarkedGraphicsList
        {
            get { return _markedGraphicsList; }
            set
            {
                SetProperty(ref _markedGraphicsList, value);
            }
        }


        //private bool _isSelectedFavouriteList;
        // public bool IsSelectedFavouriteList
        // {
        // get { return _isSelectedFavouriteList; }
        // set
        // {
        // SetProperty(ref _isSelectedFavouriteList, value);
        // IsInverseSelectedFavouriteList = !_isSelectedFavouriteList;
        // }
        // }

        private string _textMarkeer = "Markeer";
        public string TextMarkeer
        {
            get { return _textMarkeer; }
            set
            {
                SetProperty(ref _textMarkeer, value);
            }
        }

        #endregion
        private void sugCallback(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                datacontract.crabSuggestion sug = JsonConvert.DeserializeObject<datacontract.crabSuggestion>(e.Result);
                ListStreets = sug.SuggestionResult;
            }
            else
            {
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message);
                }
            }
        }
        

       

        private void updateSuggestions()
        {
            adresSuggestion = new adresSuggestion(sugCallback, 5000);

            string searchString = @$"{SearchAddressSearcher.Address}, {SearchAddressSearcher.City}";
            adresSuggestion.getAdresSuggestionAsync(searchString, 80);

            //adresSuggestion = new DataHandler.adresSuggestion(sugCallback, 5000);

            //string searchString = SearchFilter + ", " + SelectedCity;
            //adresSuggestion.getAdresSuggestionAsync(searchString, 80);
        }

        public void updateListBoxMarkeer()
        {

            utils.UpdateMarking((from markedGraphic in MarkedGraphicsList select markedGraphic.Geometry).ToList());

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
        private SearchAddressSearcher _searchAddressSearcher = new SearchAddressSearcher();
        public SearchAddressSearcher SearchAddressSearcher
        {
            get { return _searchAddressSearcher; }
            set
            {
                SetProperty(ref _searchAddressSearcher, value);
                updateSuggestions();
            }
        }

        private string _searchFilter;
        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                SetProperty(ref _searchFilter, value);
                SearchAddressSearcher.Address = _searchFilter;
                updateSuggestions();
            }
        }

        #region Command

        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ZoomTo(SelectedStreetMapPoint);
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

        public ICommand CmdRemoveFavourite
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

                    if (!GraphicsList.Any(saveGraphic => saveGraphic.Attributes["adres"].ToString() == SelectedStreet))
                    {

                        GraphicsList.Add(new Graphic(new Dictionary<string, object>
                                {
                                    {"adres", SelectedStreet},
                                }, SelectedStreetMapPoint));
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

        #endregion



        

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            pane.Activate();
        }

    }

    internal class SearchAddressDockpane_ShowButton : Button
    {
        protected async override void OnClick()
        {
            SearchAddressDockpaneViewModel.Show();
        }
    }


}
