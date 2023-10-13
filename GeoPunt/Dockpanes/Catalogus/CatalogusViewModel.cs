﻿using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using ArcGIS.Desktop.Mapping;
using GeoPunt.DataHandler;
using geopunt4Arcgis;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
        private datacontract.metadataResponse metaList = null;
        

        protected CatalogusViewModel()
        {

            initGui();
        }

        public void initGui()
        {
            clg = new catalog(timeout: 8000);

            ListKeyword = new ObservableCollection<string>(
                clg.getKeyWords()
                );

            ListGDIThema = new ObservableCollection<string>(
                clg.getGDIthemes()
                );

            ListGDIThema.Insert(0, "");

            ListOrganisationName = new ObservableCollection<string>(
                clg.getOrganisations()
                );
            ListOrganisationName.Insert(0, "");



            Dictionary<string, string> dictionaryWithEmptyValue = new Dictionary<string, string> { { "None", "" } };

            ListDataSource = new ObservableDictionary<string, string>(dictionaryWithEmptyValue.Concat(clg.getSources()).ToDictionary(x => x.Key, x => x.Value));
            ListType = new ObservableDictionary<string, string>(dictionaryWithEmptyValue.Concat(clg.dataTypes).ToDictionary(x => x.Key, x => x.Value));



            ListInspireThema = new ObservableCollection<string>(
                 clg.inspireKeywords()
                );
            ListInspireThema.Insert(0, "");



            ListFilter = new ObservableCollection<string>(new List<string>()
            { "Alles weergeven",
            "WMS",
            "Download",
            "Arcgis service"});

            SelectedFilter = ListFilter.First();


            WebBrowserControl = new System.Windows.Controls.WebBrowser();
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

                ButtonDownloadIsEnable = false;
                ButtonWMSIsEnable = false;

                if (_selectedResultSearch != null)
                {
                    if (metaList.geturl(_selectedResultSearch, "Esri Rest API", 0)) ButtonDownloadIsEnable = true;
                    if (metaList.geturl(_selectedResultSearch, "OGC:WMS")) ButtonWMSIsEnable = true;

                    updateInfo();
                }
            }
        }


        private System.Windows.Controls.WebBrowser _webBrowserControl;
        public System.Windows.Controls.WebBrowser WebBrowserControl
        {
            get { return _webBrowserControl; }
            set
            {
                SetProperty(ref _webBrowserControl, value);
            }
        }



  

        


        private ObservableCollection<string> _listFilter = new ObservableCollection<string>();
        public ObservableCollection<string> ListFilter
        {
            get { return _listFilter; }
            set
            {
                SetProperty(ref _listFilter, value);
            }
        }
        private string _selectedFilter;
        public string SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                SetProperty(ref _selectedFilter, value);
                updateFilter();
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


        private bool _buttonDownloadIsEnable = false;
        public bool ButtonDownloadIsEnable
        {
            get { return _buttonDownloadIsEnable; }
            set
            {
                SetProperty(ref _buttonDownloadIsEnable, value);
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

        public ICommand CmdDownload
        {
            get
            {
                return new RelayCommand(() => { loadArcgis(); });
            }
        }


        private void search()
        {
            ListResultSearch = new ObservableCollection<string>();
            TextStatus = "";
            WebBrowserControl.Navigate("about:blank");

            try
            {

                string selectedType = SelectedType.Key == "None" ? "" : SelectedType.Key;
                // string selectedDataSource = SelectedDataSource.Key == "None" ? "" : SelectedDataSource.Key;


                metaList = clg.searchAll(SelectedKeyword, SelectedGDIThema, SelectedOrganisationName, selectedType, "", SelectedInspireThema);

                if (metaList.to != 0)
                {
                    updateFilter();
                    TextStatus = string.Format("Aantal records gevonden: {0}", metaList.summary.count);
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
        }


        private void updateFilter()
        {
            ListResultSearch = new ObservableCollection<string>();
            List<string> terms;
            switch (SelectedFilter)
            {
                case "Alles weergeven":
                    terms = geenFilter();
                    if (terms != null)
                    {
                        terms.RemoveAll(e => e == null);
                        ListResultSearch = new ObservableCollection<string>(terms);
                    }
                    break;
                case "WMS":
                    terms = filterWMS();
                    if (terms != null)
                    {
                        terms.RemoveAll(e => e == null);
                        ListResultSearch = new ObservableCollection<string>(terms);
                    }
                    break;
                case "Arcgis service":
                    terms = filterAGS();
                    if (terms != null)
                    {
                        terms.RemoveAll(e => e == null);
                        ListResultSearch = new ObservableCollection<string>(terms);
                    }
                    break;
                case "Download":
                    terms = filterDL();
                    if (terms != null)
                    {
                        terms.RemoveAll(e => e == null);
                        ListResultSearch = new ObservableCollection<string>(terms);
                    }
                    break;
                default:
                    break;
            }
        }



        private void updateInfo()
        {
            

            string selVal = SelectedResultSearch;
            List<datacontract.metadata> metaObjs = (from n in metaList.metadataRecords
                                                    where n.title == selVal
                                                    select n).ToList();
            if (metaObjs.Count > 0)
            {
                datacontract.metadata metaObj = metaObjs[0];
                string infoMsg = string.Format("<h2>{0}</h2><br/><div>{1}</div>", metaObj.title, metaObj.description);
                infoMsg += string.Format(
                "<div><a target='_blank' href='https://metadata.vlaanderen.be/srv/dut/catalog.search#/metadata/{0}'>Ga naar fiche in metadata center</a></div>",
                                          metaObj.geonet.uuid);

                if (metaObj.links != null && metaObj.links.Count() > 0)
                {
                    infoMsg += "<br/><ul>";
                    foreach (string link in metaObj.links)
                    {
                        string[] links = link.Split('|');
                        if (links[3].ToUpper().Contains("DOWNLOAD"))
                        {
                            string url = links[2];
                            string name = (String.IsNullOrEmpty(links[0])) ? links[1] : links[0];
                            infoMsg += string.Format("<li><a target='_blank' href='{1}'>{0}</a></li>", name, url);
                        }
                    }
                    infoMsg += "</ul>";
                }
                WebBrowserControl.NavigateToString(infoMsg);
                

            }
        }



        private void loadArcgis()
        {
            //string selVal = SelectedResultSearch;
            //string arcName;
            //string arcUrl;

            //bool hasArc = metaList.geturl(selVal, "Esri Rest API", out arcUrl, out arcName, 0);
            //if (hasArc)
            //{
            //    arcUrl = arcUrl.Split('?')[0] + "?";
            //    if (geopuntHelper.websiteExists(arcUrl, true) == false)
            //    {
            //        MessageBox.Show("Kan geen connectie maken met de Service.", "Connection timed out");
            //        return;
            //    }
            //    geopuntHelper.addAGS2map(MapView.Active.Map, arcUrl, arcName);

            //}

        }

        private void addWMS()
        {
            //string selVal = searchResultsList.Text;
            //List<ESRI.ArcGIS.GISClient.IWMSLayerDescription> lyrDescripts;
            //string lyrDlgName; string lyrName; string wmsUrl;
            //layerSelectionForm lyrDlg;

            //bool hasWms = metaList.geturl(selVal, "OGC:WMS", out wmsUrl, out lyrName);
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
            //        lyrDescripts = geopuntHelper.listWMSlayers(wmsUrl);
            //        if (lyrDescripts.Count <= 2)
            //        {
            //            geopuntHelper.addWMS2map(ArcMap.Document.FocusMap, wmsUrl);
            //            return;
            //        }

            //        lyrDlg = new layerSelectionForm(lyrDescripts, lyrName);
            //        lyrDlg.ShowDialog(this);

            //        lyrDlgName = lyrDlg.selectedLayerName;

            //        if (lyrDlgName != null)
            //        {
            //            ILayer lyr = geopuntHelper.getWMSLayerByName(wmsUrl, lyrDlgName);
            //            if (lyr != null)
            //            {
            //                ArcMap.Document.FocusMap.AddLayer(lyr);
            //            }
            //            else
            //            {
            //                geopuntHelper.addWMS2map(ArcMap.Document.FocusMap, wmsUrl);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message + " : " + ex.StackTrace, "Error");
            //    }
            //}
        }




        private List<string> filterDL()
        {
            if (metaList == null) return null;
            return (from g in metaList.metadataRecords
                    where metaList.geturl(g.title, "DOWNLOAD")
                    select g.title).ToList<string>();
        }

        private List<string> filterWMS()
        {
            if (metaList == null) return null;
            return (from g in metaList.metadataRecords
                    where metaList.geturl(g.title, "OGC:WMS")
                    select g.title).ToList<string>();
        }

        private List<string> filterAGS()
        {
            if (metaList == null) return null;
            return (from g in metaList.metadataRecords
                    where metaList.geturl(g.title, "Esri Rest API", 0)
                    select g.title).ToList<string>();
        }

        private List<string> geenFilter()
        {
            if (metaList == null) return null;
            return (from g in metaList.metadataRecords
                    select g.title).ToList<string>();
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
