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

        private ObservableCollection<string> _listSeparators = new ObservableCollection<string>(new List<string>() {
            "Puntcomma",
            "Comma",
            "Spatie",
            "Tab",
            "Ander teken"
        });
        public ObservableCollection<string> ListSeparators
        {
            get { return _listSeparators; }
            set
            {
                SetProperty(ref _listSeparators, value);
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

        //DataGridView csvDataGrid;
        //csvDataGrid = new System.Windows.Forms.DataGridView();

        //private DataGridView _csvDataGrid = new System.Windows.Forms.DataGridView();
        //public DataGridView csvDataGrid
        //{
        //    get { return _csvDataGrid; }
        //    set
        //    {
        //        SetProperty(ref _csvDataGrid, value);
        //    }
        //}


        private ObservableCollection<DataRowCSV> _dataCsvList = new ObservableCollection<DataRowCSV>();
        public ObservableCollection<DataRowCSV> DataCsvList
        {
            get { return _dataCsvList; }
            set
            {
                SetProperty(ref _dataCsvList, value);
            }
        }

        private DataRowCSV _selectedDataCsvList;
        public DataRowCSV SelectedDataCsvList
        {
            get { return _selectedDataCsvList; }
            set
            {
                SetProperty(ref _selectedDataCsvList, value);
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
        }

        public static DataTable loadCSV2datatable(string csvPath, string separator, int maxRows, System.Text.Encoding codex)
        {
            FileInfo csv = new FileInfo(csvPath);
            string sep;
            DataTable tbl = new DataTable();

            System.Text.Encoding textEncoding = System.Text.Encoding.Default;
            if (codex != null) textEncoding = codex;

            textEncoding = Encoding.GetEncoding("iso-8859-1");

            if (!csv.Exists)
                throw new Exception("Deze csv-file bestaat niet: " + csv.Name);
            if (separator == "" || separator == null)
                throw new Exception("Deze separator is niet toegelaten");

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
            System.Text.Encoding codex = System.Text.Encoding.Default;
            if (SelectedListFormats == "UTF-8") codex = System.Text.Encoding.UTF8;

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
                System.Windows.MessageBox.Show(csvEx.Message, "Error");
                //csvErrorLbl.Text = csvEx.Message;
                return;
            }

            //set validation column
            validatedRow = new DataGridViewComboBoxColumn();
            validatedRow.HeaderText = "Gevalideerd adres";
            validatedRow.Name = "validAdres";
            validatedRow.Width = 120;

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

            //if (street == null || street == "") return formatedAddresses;

            //if ((houseNr == null || houseNr == "") && (municapality == null || municapality == ""))
            //    adres = street;
            //else if (municapality == null || municapality == "")
            //    adres = street + " " + houseNr;
            //else if (houseNr == null || houseNr == "")
            //    adres = street + ", " + municapality;
            //else
            //{
            //    adres = string.Format("{0} {1}, {2}", street, houseNr, municapality);
            //}

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
                        List<string> suggestions;
                        string street; string huisnr; string gemeente;

                        DataTable csvDataTbl;
                        string csvPath = TextFilePlacement;
                        System.Text.Encoding codex = System.Text.Encoding.Default;
                        if (SelectedListFormats == "UTF-8") codex = System.Text.Encoding.UTF8;
                        csvDataTbl = loadCSV2datatable(csvPath, SelectedListSeparators, 500, codex);
                        //foreach (DataRow row in csvDataTbl.Rows)
                        //{
                        //    DataTableCSV.Rows.Add(row.ItemArray);
                        //}

                        //DataGridViewRow[] rows = (DataGridViewRow[])DataTableCSV;


                        rows2validate = new DataGridViewRow[csvDataTbl.Rows.Count];
                        csvDataTbl.Rows.CopyTo(rows2validate, 0);

                        foreach (DataGridViewCell cel in rows2validate[1].Cells)
                        {
                            Debug.WriteLine(cel);
                            //cel.Style.BackColor = clr;
                        }

                        Debug.WriteLine("!!!!!!!!! PARAMETERS !!!!!!!!!!!!!!");
                        foreach (DataRow row in csvDataTbl.Rows)
                        {
                            Debug.WriteLine($@":: {row}");
                            
                            street = row[0].ToString();
                            huisnr = row[1].ToString();
                            gemeente = row[2].ToString();

                            suggestions = validateRow(street, huisnr, gemeente);

                            

                            if (suggestions.Count == 0)
                            {
                                Debug.WriteLine($@"{street} :: NOT FOUND");
                                
                            }
                            else
                            {
                                Debug.WriteLine($@"{street} :: OKAY");
                            }
                        }
                        Debug.WriteLine("!!!!!!!!! END !!!!!!!!!!!!!!");

                        
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
