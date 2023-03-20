using System;
using System.Collections;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DataHandler;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

using System.Data;

namespace GeoPunt.Dockpanes
{
    internal class SearchAddressDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchAddressDockpane";

        DataHandler.adresSuggestion adresSuggestion;
        DataHandler.adresLocation adresLocation;

        public bool isRemoveMarkeer = false;

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

        private bool _isInverseSelectedFavouriteList;
        public bool IsInverseSelectedFavouriteList
        {
            get { return _isInverseSelectedFavouriteList; }
            set
            {
                SetProperty(ref _isInverseSelectedFavouriteList, value);
            }
        }


        private bool _isSelectedFavouriteList;
        public bool IsSelectedFavouriteList
        {
            get { return _isSelectedFavouriteList; }
            set
            {
                SetProperty(ref _isSelectedFavouriteList, value);
                IsInverseSelectedFavouriteList = !_isSelectedFavouriteList;
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

        private List<string> _listStreets = new List<string>();
        public List<string> ListStreets
        {
            get { return _listStreets; }
            set
            {
               SetProperty(ref _listStreets, value);
            }
        }

        private List<MapPoint> _listStreetsMarkeer = new List<MapPoint>();
        public List<MapPoint> ListStreetsMarkeer
        {
            get { return _listStreetsMarkeer; }
            set
            {
                SetProperty(ref _listStreetsMarkeer, value);
            }
        }

        private ObservableCollection<SaveMapPoint> ListSaveMapPoint = new ObservableCollection<SaveMapPoint>();


        private ObservableCollection<MapPoint> _listStreetsFavourite = new ObservableCollection<MapPoint>();
        public ObservableCollection<MapPoint> ListStreetsFavourite
        {
            get { return _listStreetsFavourite; }
            set
            {
                SetProperty(ref _listStreetsFavourite, value);
            }
        }

        private ObservableCollection<string> _listStreetsFavouriteString = new ObservableCollection<string>();
        public ObservableCollection<string> ListStreetsFavouriteString
        {
            get { return _listStreetsFavouriteString; }
            set
            {
                SetProperty(ref _listStreetsFavouriteString, value); 
            }
        }

        private ObservableCollection<string> _listStreetsMarkeerString = new ObservableCollection<string>();
        public ObservableCollection<string> ListStreetsMarkeerString
        {
            get { return _listStreetsMarkeerString; }
            set
            {
                SetProperty(ref _listStreetsMarkeerString, value);
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
                updateCurrentMapPoint(_selectedStreet, 1);
                isRemoveMarkeer = false;

                IsSelectedFavouriteList = true;
            }
        }

        private string _selectedStreetFavourite;
        public string SelectedStreetFavourite
        {
            get { return _selectedStreetFavourite; }
            set
            {
                SetProperty(ref _selectedStreetFavourite, value);
                updateCurrentMapPoint(_selectedStreetFavourite, 1);
                isRemoveMarkeer = false;
                IsSelectedFavouriteList = false;
            }
        }

        private string _selectedStreetMarkeer;
        public string SelectedStreetMarkeer
        {
            get { return _selectedStreetMarkeer; }
            set
            {
                SetProperty(ref _selectedStreetMarkeer, value);
                updateCurrentMapPoint(_selectedStreetMarkeer, 1);
                isRemoveMarkeer = true;
                IsSelectedFavouriteList = false;
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

        MapPoint MapPointSelectedAddress = null;

        public void updateCurrentMapPoint(string query, int count)
        {            
            double x = 0;
            double y = 0;

            List<datacontract.locationResult> loc = adresLocation.getAdresLocation(query, count);
            foreach (datacontract.locationResult item in loc)
            {
                x = item.Location.X_Lambert72;
                y = item.Location.Y_Lambert72;
                
            }
            MapPointSelectedAddress = MapPointBuilderEx.CreateMapPoint(x, y);

            if (ListStreetsMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y) != null)
            {
                TextMarkeer = "Verwijder markering";
            }
            else
            {
                TextMarkeer = "Markeer";
            }
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

        private void updateSuggestions()
        {
            adresSuggestion = new DataHandler.adresSuggestion(sugCallback, 5000);
            adresLocation = new DataHandler.adresLocation(5000);
            string searchString = SearchFilter + ", " + SelectedCity;
            adresSuggestion.getAdresSuggestionAsync(searchString, 80);
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

        public ICommand CmdRemoveFavourite
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    //if (!isRemoveMarkeer)
                    //{
                    //    MapPoint pointToDelete = ListStreetsFavourite.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                    //    ListStreetsFavouriteString.Remove(SelectedStreetFavourite);
                    //    ListStreetsFavourite.Remove(pointToDelete);
                    //    GeocodeUtils.UpdateMapOverlay(pointToDelete, MapView.Active, true, true);
                        
                    //}else
                    //{
                    //    MapPoint pointToDelete = ListStreetsMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                    //    ListStreetsMarkeerString.Remove(SelectedStreetMarkeer);
                    //    ListStreetsMarkeer.Remove(pointToDelete);
                    //    GeocodeUtils.UpdateMapOverlayMarkeer(pointToDelete, MapView.Active, true, true);
                    //}

                    MapPoint pointToDelete = ListStreetsMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                    ListStreetsFavouriteString.Remove(SelectedStreetFavourite);
                    ListStreetsMarkeer.Remove(pointToDelete);
                    GeocodeUtils.UpdateMapOverlayMarkeer(pointToDelete, MapView.Active, true, true);

                    //updateListBoxFavourite();
                    updateListBoxMarkeer();
                });
            }
        }

        public void updateListBoxMarkeer()
        {
            foreach(MapPoint mapPoint in ListStreetsMarkeer)
            {
                GeocodeUtils.UpdateMapOverlayMarkeer(mapPoint, MapView.Active, false);
            }
        }

        public void updateListBoxFavourite()
        {
            foreach (MapPoint mapPoint in ListStreetsFavourite)
            {
                GeocodeUtils.UpdateMapOverlay(mapPoint, MapView.Active, true);
            }
        }

        

        public ICommand CmdPoint
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (ListStreetsMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y) == null)
                    {
                        ListStreetsMarkeer.Add(MapPointSelectedAddress);
                        updateListBoxMarkeer();
                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {
                        TextMarkeer = "Markeer";
                        MapPoint pointToDelete = ListStreetsMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                        ListStreetsMarkeer.Remove(pointToDelete);
                        GeocodeUtils.UpdateMapOverlayMarkeer(pointToDelete, MapView.Active, true, true);
                        updateListBoxMarkeer();
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
                    if (!ListStreetsFavouriteString.Contains(SelectedStreet))
                    {
                        ListStreetsFavouriteString.Add(SelectedStreet);
                        ListStreetsFavourite.Add(MapPointSelectedAddress);
                        ListSaveMapPoint.Add(new SaveMapPoint(SelectedStreet, MapPointSelectedAddress));
                        //updateListBoxFavourite();
                    }  
                });
            }
        }

        

        public ICommand CmdSaveIcon
        {
            get
            {
            return new RelayCommand(async () => 
                {
                    List<SaveMapPoint> _data = new List<SaveMapPoint>();
                    foreach (SaveMapPoint item in ListSaveMapPoint)
                    {
                        _data.Add(item);
                    }

                    using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                        if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            await using FileStream createStream = File.Create($@"{fbd.SelectedPath}\path.json");
                            await System.Text.Json.JsonSerializer.SerializeAsync(createStream, _data);
                        }
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
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    pane.Hide();
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
        


        protected SearchAddressDockpaneViewModel() 
        {
            IsSelectedFavouriteList = true;
            TextMarkeer = "Markeer";
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
