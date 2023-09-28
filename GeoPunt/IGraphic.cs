using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Internal.Framework.Utilities;
using ArcGIS.Desktop.Mapping;
using GeoPunt.Dockpanes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt
{

    internal interface IGraphicDisplayer
    {

        ObservableCollection<Graphic> GraphicsList { get; set; }

        Graphic SelectedGraphic { get; set; }


    }

    internal interface IMarkedGraphicDisplayer : IGraphicDisplayer
    {
        ObservableCollection<Graphic> MarkedGraphicsList { get; set; }


        void MarkGraphic(Graphic SelectedGraphic);

       

     
    }


}
