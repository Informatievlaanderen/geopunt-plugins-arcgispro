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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

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

        private DataGridView _csvDataGrid = new System.Windows.Forms.DataGridView();
        public DataGridView csvDataGrid
        {
            get { return _csvDataGrid; }
            set
            {
                SetProperty(ref _csvDataGrid, value);
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

        protected CSVfileViewModel() 
        {
            //DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
  
        }

        public static DataTable loadCSV2datatable(string csvPath, string separator, int maxRows, System.Text.Encoding codex)
        {
            FileInfo csv = new FileInfo(csvPath);
            string sep;
            DataTable tbl = new DataTable();

            System.Text.Encoding textEncoding = System.Text.Encoding.Default;
            if (codex != null) textEncoding = codex;

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

            //if (!File.Exists(csvPath) || SelectedListSeparators == "" || SelectedListSeparators == null) return;

            DataTable csvDataTbl;
            //DataCsvList.Clear();
            //DataGridView csvDataGrid;
            //csvDataGrid = new System.Windows.Forms.DataGridView();

            //DataTableCSV.Columns.Clear();

            //csvErrorLbl.Text = "";
            ////clear all the stuff

            //adresColCbx.Items.Clear();
            //adresColCbx.Items.Add("");
            //HuisNrCbx.Items.Clear();
            //HuisNrCbx.Items.Add("");
            //gemeenteColCbx.Items.Clear();
            //gemeenteColCbx.Items.Add("");

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
                //csvDataGrid.Columns.Add(column.ColumnName, column.ColumnName);
                //csvDataGrid.Columns[column.ColumnName].SortMode = DataGridViewColumnSortMode.Automatic;

                

                
                    DataColumn dataTableCsvColumn = new DataColumn();
                    dataTableCsvColumn.ColumnName = column.ColumnName;
                    DataTableCSV.Columns.Add(dataTableCsvColumn);
                

                //adresColCbx.Items.Add(column.ColumnName);
                //HuisNrCbx.Items.Add(column.ColumnName);
                //gemeenteColCbx.Items.Add(column.ColumnName);
            }
            //csvDataGrid.Columns.Add(validatedRow);

            foreach (DataRow row in csvDataTbl.Rows)
            {
                //csvDataGrid.Rows.Add(row.ItemArray);
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
                    
                    // openFileDialog1.InitialDirectory = @"C:\";

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

                        //Debug.WriteLine("!!!!!!!!!!! PARAMETERS !!!!!!!!!!!!!!");
                        //Debug.WriteLine("hello");
                        //Debug.WriteLine("!!!!!!!!!!! END !!!!!!!!!!!!!!");
                    }
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
    internal class CSVfile_ShowButton : System.Windows.Controls.Button
    {
        protected override void OnClick()
        {
            CSVfileViewModel.Show();
        }
    }
}
