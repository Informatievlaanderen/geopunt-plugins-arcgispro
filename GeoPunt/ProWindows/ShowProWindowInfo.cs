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
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace GeoPunt
{
    internal class ShowProWindowInfo : Button
    {

        private ProWindowInfo _prowindowinfo = null;

        protected override void OnClick()
        {
            //already open?
            if (_prowindowinfo != null)
                return;
            _prowindowinfo = new ProWindowInfo();
            _prowindowinfo.Owner = FrameworkApplication.Current.MainWindow;
            _prowindowinfo.Closed += (o, e) => { _prowindowinfo = null; };
            _prowindowinfo.Show();
            //uncomment for modal
            //_prowindowinfo.ShowDialog();
        }

    }
}
