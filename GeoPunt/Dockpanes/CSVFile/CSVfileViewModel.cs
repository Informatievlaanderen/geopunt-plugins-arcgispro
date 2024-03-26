using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using GeoPunt.DataHandler;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GeoPunt.Dockpanes.CSVFile
{
    internal class CSVfileViewModel : DockPane
    {

        private Helpers.Utils utils = new Helpers.Utils();
        private SpatialReference lambertSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(31370);
        adresLocation adresLocation;
        MapPoint MapPointSelectedAddress = null;

        private const string defaultPlaceHolder = "Nog geen bestand gekozen";

        private const string _dockPaneID = "GeoPunt_Dockpanes_CSVfile";
        //BackgroundWorker validationWorker;
        protected CSVfileViewModel()
        {
            //validationWorker = new System.ComponentModel.BackgroundWorker();
            //DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            sug = new adresSuggestion(5000);
            adresLocation = new adresLocation(5000);
            TextMarkeer = "Markeer";

            ListSeparators = new ObservableCollection<string>(new List<string>() {
                "Puntkomma",
                "Komma",
                "Spatie",
                "Tab",
                /* "Ander teken" */
            });



            SelectedSeparator = ListSeparators[0];


            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            CheckMapViewIsActive();
        }

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("ActiveMapViewChangedTriggered");

            CheckMapViewIsActive();

        }

        private void CheckMapViewIsActive()
        {
            if (MapView.Active != null)
            {
                MapViewIsActive = true;
            }
            else
            {
                MapViewIsActive = false;
            }
        }

        private bool _mapViewIsActive;
        public bool MapViewIsActive
        {
            get { return _mapViewIsActive; }
            set
            {
                SetProperty(ref _mapViewIsActive, value);
            }
        }

        protected override void OnShow(bool isVisible)
        {
            if (!isVisible)
            {
                GeocodeUtils.RemoveFromMapOverlayTemp();

                ListCSVMarkeer.Clear();
                TextMarkeer = "Markeer";
                updateCSVMarkeer();
            }
        }



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

        private bool _isCorrectAddress = false;
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

        private string _textFilePlacement = defaultPlaceHolder;
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

        private string _selectedSeparator;
        public string SelectedSeparator
        {
            get { return _selectedSeparator; }
            set
            {
                SetProperty(ref _selectedSeparator, value);

                if (SelectedSeparator != null && TextFilePlacement != null && TextFilePlacement != defaultPlaceHolder)
                {
                    QueuedTask.Run(() =>
                    {
                        loadCSV2table();
                    });
                }
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
                if (_selectedDataCsvList != null)
                {
                    SelectedDataCsvListIsSelected = true;
                }
                else
                {
                    SelectedDataCsvListIsSelected = false;
                }
                SelectedDataRowChanged();

            }
        }

        private bool _selectedDataCsvListIsSelected = false;
        public bool SelectedDataCsvListIsSelected
        {
            get { return _selectedDataCsvListIsSelected; }
            set
            {
                SetProperty(ref _selectedDataCsvListIsSelected, value);


            }
        }

        private void SelectedDataRowChanged()
        {
            IsCorrectAddress = false;
            if (SelectedDataCsvList != null)
            {

                if (SelectedStraat != null && SelectedGemeente != null)
                {

                    string optionalHuisnummer = SelectedHuisnummer != null ? _selectedDataCsvList.Row[SelectedHuisnummer].ToString() : "";
                    string var = $@"{_selectedDataCsvList.Row[SelectedStraat]} {optionalHuisnummer} ,{_selectedDataCsvList.Row[SelectedGemeente]}";

                    string valCorrect = "" + _selectedDataCsvList.Row.ItemArray[3];

                    if (_selectedDataCsvList.Row[Bestaan] != null && _selectedDataCsvList.Row[Bestaan].ToString() == "OK")
                    {
                        IsCorrectAddress = true;
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
            MapPointSelectedAddress = utils.CreateMapPoint(x, y, lambertSpatialReference);
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
                SelectedDataRowChanged();
                if (_dataTableCSV != null && _dataTableCSV.Rows.Count > 0)
                {
                    DataTableCSVHasItems = true;
                }
                else
                {
                    DataTableCSVHasItems = false;
                }
            }
        }

        private bool _dataTableCSVHasItems = false;
        public bool DataTableCSVHasItems
        {
            get { return _dataTableCSVHasItems; }
            set
            {
                SetProperty(ref _dataTableCSVHasItems, value);

            }
        }


        private string _bestaan = "Gevalideerd adres";
        public string Bestaan
        {
            get { return _bestaan; }
            set
            {
                SetProperty(ref _bestaan, value);
            }
        }




        public static DataTable loadCSV2datatable(string csvPath, string separator, int maxRows, Encoding codex)
        {
            FileInfo csv = new FileInfo(csvPath);
            string sep;
            DataTable tbl = new DataTable();

            Encoding textEncoding = Encoding.Default;
            if (codex != null) textEncoding = codex;

            textEncoding = Encoding.GetEncoding("iso-8859-1");

            //if (!csv.Exists)
            //    throw new Exception("Deze csv-file bestaat niet: " + csv.Name);
            //if (separator == "" || separator == null)
            //    throw new Exception("Deze separator is niet toegelaten");

            // separator = "Puntcomma";

            switch (separator)
            {
                case "Komma":
                    sep = ",";
                    break;
                case "Puntkomma":
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
            try
            {

                DataTableCSV = new DataTable();
                Encoding codex = Encoding.UTF8;
                //if (SelectedListFormats == "UTF-8") codex = ;

                string csvPath = TextFilePlacement;

                DataTable csvDataTbl;


                try
                {
                    int maxRowCount = 500;
                    csvDataTbl = loadCSV2datatable(csvPath, SelectedSeparator, maxRowCount, codex);

                    if (csvDataTbl.Rows.Count == maxRowCount)
                    {
                        string msg = string.Format(
                          "Maximaal aantal van {0} rijen overschreden, enkel de eerste {0} rijen worden getoond.", maxRowCount);
                        System.Windows.MessageBox.Show(msg, "Maximaal aantal rijen overschreden.");
                        //csvErrorLbl.Text = msg;
                    }
                }
                catch (Exception csvEx)
                {

                    string message = "Er is een fout opgetreden bij het verwerken van het CSV-bestand. Controleer of het CSV-bestand geldig is en probeer het opnieuw.";
                    System.Windows.MessageBox.Show(message, "Error");

                    return;
                }

                ComboBoxListOfColumns = new ObservableCollection<string>(new List<string>());

                await QueuedTask.Run(() =>
                {
                    DataTableCSV = new DataTable();


                    DataColumn dataTableCsvColumn2 = new DataColumn();
                    dataTableCsvColumn2.ColumnName = Bestaan;
                    dataTableCsvColumn2.DefaultValue = "";
                    dataTableCsvColumn2.ReadOnly = false;
                    csvDataTbl.Columns.Add(dataTableCsvColumn2);


                    foreach (DataColumn column in csvDataTbl.Columns)
                    {
                        DataColumn dataTableCsvColumn = new DataColumn();
                        dataTableCsvColumn.ColumnName = column.ColumnName;
                        DataTableCSV.Columns.Add(dataTableCsvColumn);
                        if (column.ColumnName != Bestaan)
                            ComboBoxListOfColumns.Add(column.ColumnName);

                    }

                    foreach (DataRow row in csvDataTbl.Rows)
                    {
                        DataTableCSV.Rows.Add(row.ItemArray);
                    }
                    if (DataTableCSV.Rows.Count > 0)
                    {
                        DataTableCSVHasItems = true;
                    }
                });
            }
            catch (Exception ex)
            {
                string message = "Er is een fout opgetreden bij het verwerken van het CSV-bestand. Controleer of het CSV-bestand geldig is en probeer het opnieuw.";
                System.Windows.MessageBox.Show(message, "Error");

            }
        }



        public ICommand CmdZoom
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    utils.ZoomTo(MapPointSelectedAddress);
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

                    try
                    {


                        System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();

                        openFileDialog1.RestoreDirectory = true;
                        openFileDialog1.Title = "Fichiers csv";
                        openFileDialog1.DefaultExt = "csv";
                        //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                        //openFileDialog1.Filter = "fichiers csv (*.csv)|*.csv";
                        //openFileDialog1.Filter = "CSV-file(*.csv)|*.csv|Text-file(*.txt)|*.txt|All Files(*.*)|*.*";
                        openFileDialog1.Filter = "CSV-file(*.csv)|*.csv|Text-file(*.txt)|*.txt";


                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            await QueuedTask.Run(() =>
                            {
                                TextFilePlacement = openFileDialog1.FileName;
                                loadCSV2table();
                            });
                        }
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message + " : " + ex.StackTrace, "Error");
                    }
                });
            }
        }

        adresSuggestion sug;
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
                        if (SelectedGemeente == null /* || SelectedHuisnummer == null */ || SelectedStraat == null)
                        {
                            System.Windows.MessageBox.Show("U moet de waarde van de kolommen kiezen", "Error");
                            return;
                        }

                        try
                        {


                            List<string> suggestions;
                            string street; string huisnr; string gemeente;



                            var cpt = 0;
                            var streetIndex = 0;
                            var huisnrIndex = 0;
                            var gemeenteIndex = 0;

                            foreach (DataColumn column in DataTableCSV.Columns)
                            {
                                if (SelectedStraat == column.ColumnName)
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


                            //DataColumn dataTableCsvColumn2 = new DataColumn();
                            //dataTableCsvColumn2.ColumnName = "Bestaan";
                            //dataTableCsvColumn2.DefaultValue = "";
                            //csvDataTbl.Columns.Add(dataTableCsvColumn2);

                            // DataTableCSV.Rows.Clear();
                            foreach (DataRow row in DataTableCSV.Rows)
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
                                    row[Bestaan] = "NOK";
                                }
                                else
                                {
                                    row[Bestaan] = "OK";
                                }
                            }
                            // refreshDatGrid(csvDataTbl);
                            SelectedDataRowChanged();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error");
                        }
                    });

                });
            }
        }

        public ICommand CmdValideer
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await QueuedTask.Run(() =>
                    {
                        if (SelectedGemeente == null /* || SelectedHuisnummer == null */ || SelectedStraat == null)
                        {
                            System.Windows.MessageBox.Show("U moet de waarde van de kolommen kiezen", "Error");
                            return;
                        }

                        try
                        {

                            List<string> suggestions;
                            string street; string huisnr; string gemeente;


                            var cpt = 0;
                            var streetIndex = 0;
                            var huisnrIndex = 0;
                            var gemeenteIndex = 0;
                            foreach (DataColumn column in DataTableCSV.Columns)
                            {
                                if (SelectedStraat == column.ColumnName)
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


                            //DataColumn dataTableCsvColumn2 = new DataColumn();
                            //dataTableCsvColumn2.ColumnName = "Bestaan";
                            //dataTableCsvColumn2.DefaultValue = "";
                            //csvDataTbl.Columns.Add(dataTableCsvColumn2);


                            // DataTableCSV.Rows.Clear();

                            //street = row[0].ToString();
                            //huisnr = row[1].ToString();
                            //gemeente = row[2].ToString();

                            street = SelectedDataCsvList[streetIndex].ToString();
                            huisnr = SelectedDataCsvList[huisnrIndex].ToString();
                            gemeente = SelectedDataCsvList[gemeenteIndex].ToString();

                            suggestions = validateRow(street, huisnr, gemeente);

                            if (suggestions.Count == 0)
                            {
                                SelectedDataCsvList[Bestaan] = "NOK";
                            }
                            else
                            {
                                SelectedDataCsvList[Bestaan] = "OK";
                            }

                            SelectedDataCsvList = null;

                            // refreshDatGrid(csvDataTbl);
                            SelectedDataRowChanged();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error");
                        }
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
