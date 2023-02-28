using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using GeoPunt.datacontract;
using GeoPunt.DataHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;


namespace GeoPunt.Dockpanes
{
    internal class SearchPerceelViewModel : DockPane
    {
        private const string _dockPaneID = "GeoPunt_Dockpanes_SearchPerceel";

        private List<string> _listGemeente = new List<string>();
        public List<string> ListGemeente
        {
            get { return _listGemeente; }
            set
            {
                SetProperty(ref _listGemeente, value);
            }
        }

        private string _selectedListGemeente;
        public string SelectedListGemeente
        {
            get { return _selectedListGemeente; }
            set
            {
                SetProperty(ref _selectedListGemeente, value);
                gemeenteSelectionChange();
            }
        }

        private List<string> _listDepartments = new List<string>();
        public List<string> ListDepartments
        {
            get { return _listDepartments; }
            set
            {
                SetProperty(ref _listDepartments, value);
            }
        }

        private string _selectedListDepartments;
        public string SelectedListDepartments
        {
            get { return _selectedListDepartments; }
            set
            {
                SetProperty(ref _selectedListDepartments, value);
                departmentSelectionChange();
            }
        }

        List<datacontract.municipality> municipalities;
        List<datacontract.department> departments;
        List<datacontract.parcel> parcels;
        datacontract.parcel perceel;

        DataHandler.capakey capakey;
        public SearchPerceelViewModel() 
        {
            capakey = new DataHandler.capakey(5000);
            municipalities = capakey.getMunicipalities().municipalities;
            ListGemeente = (from n in municipalities select n.municipalityName).ToList();
        }

        
        private void gemeenteSelectionChange()
        {
            MessageBox.Show($@"gemeente Selection change");
            string gemeente = SelectedListGemeente;
            string niscode = municipality2nis(gemeente);

            //add2mapBtn.Enabled = false;
            //addMarkerBtn.Enabled = false;

            //msgLbl.Text = "";
            //perceel = null;

            if (niscode == "" || niscode == null) return;

            try
            {
                departments = capakey.getDepartments(int.Parse(niscode)).departments;
                //departementCbx.Items.Clear(); departementCbx.Text = "";
                //sectieCbx.Items.Clear(); sectieCbx.Text = "";
                //parcelCbx.Items.Clear(); parcelCbx.Text = "";
                ListDepartments = (from n in departments select n.departmentName).ToList();
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
                MessageBox.Show(ex.Message + ": " + ex.StackTrace);
            }
        }

        public void departmentSelectionChange()
        {
            MessageBox.Show($@"department Selection change");
        }

        private string municipality2nis(string muniName)
        {
            if (muniName == null || muniName == "") return "";

            var niscodes = (
                from n in municipalities
                where n.municipalityName == muniName
                select n.municipalityCode);

            if (niscodes.Count() == 0) return "";

            return niscodes.First<string>();
        }
        private ObservableCollection<MapPoint> LisPointsFromPolygones = new ObservableCollection<MapPoint>();

        public void updateListPointFromPolygone()
        {
            foreach (MapPoint mapPoint in LisPointsFromPolygones)
            {
                GeocodeUtils.UpdateMapOverlayMapPoint(mapPoint, MapView.Active, true);
            }
        }

        private CIMPointSymbol CreatePoinSymbol(System.Drawing.Color color, double size)
        {
            var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.CreateColor(color), size, SimpleMarkerStyle.Square);
            pointSymbol.UseRealWorldSymbolSizes = true;
            var marker = pointSymbol.SymbolLayers[0] as CIMVectorMarker;
            var polygonSymbol = marker.MarkerGraphics[0].Symbol as CIMPolygonSymbol;
            polygonSymbol.SymbolLayers[0] = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 0, SimpleLineStyle.Null);
            return pointSymbol;
        }
        private void createGrapicAndZoomTo(string capakeyResponse, datacontract.geojson Geom)
        {
            //IRgbColor inClr = new RgbColorClass() { Red = 0, Blue = 100, Green = 0 }; ;
            //IRgbColor outLine = new RgbColorClass() { Red = 0, Blue = 200, Green = 0, Transparency = 240 };

            if (Geom.type == "MultiPolygon")
            {
         
                datacontract.geojsonMultiPolygon muniPolygons =
                                  JsonConvert.DeserializeObject<datacontract.geojsonMultiPolygon>(capakeyResponse);

                //IGeometryCollection multiPoly = new GeometryBagClass();

                //clearGraphics();

                foreach (datacontract.geojsonPolygon poly in muniPolygons.toPolygonList())
                {
                    MessageBox.Show($@"Multipolygones :: {poly}");

                    //IPolygon lbPoly = geopuntHelper.geojson2esriPolygon(poly, (int)dataHandler.CRS.Lambert72);
                    //lbPoly.SimplifyPreserveFromTo();
                    //IGeometry prjGeom = geopuntHelper.Transform((IGeometry)lbPoly, map.SpatialReference);

                    //IElement muniGrapic = geopuntHelper.AddGraphicToMap(map, prjGeom, inClr, outLine, 2, true);
                    //graphics.Add(muniGrapic);

                    //object Missing = Type.Missing;
                    //multiPoly.AddGeometry(prjGeom, ref Missing, ref Missing);
                }
                //view.Extent = ((IGeometryBag)multiPoly).Envelope;
                //view.Refresh();
            }
            else if (Geom.type == "Polygon")
            {
                datacontract.geojsonPolygon municipalityPolygon =
                            JsonConvert.DeserializeObject<datacontract.geojsonPolygon>(capakeyResponse);
                MapPoint MapPointFromPolygone = null;
                // NB POLYGONES
                //MessageBox.Show($@"Polygon: nb coordinates:: {municipalityPolygon.coordinates.Count}");

                foreach (var a in municipalityPolygon.coordinates)
                {
                    // NB POINTS MAP
                    //MessageBox.Show($@"aa: nb points:: {a.Count}");
                    foreach (var b in a)
                    {
                        // X & Y
                        //MessageBox.Show($@"x ::: {b[0]} || y ::: {b[1]}");

                        MapPointFromPolygone = MapPointBuilderEx.CreateMapPoint(b[0], b[1]);
                        //MapPolygoneFromPolygone = PolygonBuilderEx.CreatePolygon()
                        
                        LisPointsFromPolygones.Add(MapPointFromPolygone);

                        //foreach (var c in b)
                        //{
                        //    MessageBox.Show($@"cc ::: {c} || {c.ToString()} || {c.GetType}");
                        //}
                    }
                }

                //List<MapPoint> list = new List<MapPoint>();
                //var pointSymbol = CreatePoinSymbol(Color.FromArgb(255, 0, 255, 255), 10.0);
                //MapPoint point = MapPointBuilder.CreateMapPoint(centerPoint.X + dx, centerPoint.Y + dy, sr);
                //for (int Row = 0; Row < quadrant; ++Row)
                //{
                //    for (int Col = 0; Col < quadrant; ++Col)
                //    {
                //        list.Add(point);
                //        //Graphics.Add(mapView.AddOverlay(point, pointSymbol.MakeSymbolReference()));
                //        point = MapPointBuilder.CreateMapPoint(point.X + dx, point.Y, sr);
                //        list.Add(point);
                //    }

                //    point = MapPointBuilder.CreateMapPoint(centerPoint.X + dx, point.Y + dy, sr);
                //}

                //ArcGIS.Core.CIM.CIMSymbolReference symbol = null;
                //symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 10.0, SimpleMarkerStyle.Circle);

                //other



                QueuedTask.Run(() =>
                {
                    //Build geometry
                    //List<Coordinate2D> plyCoords = new List<Coordinate2D>();
                    //plyCoords.Add(new Coordinate2D(1, 7));
                    //plyCoords.Add(new Coordinate2D(2, 7));
                    //plyCoords.Add(new Coordinate2D(2, 6.7));
                    //plyCoords.Add(new Coordinate2D(3, 6.7));
                    //plyCoords.Add(new Coordinate2D(3, 6.1));
                    //plyCoords.Add(new Coordinate2D(1, 6.1));
                    ////At 2.x - Polygon poly = PolygonBuilder.CreatePolygon(plyCoords);
                    //ArcGIS.Core.Geometry.Polygon poly = PolygonBuilderEx.CreatePolygon(plyCoords);

                    ArcGIS.Core.Geometry.Polygon poly = PolygonBuilderEx.CreatePolygon(LisPointsFromPolygones);
                    

                    //Set symbolology, create and add element to layout
                    CIMStroke outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
                    CIMPolygonSymbol polySym = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlueRGB, SimpleFillStyle.ForwardDiagonal, outline);
                    //At 2.x - GraphicElement polyElm = LayoutElementFactory.Instance.CreatePolygonGraphicElement(layout, poly, polySym);
                    //         polyElm.SetName("New Polygon"); 

                    MapView.Active.AddOverlay(poly,polySym.MakeSymbolReference());
                    MapView.Active.ZoomTo(poly, new TimeSpan(0, 0, 0, 1));


                    //GraphicElement polyElm = ElementFactory.Instance.CreateGraphicElement(layout, poly, polySym, "New Polygon");
                });





                //QueuedTask.Run(() => {

                //    var pointSymbol = CreatePoinSymbol(System.Drawing.Color.FromArgb(255, 0, 255, 255), 10.0);

                //    ArcGIS.Core.Geometry.Multipoint multiPoint = MultipointBuilder.CreateMultipoint(LisPointsFromPolygones);
                //    MapView.Active.AddOverlay(multiPoint, pointSymbol.MakeSymbolReference());

                //});


                //updateListPointFromPolygone();






                //var polygonFeatureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                //        lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon).FirstOrDefault();

                //var createOperation = new EditOperation()
                //{
                //    Name = "Create polygons",
                //    SelectNewFeatures = false
                //};

                //var newPolygon = GeometryEngine.Instance.ConvexHull(polylineBuilder.ToGeometry()) as Polygon;

                //createOperation.Create(polygonFeatureLayer, newPolygon);
                //createOperation.ExecuteAsync();










                //IPolygon lbPoly = geopuntHelper.geojson2esriPolygon(municipalityPolygon, (int)dataHandler.CRS.Lambert72);
                //lbPoly.SimplifyPreserveFromTo();
                //IPolygon prjPoly = (IPolygon)geopuntHelper.Transform((IGeometry)lbPoly, map.SpatialReference);
                //view.Extent = prjPoly.Envelope;

                //clearGraphics();
                //IElement muniGrapic = geopuntHelper.AddGraphicToMap(map, (IGeometry)prjPoly, inClr, outLine, 3, true);
                //graphics.Add(muniGrapic);
                //view.Refresh();
            }
        }

        public ICommand CmdZoomGemeente
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    MessageBox.Show($@"Button CmdZoomGemeente click");

                    string gemeente = SelectedListGemeente;
                    string niscode = municipality2nis(gemeente);

                    if (niscode == "" || niscode == null) return;

                    try
                    {
                        datacontract.municipality municipality = capakey.getMunicipalitiy(int.Parse(niscode),
                                                                DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
                        datacontract.geojson municipalityGeom = JsonConvert.DeserializeObject<datacontract.geojson>(municipality.geometry.shape);

                        createGrapicAndZoomTo(municipality.geometry.shape, municipalityGeom);
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
                        MessageBox.Show(ex.Message + ": " + ex.StackTrace);
                    }
                });
            }
        }

        public ICommand CmdZoomDepartment
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    MessageBox.Show($@"Button CmdZoomDepartment click");
                });
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
    internal class SearchPerceel_ShowButton : Button
    {
        protected override void OnClick()
        {
            SearchPerceelViewModel.Show();
        }
    }
}
