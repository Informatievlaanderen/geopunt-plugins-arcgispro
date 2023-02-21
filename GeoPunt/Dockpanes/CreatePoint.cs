using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Editing;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Dockpanes
{
    internal class CreatePoint : MapTool
    {
        private CIMPointSymbol _pointSymbol = null;
        private IDisposable _graphic = null;
        private SpatialReference Lambert72 = null;
        private SpatialReference Lambert2008 = null;
        private SpatialReference WGS84 = null;

        public CreatePoint()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
            //Module1.CreatePoint = this;
            _pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.CreateRGBColor(230, 35, 252), 10, SimpleMarkerStyle.Circle);

            QueuedTask.Run(() =>
            {
                Lambert72 = SpatialReferenceBuilder.CreateSpatialReference(31370);
                Lambert2008 = SpatialReferenceBuilder.CreateSpatialReference(3812);
                WGS84 = SpatialReferences.WGS84;
            });

        }

        //protected async override Task OnToolActivateAsync(bool active)
        //{
        //    if (_pointSymbol == null) _pointSymbol = await CreatePointSymbolAsync();

        //    //if (Module1.CreePointVerticalVM != null)
        //    //    CheckForCoordinates(Module1.CreePointVerticalVM);
        //}

        //private async void CheckForCoordinates(CreePointVerticalViewModel creePointVerticalVM)
        //{
        //    bool haveLongitude = false;
        //    bool haveLatitude = false;
        //    double longitude = 0;
        //    double latitude = 0;

        //    haveLongitude = double.TryParse(creePointVerticalVM.XLambert72, out longitude);
        //    haveLatitude = double.TryParse(creePointVerticalVM.YLambert72, out latitude);

        //    if (haveLongitude && haveLatitude)
        //    {
        //        await QueuedTask.Run(() =>
        //        {
        //            MapPoint point = MapPointBuilder.CreateMapPoint(longitude, latitude, Lambert72);
        //            UpdatePoint(point);
        //        });
        //        return;
        //    }

        //    haveLongitude = double.TryParse(creePointVerticalVM.Longitude, out longitude);
        //    haveLatitude = double.TryParse(creePointVerticalVM.Latitude, out latitude);

        //    if (haveLongitude && haveLatitude)
        //    {
        //        await QueuedTask.Run(() =>
        //        {
        //            MapPoint point = MapPointBuilder.CreateMapPoint(longitude, latitude, WGS84);
        //            UpdatePoint(point);
        //        });
        //        return;
        //    }

        //    haveLongitude = double.TryParse(creePointVerticalVM.XLambert08, out longitude);
        //    haveLatitude = double.TryParse(creePointVerticalVM.YLambert08, out latitude);

        //    if (haveLongitude && haveLatitude)
        //    {
        //        await QueuedTask.Run(() =>
        //        {
        //            MapPoint point = MapPointBuilder.CreateMapPoint(longitude, latitude, Lambert2008);
        //            UpdatePoint(point);
        //        });
        //        return;
        //    }

        //}

        //protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        //        e.Handled = true;
        //}

        //protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        //{

        //    if (Module1.CreePointVerticalVM == null)
        //    {
        //        return QueuedTask.Run(() =>
        //        {
        //            MessageBox.Show("Pour utiliser cet outil, veuillez ouvrir son autre outil complémentaire.");
        //        });
        //    }
        //    else
        //    {
        //        Module1.CreePointVerticalVM.Activate();
        //    }

        //    if (_graphic != null) _graphic.Dispose();
        //    _graphic = null;

        //    return QueuedTask.Run(async () =>
        //    {
        //        var mapPoint = ActiveMapView.ClientToMap(e.ClientPoint);
        //        var geometry = GeometryEngine.Instance.Project(mapPoint, Lambert72);
        //        var coords = geometry as MapPoint;
        //        if (coords == null) return;

        //        _graphic = await this.AddOverlayAsync(coords, _pointSymbol.MakeSymbolReference());

        //        Module1.CreePointVerticalVM.SetCoords(coords);

        //        await FinishSketchAsync();

        //    });
        //}

        //protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        //{
        //    if (_graphic != null) _graphic.Dispose();//Clear out the old overlay
        //    _graphic = null;
        //    return base.OnToolDeactivateAsync(hasMapViewChanged);
        //}

        //internal static Task<CIMPointSymbol> CreatePointSymbolAsync()
        //{
        //    return QueuedTask.Run(() => SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.CreateRGBColor(230, 35, 252), 10, SimpleMarkerStyle.Circle));
        //}

        public void UpdatePoint(MapPoint pt)
        {

          
                var geometry = GeometryEngine.Instance.Project(pt, Lambert72);
                var coords = geometry as MapPoint;
                //if (coords == null) return;
                MessageBox.Show("drawing");
                _graphic = this.AddOverlayAsync(coords, _pointSymbol.MakeSymbolReference());
           
        }
    }
}
