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
    internal class CSVfileViewModel : DockPane
    {

        private string _textFilePlacement;
        public string TextFilePlacement
        {
            get { return _textFilePlacement; }
            set
            {
                SetProperty(ref _textFilePlacement, value);
            }
        }

        //private ObservableCollection<string> ListFormats = new ObservableCollection<string>(new List<string>() {
        //    "Aalst",
        //    "Kruisem"
        //});
        //public ObservableCollection<string> ListCities
        //{
        //    get { return _listCities; }
        //    set
        //    {
        //        SetProperty(ref _listCities, value);
        //    }
        //}

        //private ObservableCollection<string> _listCities = new ObservableCollection<string>(new List<string>() {
        //    "Aalst",
        //    "Kruisem"
        //});
        //public ObservableCollection<string> ListCities
        //{
        //    get { return _listCities; }
        //    set
        //    {
        //        SetProperty(ref _listCities, value);
        //    }
        //}




        private const string _dockPaneID = "GeoPunt_Dockpanes_CSVfile";

        protected CSVfileViewModel() { }

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
    internal class CSVfile_ShowButton : Button
    {
        protected override void OnClick()
        {
            CSVfileViewModel.Show();
        }
    }
}
