using ActiproSoftware.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using geopunt4Arcgis;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace GeoPunt.Dockpanes.Catalogus
{
    internal class CatalogusViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_Catalogus_Catalogus";
        private catalog clg;
        private datacontract.catalogResponse cataList = null;


        protected CatalogusViewModel()
        {

            initGui();

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



        public void initGui()
        {
            clg = new catalog(timeout: 8000);

            //ListKeyword = new ObservableCollection<string>(
            //    clg.getKeyWords()
            //    );

            ListGDIThema = new ObservableCollection<string>(
                clg.getGDIthemes()
                );

            ListGDIThema.Insert(0, "");


            List<string> organisationNames = clg.getOrganisations();
            organisationNames.Sort();

            ListOrganisationName = new ObservableCollection<string>(
                organisationNames
                );
            ListOrganisationName.Insert(0, "");



            Dictionary<string, string> dictionaryWithEmptyValue = new Dictionary<string, string> { { "None", "" } };

            ListType = new ObservableDictionary<string, string>(dictionaryWithEmptyValue.Concat(clg.dataTypes).ToDictionary(x => x.Key, x => x.Value));



            ListInspireThema = new ObservableCollection<string>(
                 clg.inspireKeywords()
                );
            ListInspireThema.Insert(0, "");


            StackPanelControl = new System.Windows.Controls.StackPanel();
        }


        private ObservableCollection<string> _listKeyword = new ObservableCollection<string>();
        public ObservableCollection<string> ListKeyword
        {
            get { return _listKeyword; }
            set
            {
                SetProperty(ref _listKeyword, value);
            }
        }

        private string _selectedKeyword;
        public string SelectedKeyword
        {
            get { return _selectedKeyword; }
            set
            {
                SetProperty(ref _selectedKeyword, value);
                if (_selectedKeyword != null && _selectedKeyword.Trim() != "")
                {
                    try
                    {
                        ListKeyword = new ObservableCollection<string>(
                        clg.getKeyWords(_selectedKeyword)
                        );
                    }
                    catch (Exception ex)
                    {
                        ListKeyword = new ObservableCollection<string>();
                        Debug.WriteLine(ex.ToString());
                        throw;
                    }

                }
            }
        }




        private ObservableCollection<string> _listGDIThema = new ObservableCollection<string>();
        public ObservableCollection<string> ListGDIThema
        {
            get { return _listGDIThema; }
            set
            {
                SetProperty(ref _listGDIThema, value);
            }
        }
        private string _selectedGDIThema;
        public string SelectedGDIThema
        {
            get { return _selectedGDIThema; }
            set
            {
                SetProperty(ref _selectedGDIThema, value);
            }
        }



        private ObservableCollection<string> _listOrganisationName = new ObservableCollection<string>();
        public ObservableCollection<string> ListOrganisationName
        {
            get { return _listOrganisationName; }
            set
            {
                SetProperty(ref _listOrganisationName, value);
            }
        }
        private string _selectedOrganisationName;
        public string SelectedOrganisationName
        {
            get { return _selectedOrganisationName; }
            set
            {
                SetProperty(ref _selectedOrganisationName, value);
            }
        }



        private ObservableDictionary<string, string> _listDataSource = new ObservableDictionary<string, string>();
        public ObservableDictionary<string, string> ListDataSource
        {
            get { return _listDataSource; }
            set
            {
                SetProperty(ref _listDataSource, value);
            }
        }
        private KeyValuePair<string, string> _selectedDataSource;
        public KeyValuePair<string, string> SelectedDataSource
        {
            get { return _selectedDataSource; }
            set
            {
                SetProperty(ref _selectedDataSource, value);
            }
        }



        private ObservableDictionary<string, string> _listType = new ObservableDictionary<string, string>();
        public ObservableDictionary<string, string> ListType
        {
            get { return _listType; }
            set
            {
                SetProperty(ref _listType, value);
            }
        }
        private KeyValuePair<string, string> _selectedType;
        public KeyValuePair<string, string> SelectedType
        {
            get { return _selectedType; }
            set
            {
                SetProperty(ref _selectedType, value);
            }
        }

        private ObservableCollection<string> _listInspireThema = new ObservableCollection<string>();
        public ObservableCollection<string> ListInspireThema
        {
            get { return _listInspireThema; }
            set
            {
                SetProperty(ref _listInspireThema, value);
            }
        }
        private string _selectedInspireThema;
        public string SelectedInspireThema
        {
            get { return _selectedInspireThema; }
            set
            {
                SetProperty(ref _selectedInspireThema, value);
            }
        }


        private ObservableCollection<string> _listResultSearch = new ObservableCollection<string>();
        public ObservableCollection<string> ListResultSearch
        {
            get { return _listResultSearch; }
            set
            {
                SetProperty(ref _listResultSearch, value);
            }
        }
        private string _selectedResultSearch;
        public string SelectedResultSearch
        {
            get { return _selectedResultSearch; }
            set
            {
                SetProperty(ref _selectedResultSearch, value);

                ButtonWMSIsEnable = false;
                ButtonNavigateResultIsEnabled = false;
                LinkFiche = "";

                if (_selectedResultSearch != null)
                {
                    StackPanelControl.Children.Clear();
                    string selVal = SelectedResultSearch;
                    datacontract.catalogRecord cataObj = (from n in cataList.catalogRecords
                                                          where n.Title == _selectedResultSearch
                                                          select n).ToList().FirstOrDefault();
                    if (cataObj != null)
                    {
                        catalogRecordInfo catalogrecordInfo = clg.searchCatalogRecordInfo(cataObj.ID);


                        updateInfo(catalogrecordInfo);
                    }




                }
            }
        }



        private System.Windows.Controls.StackPanel _stackPanelControl;
        public System.Windows.Controls.StackPanel StackPanelControl
        {
            get { return _stackPanelControl; }
            set
            {
                SetProperty(ref _stackPanelControl, value);
            }
        }



        private bool _buttonNavigateResultIsEnabled = false;
        public bool ButtonNavigateResultIsEnabled
        {
            get { return _buttonNavigateResultIsEnabled; }
            set
            {
                SetProperty(ref _buttonNavigateResultIsEnabled, value);
            }
        }


        private bool _searchIsNotBusy = true;
        public bool SearchIsNotBusy
        {
            get { return _searchIsNotBusy; }
            set
            {
                SetProperty(ref _searchIsNotBusy, value);
            }
        }




        private string _linkFiche = "";

        public string LinkFiche
        {
            get { return _linkFiche; }
            set
            {
                SetProperty(ref _linkFiche, value);
                ButtonNavigateResultIsEnabled = false;

                if (_linkFiche != null && _linkFiche != "")
                {
                    ButtonNavigateResultIsEnabled = true;
                }
            }
        }

        private bool _buttonWMSIsEnable = false;
        public bool ButtonWMSIsEnable
        {
            get { return _buttonWMSIsEnable; }
            set
            {
                SetProperty(ref _buttonWMSIsEnable, value);
            }
        }






        private string _textStatus;
        public string TextStatus
        {
            get { return _textStatus; }
            set
            {
                SetProperty(ref _textStatus, value);
            }
        }



        public ICommand CmdSearch
        {
            get
            {
                return new RelayCommand(() => { search(); });
            }
        }


        public ICommand CmdAddWms
        {
            get
            {
                return new RelayCommand(() => { addWMS(); });
            }
        }


        private void search()
        {
            ListResultSearch = new ObservableCollection<string>();
            TextStatus = "";

            StackPanelControl.Children.Clear();
            SearchIsNotBusy = false;

            try
            {


                string selectedType = SelectedType.Key == "None" || SelectedType.Key == null ? "" : SelectedType.Value;

                // cataList = clg.searchAll(SelectedKeyword, SelectedGDIThema, SelectedOrganisationName, selectedType, "", SelectedInspireThema);
                cataList = clg.search(SelectedKeyword, 0, 100, SelectedGDIThema, SelectedOrganisationName, selectedType, "", SelectedInspireThema);

                if (cataList.TotalItems != 0)
                {

                    List<string> catalogTitleList = (from g in cataList.catalogRecords
                                                     select g.Title).ToList<string>();
                    if (catalogTitleList != null)
                    {
                        catalogTitleList.RemoveAll(e => e == null);
                        ListResultSearch = new ObservableCollection<string>(catalogTitleList);
                    }

                    TextStatus = string.Format("Aantal records gevonden: {0}", cataList.TotalItems);
                }
                else
                {
                    MessageBox.Show("Er werd niets gevonden dat voldoet aan deze criteria", "Geen resultaat");
                }
                // addWMSbtn.Enabled = false;
                // OpenArcgisBtn.Enabled = false;
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
                MessageBox.Show(ex.Message + " : " + ex.StackTrace, "Error");
            }
            finally
            {
                SearchIsNotBusy = true;
            }
        }

        private void updateInfo(catalogRecordInfo catalogrecordInfo)
        {

            StackPanelControl.Children.Clear();


            System.Windows.Controls.TextBlock titleTextBlock = new System.Windows.Controls.TextBlock();
            titleTextBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
            titleTextBlock.Margin = new System.Windows.Thickness(2);
            StackPanelControl.Children.Add(titleTextBlock);
            titleTextBlock.Inlines.Add(new System.Windows.Documents.Run(catalogrecordInfo.CatalogRecordExtra.Title) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 24 });


            System.Windows.Controls.TextBlock descriptionTextBlock = new System.Windows.Controls.TextBlock();
            descriptionTextBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
            descriptionTextBlock.Margin = new System.Windows.Thickness(2);
            StackPanelControl.Children.Add(descriptionTextBlock);
            descriptionTextBlock.Inlines.Add(new System.Windows.Documents.Run(catalogrecordInfo.CatalogRecordExtra.Description));


            ButtonNavigateResultIsEnabled = true;
            LinkFiche = catalogrecordInfo.Url;


            System.Windows.Controls.StackPanel linkContainer = new System.Windows.Controls.StackPanel();
            StackPanelControl.Children.Add(linkContainer);
            if (catalogrecordInfo.CatalogRecordExtra.Distributions != null && catalogrecordInfo.CatalogRecordExtra.Distributions.Count() > 0)
            {

                foreach (catalogDistribution distribution in catalogrecordInfo.CatalogRecordExtra.Distributions)
                {

                    if (distribution.AccessUrl != null)
                    {
                        string url = distribution.AccessUrl;
                        string name = distribution.Title;

                        //bulleted[i] = "\u2022" + unbulleted[i];
                        System.Windows.Documents.Hyperlink hyperlink = new System.Windows.Documents.Hyperlink();
                        string linkName = $@"   •   {name}/{distribution.Description}";
                        hyperlink.Inlines.Add(linkName);
                        hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                        hyperlink.NavigateUri = new Uri(url);

                        System.Windows.Controls.TextBlock hyperlinkTextBlock = new System.Windows.Controls.TextBlock();
                        hyperlinkTextBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
                        hyperlinkTextBlock.Margin = new System.Windows.Thickness(2, 0, 2, 0);
                        hyperlinkTextBlock.Inlines.Add(hyperlink);


                        linkContainer.Children.Add(hyperlinkTextBlock);
                    }
                }
            }


        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }


        private void addWMS()
        {
            //string selVal = SelectedResultSearch;

            //string lyrName; string wmsUrl;

            //bool hasWms = cataList.geturl(selVal, "OGC:WMS", out wmsUrl, out lyrName);
            //if (hasWms)
            //{
            //    wmsUrl = wmsUrl.Split('?')[0] + "?";

            //    if (geopuntHelper.websiteExists(wmsUrl, true) == false)
            //    {
            //        MessageBox.Show("Kan geen connectie maken met de Service.", "Connection timed out");
            //        return;
            //    }
            //    try
            //    {

            //        if (MapView.Active == null)
            //        {
            //            MessageBox.Show("No map view active.");
            //            return;
            //        }

            //        // TODO change below part to make able to select which layer to add

            //        var serverConnection = new CIMInternetServerConnection { URL = wmsUrl };
            //        var connection = new CIMWMSServiceConnection { ServerConnection = serverConnection };

            //        // Add a new layer to the map
            //        var layerParams = new LayerCreationParams(connection);


            //        QueuedTask.Run(() =>
            //        {

            //            try
            //            {
            //                LayerFactory.Instance.CreateLayer<WMSLayer>(layerParams, MapView.Active.Map);
            //            }
            //            catch (Exception ex)
            //            {
            //                MessageBox.Show(ex.Message, $@"Error trying to add layer");
            //            }
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message + " : " + ex.StackTrace, "Error");
            //    }
            //}
        }




        //private List<string> filterDL()
        //{
        //    if (cataList == null) return null;
        //    return (from g in cataList.catalogRecords
        //            where cataList.geturl(g.Title, "DOWNLOAD")
        //            select g.Title).ToList<string>();
        //}

        //private List<string> filterWMS()
        //{
        //    if (cataList == null) return null;
        //    return (from g in cataList.catalogRecords
        //            where cataList.geturl(g.Title, "OGC:WMS")
        //            select g.Title).ToList<string>();
        //}

        //private List<string> filterAGS()
        //{
        //    if (cataList == null) return null;
        //    return (from g in cataList.catalogRecords
        //            where cataList.geturl(g.Title, "Esri Rest API", 0)
        //            select g.Title).ToList<string>();
        //}

        private List<string> geenFilter()
        {
            if (cataList == null) return null;
            return (from g in cataList.catalogRecords
                    select g.Title).ToList<string>();
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
            get => _heading;
            set => SetProperty(ref _heading, value);
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Catalogus_ShowButton : Button
    {
        protected override void OnClick()
        {
            CatalogusViewModel.Show();
        }
    }
}
