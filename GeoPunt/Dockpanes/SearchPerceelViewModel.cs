﻿using ArcGIS.Core.CIM;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
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
            //MessageBox.Show($@"gemeente Selection change");
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
            //MessageBox.Show($@"department Selection change");
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

        private string department2code(string depName)
        {
            if (depName == null || depName == "") return "";

            var depcodes = (
                from n in departments
                where n.departmentName == depName
                select n.departmentCode);

            if (depcodes.Count() == 0) return "";

            return depcodes.First<string>();
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

        public static void RemoveFromMapOverlayPerceel()
        {
            if (_overlayObjectPerceel != null)
            {
                foreach (var overlay in _overlayObjectPerceel)
                {
                    overlay.Dispose();
                }
                _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
            }
        }

        private static ObservableCollection<System.IDisposable> _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();

        ArcGIS.Core.Geometry.Polygon lastPoly;
        private async void createGrapicAndZoomTo(string capakeyResponse, datacontract.geojson Geom)
        {
            

            if (Geom.type == "MultiPolygon")
            {
         
                datacontract.geojsonMultiPolygon muniPolygons =
                                  JsonConvert.DeserializeObject<datacontract.geojsonMultiPolygon>(capakeyResponse);

                foreach (datacontract.geojsonPolygon poly in muniPolygons.toPolygonList())
                {
                    MessageBox.Show($@"Multipolygones :: {poly}");

                }
            }
            else if (Geom.type == "Polygon")
            {
                datacontract.geojsonPolygon municipalityPolygon =
                            JsonConvert.DeserializeObject<datacontract.geojsonPolygon>(capakeyResponse);
                MapPoint MapPointFromPolygone = null;
                LisPointsFromPolygones.Clear();


                foreach (var a in municipalityPolygon.coordinates)
                {
                    foreach (var b in a)
                    {

                        MapPointFromPolygone = MapPointBuilderEx.CreateMapPoint(b[0], b[1]);
                        
                        LisPointsFromPolygones.Add(MapPointFromPolygone);

                    }
                }

                await QueuedTask.Run(() =>
                {
                    if (_overlayObjectPerceel.Count > 0)
                    {
   

                        foreach (var overlay in _overlayObjectPerceel)
                        {
                            overlay.Dispose();
                        }
                        _overlayObjectPerceel = new ObservableCollection<System.IDisposable>();
                        
                    }

                    

                    ArcGIS.Core.Geometry.Polygon poly = PolygonBuilderEx.CreatePolygon(LisPointsFromPolygones);

                    //Set symbolology, create and add element to layout
                    CIMStroke outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
                    CIMPolygonSymbol polySym = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlueRGB, SimpleFillStyle.ForwardDiagonal, outline);
                    

                    _overlayObjectPerceel.Add(MapView.Active.AddOverlay(poly,polySym.MakeSymbolReference()));
                    MapView.Active.ZoomTo(poly, new TimeSpan(0, 0, 0, 1));

                });
            }
        }

        public ICommand CmdZoomGemeente
        {
            get
            {
                return new RelayCommand(async () =>
                {
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
                    string gemeente = SelectedListGemeente;
                    string niscode = municipality2nis(gemeente);

                    string department = SelectedListDepartments;
                    string depCode = department2code(department);

                    if (niscode == "" || niscode == null) return;

                    datacontract.department dep = capakey.getDepartment(int.Parse(niscode), int.Parse(depCode),
                                                        DataHandler.CRS.Lambert72, DataHandler.capakeyGeometryType.full);
                    datacontract.geojson depGeom = JsonConvert.DeserializeObject<datacontract.geojson>(dep.geometry.shape);

                    createGrapicAndZoomTo(dep.geometry.shape, depGeom);
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
