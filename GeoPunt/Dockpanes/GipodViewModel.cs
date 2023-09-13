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
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.Dockpanes
{
    internal class GipodViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_Gipod";

        protected GipodViewModel()
        {
            initGUI();
        }



        private void initGUI()
        {
            capakey capakey = new capakey(5000);
            gipod gipod = new gipod(5000);
            municipalityList municipality = capakey.getMunicipalities();

            List<string> provincies = new List<string>() { "Antwerpen", "Limburg", "Vlaams-Brabant", "Oost-Vlaanderen", "West-Vlaanderen" };
            List<string> municipalities = (from datacontract.municipality t in municipality.municipalities select t.municipalityName).ToList();
            List<string> owners = gipod.getReferencedata(gipodReferencedata.owner);
            List<string> eventTypes = gipod.getReferencedata(gipodReferencedata.eventtype);

            ListProvincie = new ObservableCollection<string>(provincies);
            ListGemeente = new ObservableCollection<string>(municipalities);
            ListEigenaar = new ObservableCollection<string>(owners);
            ListManifestatie = new ObservableCollection<string>(eventTypes);

        


        private bool _manifestionChecked = false;
        public bool ManifestionChecked
        {
            get { return _manifestionChecked; }
            set
            {
                SetProperty(ref _manifestionChecked, value);
            }
        }

        private ObservableCollection<string> _listProvincie = new ObservableCollection<string>();
        public ObservableCollection<string> ListProvincie
        {
            get { return _listProvincie; }
            set
            {
                SetProperty(ref _listProvincie, value);
            }
        }



        private string _selectedProvincie;
        public string SelectedProvincie
        {
            get { return _selectedProvincie; }
            set
            {
                SetProperty(ref _selectedProvincie, value);
            }
        }


        private ObservableCollection<string> _listGemeente = new ObservableCollection<string>();
        public ObservableCollection<string> ListGemeente
        {
            get { return _listGemeente; }
            set
            {
                SetProperty(ref _listGemeente, value);
            }
        }



        private string _selectedGemeente;
        public string SelectedGemeente
        {
            get { return _selectedGemeente; }
            set
            {
                SetProperty(ref _selectedGemeente, value);
            }
        }


        private ObservableCollection<string> _listEigenaar = new ObservableCollection<string>();
        public ObservableCollection<string> ListEigenaar
        {
            get { return _listEigenaar; }
            set
            {
                SetProperty(ref _listEigenaar, value);
            }
        }



        private string _selectedEigenaar;
        public string SelectedEigenaar
        {
            get { return _selectedEigenaar; }
            set
            {
                SetProperty(ref _selectedEigenaar, value);
            }
        }


        private ObservableCollection<string> _listManifestatie = new ObservableCollection<string>();
        public ObservableCollection<string> ListManifestatie
        {
            get { return _listManifestatie; }
            set
            {
                SetProperty(ref _listManifestatie, value);
            }
        }



        private string _selectedManifestatie;
        public string SelectedManifestatie
        {
            get { return _selectedManifestatie; }
            set
            {
                SetProperty(ref _selectedManifestatie, value);
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
        private string _heading = "Bevraag GIPOD";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Gipod_ShowButton : Button
    {
        protected override void OnClick()
        {
            GipodViewModel.Show();
        }
    }
}
