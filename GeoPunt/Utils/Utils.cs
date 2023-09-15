using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using GeoPunt.Dockpanes;
using System.IO;
using System.Drawing;
using GeoPunt.GeoJSON;
using static GeoPunt.GeoJSON.GeoJSONGeometry;
using System.Linq;
using GeoPunt.datacontract;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ArcGIS.Desktop.Internal.DesktopService;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GeoPunt.Helpers
{
    public class Utils
    {
        public Utils()
        {
        }


        public void ZoomTo(Geometry geometry)
        {

            switch (geometry.GeometryType)
            {
                case GeometryType.Point:
                    ZoomTo(geometry as MapPoint);
                    break;
                case GeometryType.Polygon:
                    ZoomTo(geometry as Polygon);
                    break;
                default:
                    MessageBox.Show("Zoom is not supported for this geometry.");
                    break;
            }

        }


        public void ZoomTo(MapPoint mapPoint, int distance = 500)
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

        public void ZoomTo(Polygon polygon)
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

        public MapPoint CreateMapPoint(double x, double y, SpatialReference spatialReference)
        {
            MapPoint mapPoint = MapPointBuilderEx.CreateMapPoint(x, y, spatialReference);
            MapView mapView = MapView.Active;
            int mapWkid = mapView.Map.SpatialReference.Wkid;

            if (mapWkid != mapPoint.SpatialReference.Wkid)
            {
                MapPoint projectedMapPoint = GeometryEngine.Instance.Project(mapPoint, SpatialReferenceBuilder.CreateSpatialReference(mapWkid)) as MapPoint;
                return projectedMapPoint;
            }

            return mapPoint;

        }


        public Polygon CreatePolygon(IEnumerable<MapPoint> points, SpatialReference spatialReference)
        {
            Polygon polygon = PolygonBuilderEx.CreatePolygon(points, spatialReference);
            MapView mapView = MapView.Active;
            int mapWkid = mapView.Map.SpatialReference.Wkid;

            if (mapWkid != polygon.SpatialReference.Wkid)
            {
                Polygon projectedPolygon = GeometryEngine.Instance.Project(polygon, SpatialReferenceBuilder.CreateSpatialReference(mapWkid)) as Polygon;
                return projectedPolygon;
            }

            return polygon;
        }

        public void ExportToGeoJson(List<Graphic> graphics)
        {
            List<GeoJSONFeature> geoJSONFeatures = new List<GeoJSONFeature>();
            foreach (Graphic graphic in graphics)
            {

                switch (graphic.Geometry.GeometryType)
                {

                    case GeometryType.Point:
                        MapPoint mapPoint = graphic.Geometry as MapPoint;
                        geoJSONFeatures.Add(new GeoJSONFeature(new GeoJSONPointGeometry(mapPoint.X, mapPoint.Y), graphic.Attributes));
                        break;

                    case GeometryType.Polyline:
                        Polyline polyline = graphic.Geometry as Polyline;
                        geoJSONFeatures.Add(new GeoJSONFeature(new GeoJSONLineStringGeometry((List<List<double>>)polyline.Copy2DCoordinatesToList()), graphic.Attributes));
                        break;
                    case GeometryType.Polygon:
                        Polygon polygon = graphic.Geometry as Polygon;
                        List<List<double>> coordinates = new List<List<double>>();
                        ReadOnlyPointCollection test = polygon.Points;
                        foreach (MapPoint item in test)
                        {
                            List<double> xAndY = new List<double>
                            {
                                item.X,
                                item.Y
                            };
                            coordinates.Add(xAndY);
                        }

                        List<List<List<double>>> polygonCoordinates = new List<List<List<double>>>
                        {
                            coordinates
                        };

                        geoJSONFeatures.Add(new GeoJSONFeature(new GeoJSONPolygonGeometry(polygonCoordinates), graphic.Attributes));
                        break;

                    default:
                        break;
                }
            }

            if (graphics.Count > 0 && graphics[0].Geometry.SpatialReference != null && graphics[0].Geometry.SpatialReference.Wkid != 4326)
            {
                GeoJSONClass geoJSON = new GeoJSONClass(geoJSONFeatures, graphics[0].Geometry.SpatialReference.Wkid);
                SaveJsonFile(geoJSON);
            }
            else
            {
                GeoJSONClass geoJSON = new GeoJSONClass(geoJSONFeatures);
                SaveJsonFile(geoJSON);
            }


        }

        private void SaveJsonFile(object objectToExport)
        {
            System.Windows.Forms.SaveFileDialog oSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            oSaveFileDialog.Filter = "Json files (*.json) | *.json";
            if (oSaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = oSaveFileDialog.FileName;

                using StreamWriter file = File.CreateText(fileName);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, objectToExport);

            }
        }

    }
}
