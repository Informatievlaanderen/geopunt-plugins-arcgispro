using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using ArcGIS.Desktop.Mapping;

namespace GeoPunt.Helpers
{
    public class Utils
    {
        public Utils()
        {
        }

        public void zoomTo(MapPoint mapPoint, int distance = 500)
        {


            if (mapPoint == null)
            {
                MessageBox.Show($"Unable to zoom: MapPoint is null");
                return;
            }

            if (mapPoint.SpatialReference == null)
            {
                MessageBox.Show($"Unable to zoom: MapPoint spatial reference is null");
                return;
            }

            QueuedTask.Run(() =>
            {
                MapView mapView = MapView.Active;
                int mapWkid = mapView.Map.SpatialReference.Wkid;

                if (mapWkid != mapPoint.SpatialReference.Wkid)
                {
                    MapPoint projectedMapPoint = GeometryEngine.Instance.Project(mapPoint, SpatialReferenceBuilder.CreateSpatialReference(mapWkid)) as MapPoint;
                    ArcGIS.Core.Geometry.Polygon buffedMapPoint = GeometryEngine.Instance.Buffer(projectedMapPoint, distance) as ArcGIS.Core.Geometry.Polygon;
                    mapView.ZoomTo(buffedMapPoint, new TimeSpan(0, 0, 0, 1));

                }
                else
                {
                    ArcGIS.Core.Geometry.Polygon buffedMapPoint = GeometryEngine.Instance.Buffer(mapPoint, distance) as ArcGIS.Core.Geometry.Polygon;
                    mapView.ZoomTo(buffedMapPoint, new TimeSpan(0, 0, 0, 1));
                }

            });
        }

        public void zoomTo(Polygon polygon)
        {


            if (polygon == null)
            {
                MessageBox.Show($"Unable to zoom: Polygon is null");
                return;
            }


            if (polygon.SpatialReference == null)
            {
                MessageBox.Show($"Unable to zoom: Polygon spatial reference is null");
                return;
            }

            QueuedTask.Run(() =>
            {
                MapView mapView = MapView.Active;
                int mapWkid = mapView.Map.SpatialReference.Wkid;

                if (mapWkid != polygon.SpatialReference.Wkid)
                {
                    Polygon projectedPolygon = GeometryEngine.Instance.Project(polygon, SpatialReferenceBuilder.CreateSpatialReference(mapWkid)) as Polygon;
                    mapView.ZoomTo(projectedPolygon, new TimeSpan(0, 0, 0, 1));

                }
                else
                {
                    mapView.ZoomTo(polygon, new TimeSpan(0, 0, 0, 1));
                }

            });
        }
    }
}
