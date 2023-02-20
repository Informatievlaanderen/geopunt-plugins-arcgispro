using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("GeoPunt_Module");

        //public static async Task OnProjectClosingAsync(CancelEventArgs arg)
        //{
        //    var paneStart = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_StartDockpane");
        //    var paneAddress = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_SearchAddressDockpane");
        //    var panePointMap = FrameworkApplication.DockPaneManager.Find("GeoPunt_Dockpanes_PointMapDockpane");
        //    paneStart.Hide();
        //    paneAddress.Hide();
        //    panePointMap.Hide();
        //    MessageBox.Show($@"execute event close project");
        //    //FrameworkApplication.Panes.ClosePane(MyPaneID);
        //    await Project.Current.SaveAsync();

        //}
        


        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides


       
        









    }
}
