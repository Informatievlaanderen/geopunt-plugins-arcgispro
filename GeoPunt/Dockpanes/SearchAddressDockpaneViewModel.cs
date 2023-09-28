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
using System.IO;
using GeoPunt.Helpers;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using System.Collections;

namespace GeoPunt.Dockpanes
{
    internal class SearchAddressDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchAddressDockpane";

        private adresSuggestion adresSuggestion;
        private adresLocation adresLocation;

        private Utils utils = new Utils();
        // public bool isRemoveMarkeer = false;
        private SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);

        protected SearchAddressDockpaneViewModel()
        {
            // IsSelectedFavouriteList = true;
            TextMarkeer = "Markeer";
            adresSuggestion = new adresSuggestion(sugCallback, 5000);
            adresLocation = new adresLocation(5000);
        }


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
                QueuedTask.Run(() =>
                {

                    ListStreets.Clear();
                    updateSuggestions();
                    SearchFilter = null;
                });
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

        private string _textMarkeer;
        public string TextMarkeer
        {
            get { return _textMarkeer; }
            set
            {
                SetProperty(ref _textMarkeer, value);
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
                //updateCurrentMapPoint(_selectedStreet, 1);


                MapPointSelectedAddressSimple = null;

                if (_selectedStreet != null)
                {
                    double x = 0;
                    double y = 0;

                    List<datacontract.locationResult> loc = adresLocation.getAdresLocation(_selectedStreet, 1);
                    foreach (datacontract.locationResult item in loc)
                    {
                        x = item.Location.X_Lambert72;
                        y = item.Location.Y_Lambert72;

                    }
                    MapPointSelectedAddressSimple = utils.CreateMapPoint(x, y, lambertSpatialReference);

                    // isRemoveMarkeer = false;
                    // IsSelectedFavouriteList = true;

                    SearchFilter = SelectedStreet;

                }
            }
        }


        private ObservableCollection<Graphic> _listSaveGraphic = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> ListSaveGraphic
        {
            get { return _listSaveGraphic; }
            set
            {
                SetProperty(ref _listSaveGraphic, value);
            }
        }


        private Graphic _selectedSaveGraphic;
        public Graphic SelectedSaveGraphic
        {

            get { return _selectedSaveGraphic; }
            set
            {
                SetProperty(ref _selectedSaveGraphic, value);
                if (_selectedSaveGraphic != null)
                {
                    if (ListSaveGraphicMarked.Any(saveGraphic => saveGraphic.Attributes["adres"].ToString() == _selectedSaveGraphic.Attributes["adres"].ToString()))
                    {

                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {
                        TextMarkeer = "Markeer";
                    }
                }
                else
                {
                    TextMarkeer = "Markeer";
                }
                // isRemoveMarkeer = false;
                // IsSelectedFavouriteList = false;
            }
        }

        private ObservableCollection<Graphic> _listSaveGraphicMarked = new ObservableCollection<Graphic>();
        public ObservableCollection<Graphic> ListSaveGraphicMarked
        {
            get { return _listSaveGraphicMarked; }
            set
            {
                SetProperty(ref _listSaveGraphicMarked, value);
            }
        }


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


        MapPoint MapPointSelectedAddressSimple = null;

        public void updateCurrentMapPoint(string query, int count)
        {

        }

        private void updateSuggestions()
        {
            adresSuggestion = new adresSuggestion(sugCallback, 5000);
            string searchString = SearchFilter != null ? SearchFilter : SelectedCity;
            adresSuggestion.getAdresSuggestionAsync(searchString, 80);
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
                    utils.ZoomTo(MapPointSelectedAddressSimple);
                });
            }
        }

        public ICommand CmdRemoveFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    Graphic graphic = ListSaveGraphic.Where(saveGraphic => saveGraphic.Attributes["adres"] == SelectedSaveGraphic.Attributes["adres"]).First();
                    Graphic graphicMarked = ListSaveGraphicMarked.Where(saveGraphicMarked => saveGraphicMarked.Attributes["adres"] == SelectedSaveGraphic.Attributes["adres"]).First();

                    ListSaveGraphic.Remove(graphic);
                    ListSaveGraphicMarked.Remove(graphicMarked);
                    GeocodeUtils.UpdateMapOverlayMarkeer(graphicMarked.Geometry as MapPoint, MapView.Active, true, true);

                    updateListBoxMarkeer();
                });
            }
        }

        public void updateListBoxMarkeer()
        {
            foreach (Graphic saveGraphicMarked in ListSaveGraphicMarked)
            {
                GeocodeUtils.UpdateMapOverlayMarkeer(saveGraphicMarked.Geometry as MapPoint, MapView.Active, false);
            }
        }



        public ICommand CmdMark
        {
            get
            {
                return new RelayCommand(async () =>
                {

                    if (!ListSaveGraphicMarked.Any(saveGraphic => saveGraphic.Attributes["adres"] == SelectedSaveGraphic.Attributes["adres"]))
                    {
                        ListSaveGraphicMarked.Add(SelectedSaveGraphic);
                        updateListBoxMarkeer();
                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {

                        Graphic pointToDelete = ListSaveGraphicMarked.Where(saveGraphic => saveGraphic.Attributes["adres"] == SelectedSaveGraphic.Attributes["adres"]).First();
                        ListSaveGraphicMarked.Remove(pointToDelete);
                        GeocodeUtils.UpdateMapOverlayMarkeer(pointToDelete.Geometry as MapPoint, MapView.Active, true, true);
                        updateListBoxMarkeer();
                        TextMarkeer = "Markeer";
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



                    if (!ListSaveGraphic.Any(saveGraphic => saveGraphic.Attributes["adres"].ToString() == SelectedStreet))
                    {

                        ListSaveGraphic.Add(new Graphic(new Dictionary<string, object>
                                {
                                    {"adres", SelectedStreet},
                                }, MapPointSelectedAddressSimple));
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
                    utils.ExportToGeoJson(ListSaveGraphic.ToList());
                });
            }
        }





        private string _searchFilter;
        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                SetProperty(ref _searchFilter, value);
                updateSuggestions();
            }
        }




        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
            pane.Activate();
        }

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

    internal class SearchAddressDockpane_ShowButton : Button
    {
        protected async override void OnClick()
        {
            SearchAddressDockpaneViewModel.Show();
        }
    }
}
