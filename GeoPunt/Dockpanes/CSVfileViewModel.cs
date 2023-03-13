﻿using ArcGIS.Core.CIM;
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
using System.Diagnostics;
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



        private const string _dockPaneID = "GeoPunt_Dockpanes_CSVfile";

        protected CSVfileViewModel() 
        {
            //DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
           
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
                    openFileDialog1.Title = "Fichiers raw";
                    openFileDialog1.DefaultExt = "raw";
                    //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    openFileDialog1.Filter = "fichiers raw (*.raw)|*.raw";
                    openFileDialog1.Multiselect = true;
                    
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        foreach (String file in openFileDialog1.FileNames)
                        {
                            ListBoxItem listBoxItem = new ListBoxItem();
                            listBoxItem.Content = file;
                            //RawListBox.Add(listBoxItem);
                        }
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
