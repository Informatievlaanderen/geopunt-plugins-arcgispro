using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
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
using GeoPunt.DataHandler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GeoPunt.Dockpanes
{
    internal class CSVfileViewModel : DockPane
    {
        private bool _isCheckedMeerdere = true;
        public bool IsCheckedMeerdere
        {
            get { return _isCheckedMeerdere; }
            set
            {
                SetProperty(ref _isCheckedMeerdere, value);
                if (_isCheckedMeerdere)
                {
                    IsCheckedOne = !value;
                }
            }
        }

        private bool _isCorrectAddress = true;
        public bool IsCorrectAddress
        {
            get { return _isCorrectAddress; }
            set
            {
                SetProperty(ref _isCorrectAddress, value);
            }
        }

        private bool _isCheckedOne;
        public bool IsCheckedOne
        {
            get { return _isCheckedOne; }
            set
            {
                SetProperty(ref _isCheckedOne, value);
                if (_isCheckedOne)
                {
                    IsCheckedMeerdere = !value;
                }
            }
        }

        private string _textFilePlacement = "< input CSV-file >";
        public string TextFilePlacement
        {
            get { return _textFilePlacement; }
            set
            {
                SetProperty(ref _textFilePlacement, value);
            }
        }

        private string _selectedStraat;
        public string SelectedStraat
        {
            get { return _selectedStraat; }
            set
            {
                SetProperty(ref _selectedStraat, value);
            }
        }

        private string _selectedHuisnummer;
        public string SelectedHuisnummer
        {
            get { return _selectedHuisnummer; }
            set
            {
                SetProperty(ref _selectedHuisnummer, value);
            }
        }

        private string _selectedGemeente;
        public string SelectedGemeente
        {
            get { return _selectedGemeente; }
            set
            {
                SetProperty(ref _selectedGemeente, value);
            }
        }

        private ObservableCollection<string> _listFormats = new ObservableCollection<string>(new List<string>() {
            "UTF-8",
            "ANSI"
        });
        public ObservableCollection<string> ListFormats
        {
            get { return _listFormats; }
            set
            {
                SetProperty(ref _listFormats, value);
            }
        }

        private ObservableCollection<string> _listSeparators;
        public ObservableCollection<string> ListSeparators
        {
            get { return _listSeparators; }
            set
            {
                SetProperty(ref _listSeparators, value);
            }
        }

        private ObservableCollection<string> _comboBoxListOfColumns = new ObservableCollection<string>(new List<string>());
        public ObservableCollection<string> ComboBoxListOfColumns
        {
            get { return _comboBoxListOfColumns; }
            set
            {
                SetProperty(ref _comboBoxListOfColumns, value);
            }
        }

        private string _selectedListFormats;
        public string SelectedListFormats
        {
            get { return _selectedListFormats; }
            set
            {
                SetProperty(ref _selectedListFormats, value);
            }
        }

        private string _selectedListSeparators;
        public string SelectedListSeparators
        {
            get { return _selectedListSeparators; }
            set
            {
                SetProperty(ref _selectedListSeparators, value);
            }
        }

        private ObservableCollection<DataRowCSV> _dataCsvList = new ObservableCollection<DataRowCSV>();
        public ObservableCollection<DataRowCSV> DataCsvList
        {
            get { return _dataCsvList; }
            set
            {
                SetProperty(ref _dataCsvList, value);
            }
        }



        private DataRowView _selectedDataCsvList;
        public DataRowView SelectedDataCsvList
        {
            get { return _selectedDataCsvList; }
            set
            {
                SetProperty(ref _selectedDataCsvList, value);
                Debug.WriteLine(_selectedDataCsvList);
                if (_selectedDataCsvList != null)
                {
                    string var = _selectedDataCsvList.Row.ItemArray[0] + ", " + _selectedDataCsvList.Row.ItemArray[2];

                    string valCorrect = ""+_selectedDataCsvList.Row.ItemArray[3];
                    IsCorrectAddress = true;
                    if (valCorrect != "Ja")
                    {
                        IsCorrectAddress = false;
                    }

                    updateCurrentMapPoint(var, 1);
                }
            }
        }

        private List<MapPoint> _listCSVMarkeer = new List<MapPoint>();
        public List<MapPoint> ListCSVMarkeer
        {
            get { return _listCSVMarkeer; }
            set
            {
                SetProperty(ref _listCSVMarkeer, value);
            }
        }

        private string _textMarkeer;
        public string TextMarkeer
        {
            get { return _textMarkeer; }
            set
            {
                SetProperty(ref _textMarkeer, value);
            }
        }

        DataHandler.adresLocation adresLocation;
        MapPoint MapPointSelectedAddress = null;
        public void updateCurrentMapPoint(string query, int count)
        {
            double x = 0;
            double y = 0;



            List<datacontract.locationResult> loc = adresLocation.getAdresLocation(query, count);
            foreach (datacontract.locationResult item in loc)
            {
                x = item.Location.X_Lambert72;
                y = item.Location.Y_Lambert72;

            }
            MapPointSelectedAddress = MapPointBuilderEx.CreateMapPoint(x, y);
            //MessageBox.Show($@"update: {MapPointSelectedAddress.X} || {MapPointSelectedAddress.Y}");

            if (ListCSVMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y) != null)
            {
                TextMarkeer = "Verwijder markering";
            }
            else
            {
                TextMarkeer = "Markeer";
            }
        }

        private DataTable _dataTableCSV;
        public DataTable DataTableCSV
        {
            get { return _dataTableCSV; }
            set
            {
                SetProperty(ref _dataTableCSV, value);
            }
        }





        private const string _dockPaneID = "GeoPunt_Dockpanes_CSVfile";
        //BackgroundWorker validationWorker;
        protected CSVfileViewModel() 
        {
            //validationWorker = new System.ComponentModel.BackgroundWorker();
            //DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            sug = new DataHandler.adresSuggestion(5000);
            adresLocation = new DataHandler.adresLocation(5000);
            TextMarkeer = "Markeer";

            ListSeparators = new ObservableCollection<string>(new List<string>() {
                "Puntcomma",
                "Comma",
                "Spatie",
                "Tab",
                /* "Ander teken" */
            });


            SelectedListSeparators = ListSeparators[0];


        }

        public static DataTable loadCSV2datatable(string csvPath, string separator, int maxRows, System.Text.Encoding codex)
        {
            FileInfo csv = new FileInfo(csvPath);
            string sep;
            DataTable tbl = new DataTable();

            System.Text.Encoding textEncoding = System.Text.Encoding.Default;
            if (codex != null) textEncoding = codex;

            textEncoding = Encoding.GetEncoding("iso-8859-1");

            //if (!csv.Exists)
            //    throw new Exception("Deze csv-file bestaat niet: " + csv.Name);
            //if (separator == "" || separator == null)
            //    throw new Exception("Deze separator is niet toegelaten");

            // separator = "Puntcomma";

            switch (separator)
            {
                case "Comma":
                    sep = ",";
                    break;
                case "Puntcomma":
                    sep = ";";
                    break;
                case "Spatie":
                    sep = " ";
                    break;
                case "Tab":
                    sep = "/t";
                    break;
                default:
                    sep = separator;
                    break;
            }
            using (Microsoft.VisualBasic.FileIO.TextFieldParser csvParser =
                            new Microsoft.VisualBasic.FileIO.TextFieldParser(csv.FullName, textEncoding, true))
            {
                csvParser.Delimiters = new string[] { sep };
                csvParser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
                csvParser.TrimWhiteSpace = !(separator == "TAB" || separator == "SPATIE");

                string[] colNames = csvParser.ReadFields();
                string[] row;
                int counter = 0;

                foreach (string colName in colNames)
                {
                    tbl.Columns.Add(colName);
                }
                while (!csvParser.EndOfData)
                {
                    try
                    {
                        if (counter >= maxRows)
                        {
                            return tbl;
                        }
                        counter++;

                        row = csvParser.ReadFields();

                        if (tbl.Columns.Count != row.Count())
                        {
                            throw new Exception("Niet alle rijen hebben hetzelfde aantal kolommen, op eerste lijn: " +
                             tbl.Rows.Count.ToString() + " gevonden: " + row.Count() + " op lijn: " + string.Join(sep, row));
                        }
                        tbl.Rows.Add(row);
                    }
                    catch (Microsoft.VisualBasic.FileIO.MalformedLineException ex)
                    {
                        throw new Exception("CSV is kan niet worden gelezen, het heeft niet de correcte vorm: " + csvParser.ErrorLine, ex);
                    }
                }
            }
            return tbl;
        }

        private async void loadCSV2table()
        {
            System.Text.Encoding codex = System.Text.Encoding.UTF8;
            //if (SelectedListFormats == "UTF-8") codex = ;

            string csvPath = TextFilePlacement;
            DataGridViewComboBoxColumn validatedRow;

            DataTable csvDataTbl;
            

            try
            {
                int maxRowCount = 500;
                csvDataTbl = loadCSV2datatable(csvPath, SelectedListSeparators, maxRowCount, codex);

                if (csvDataTbl.Rows.Count == maxRowCount)
                {
                    string msg = String.Format(
                      "Maximaal aantal van {0} rijen overschreden, enkel de eerste {0} rijen worden getoont.", maxRowCount);
                    System.Windows.MessageBox.Show(msg, "Maximaal aantal rijen overschreden.");
                    //csvErrorLbl.Text = msg;
                }
            }
            catch (Exception csvEx)
            {
                //Debug.WriteLine(csvEx.Message, "Error");
                System.Windows.MessageBox.Show(csvEx.Message, "Error");
                //csvErrorLbl.Text = csvEx.Message;
                return;
            }

            //set validation column
            validatedRow = new DataGridViewComboBoxColumn();
            validatedRow.HeaderText = "Gevalideerd adres";
            validatedRow.Name = "validAdres";
            validatedRow.Width = 120;
            ComboBoxListOfColumns = new ObservableCollection<string>(new List<string>());

            await QueuedTask.Run(() =>
            {
                DataTableCSV = new DataTable();
                

                DataColumn dataTableCsvColumn2 = new DataColumn();
                dataTableCsvColumn2.ColumnName = "Bestaan";
                dataTableCsvColumn2.DefaultValue = "";
                csvDataTbl.Columns.Add(dataTableCsvColumn2);

                
                foreach (DataColumn column in csvDataTbl.Columns)
                {
                        DataColumn dataTableCsvColumn = new DataColumn();
                        dataTableCsvColumn.ColumnName = column.ColumnName;
                        DataTableCSV.Columns.Add(dataTableCsvColumn);
                        ComboBoxListOfColumns.Add(column.ColumnName);

                }

                foreach (DataRow row in csvDataTbl.Rows)
                {

                    //row[2] = "aa";
                    //row.ItemArray[3] = "koko;
                

                    //DataRowCSV DataCSV = new DataRowCSV();
                    //DataCSV.Straat = row[0].ToString();
                    //DataCSV.Nummer = row[1].ToString();
                    //DataCSV.Gemeente = row[2].ToString();
                    //DataCSV.Gemeente = "";

                    DataTableCSV.Rows.Add(row.ItemArray);
                    //DataCsvList.Add(DataCSV);
                }
            });
        }

        private async void refreshDatGrid(DataTable csvDataTbl)
        {
            await QueuedTask.Run(() =>
            {
                DataTableCSV = new DataTable();

                foreach (DataColumn column in csvDataTbl.Columns)
                {
                    DataColumn dataTableCsvColumn = new DataColumn();
                    dataTableCsvColumn.ColumnName = column.ColumnName;
                    DataTableCSV.Columns.Add(dataTableCsvColumn);
                }

                foreach (DataRow row in csvDataTbl.Rows)
                {
                    DataTableCSV.Rows.Add(row.ItemArray);
                }
            });
        }

        private void zoomToQuery(MapPoint mapPoint)
        {
            QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                var poly = GeometryEngine.Instance.Buffer(mapPoint, 50);
                mapView.ZoomTo(poly, new TimeSpan(0, 0, 0, 1));
            });
        }
        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    zoomToQuery(MapPointSelectedAddress);
                });
            }
        }

        public ICommand CmdPoint
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (MapPointSelectedAddress == null) return;
                    if (ListCSVMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y) == null)
                    {
                        ListCSVMarkeer.Add(MapPointSelectedAddress);
                        updateCSVMarkeer();
                        TextMarkeer = "Verwijder markering";
                    }
                    else
                    {
                        TextMarkeer = "Markeer";
                        MapPoint pointToDelete = ListCSVMarkeer.FirstOrDefault(m => m.X == MapPointSelectedAddress.X && m.Y == MapPointSelectedAddress.Y);
                        ListCSVMarkeer.Remove(pointToDelete);
                        GeocodeUtils.UpdateMapOverlayCSV(pointToDelete, MapView.Active, true, true);
                        updateCSVMarkeer();
                    }
                });
            }
        }

        public void updateCSVMarkeer()
        {
            foreach (MapPoint mapPoint in ListCSVMarkeer)
            {
                GeocodeUtils.UpdateMapOverlayCSV(mapPoint, MapView.Active, true);
            }
        }

        public ICommand CmdOpen
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    // if you have problem with System.Windows.Forms
                    // 1) double click on project name
                    // 2) add " <UseWindowsForms>true</UseWindowsForms> " to PropertyGroup

                    System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                    
                    openFileDialog1.RestoreDirectory = true;
                    openFileDialog1.Title = "Fichiers csv";
                    openFileDialog1.DefaultExt = "csv";
                    //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    //openFileDialog1.Filter = "fichiers csv (*.csv)|*.csv";
                    openFileDialog1.Filter = "CSV-file(*.csv)|*.csv|Text-file(*.txt)|*.txt|All Files(*.*)|*.*";
                    
                    
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        await QueuedTask.Run(() =>
                        {
                            TextFilePlacement = openFileDialog1.FileName;
                            loadCSV2table();
                        });
                    }
                });
            }
        }

        DataHandler.adresSuggestion sug;
        int straatCol;
        int huisnrCol;
        int gemeenteCol;

        private List<string> validateRow(string street, string houseNr, string municapality)
        {
            string adres;
            List<string> formatedAddresses = new List<string>();

            adres = string.Format("{0} {1}, {2}", street, houseNr, municapality);

            formatedAddresses.AddRange(sug.getAdresSuggestion(adres, 5));
            return formatedAddresses;
        }
        DataGridViewRow[] rows2validate;
        public ICommand CmdValideerAlles
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await QueuedTask.Run(() =>
                    {
                        if(SelectedGemeente == null || SelectedHuisnummer == null || SelectedStraat == null)
                        {
                            System.Windows.MessageBox.Show("U moet de waarde van de kolommen kiezen", "Error");
                            return;
                        }

                        

                        List<string> suggestions;
                        string street; string huisnr; string gemeente;

                        DataTable csvDataTbl;
                        string csvPath = TextFilePlacement;
                        System.Text.Encoding codex = System.Text.Encoding.Default;
                        if (SelectedListFormats == "UTF-8") codex = System.Text.Encoding.UTF8;
                        csvDataTbl = loadCSV2datatable(csvPath, SelectedListSeparators, 500, codex);

                        var cpt = 0;
                        var streetIndex = 0;
                        var huisnrIndex = 0;
                        var gemeenteIndex = 0;
                        foreach (DataColumn column in csvDataTbl.Columns)
                        {
                            if(SelectedStraat == column.ColumnName)
                            {
                                streetIndex = cpt;
                            }
                            if (SelectedHuisnummer == column.ColumnName)
                            {
                                huisnrIndex = cpt;
                            }
                            if (SelectedGemeente == column.ColumnName)
                            {
                                gemeenteIndex = cpt;
                            }
                            cpt++;
                        }

                        DataColumn dataTableCsvColumn2 = new DataColumn();
                        dataTableCsvColumn2.ColumnName = "Bestaan";
                        dataTableCsvColumn2.DefaultValue = "ppp";
                        csvDataTbl.Columns.Add(dataTableCsvColumn2);

                        DataTableCSV.Rows.Clear();
                        foreach (DataRow row in csvDataTbl.Rows)
                        {
                            //street = row[0].ToString();
                            //huisnr = row[1].ToString();
                            //gemeente = row[2].ToString();

                            street = row[streetIndex].ToString();
                            huisnr = row[huisnrIndex].ToString();
                            gemeente = row[gemeenteIndex].ToString();

                            suggestions = validateRow(street, huisnr, gemeente);

                            if (suggestions.Count == 0)
                            {
                                row[3] = "Nee";
                            }
                            else
                            {
                                row[3] = "Ja";
                            }
                        }
                        refreshDatGrid(csvDataTbl);
                    });
                    
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

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
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
    internal class CSVfile_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            CSVfileViewModel.Show();
        }
    }
}
