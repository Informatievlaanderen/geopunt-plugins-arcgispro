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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.DrawTools
{
    internal class Drawline : MapTool
    {

        public Drawline()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Line;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            ActivateVM();

            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (ActivateVM())
            {
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                Polyline polyline = geometry as Polyline;
                Module1.ElevationProfileViewModel.ProfileLine = polyline;
                
            }
            return base.OnSketchCompleteAsync(geometry);
        }

        private bool ActivateVM()
        {

            if (Module1.ElevationProfileViewModel == null)
            {
                MessageBox.Show("To use this tool, please open its pane.");
                return false;
            }
            else
            {
                Module1.ElevationProfileViewModel.Activate();
                return true;
            }
        }

    }
}
