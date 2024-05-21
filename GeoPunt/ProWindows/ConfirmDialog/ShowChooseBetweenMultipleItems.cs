using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeoPunt.ProWindows.ConfirmDialog
{
    internal class ShowChooseBetweenMultipleItems : Button
    {

        private ChooseBetweenMultipleItems _choosebetweenmultipleitems = null;
        private Action<string> confirmAction;


        public ShowChooseBetweenMultipleItems(string title, Dictionary<string, string> multipleItems)
        {
            ProWindowTitle = title;
            MultipleChooseSource = new ObservableDictionary<string, string>(multipleItems);
        }

        public ShowChooseBetweenMultipleItems(string title, Dictionary<string, string> multipleItems, Action<string> addWFSToMap) : this(title, multipleItems)
        {
            confirmAction = addWFSToMap;
        }


        

        private string _proWindowTitle;
        public string ProWindowTitle
        {
            get { return _proWindowTitle; }
            set
            {
                SetProperty(ref _proWindowTitle, value);
            }
        }


        private ObservableDictionary<string, string> _multipleChooseSource = new ObservableDictionary<string, string>();
        public ObservableDictionary<string, string> MultipleChooseSource
        {
            get { return _multipleChooseSource; }
            set
            {
                SetProperty(ref _multipleChooseSource, value);
                ServiceIsSelected = false;
            }
        }
        private KeyValuePair<string, string> _selectedDataSource;
        public KeyValuePair<string, string> SelectedDataSource
        {
            get { return _selectedDataSource; }
            set
            {
                
                SetProperty(ref _selectedDataSource, value);
                if(MultipleChooseSource.ContainsKey(SelectedDataSource.Key))
                {
                    ServiceIsSelected = true;
                }
                else
                {
                    ServiceIsSelected = false;
                }
            }
        }

        private bool _serviceIsSelected = false;
        public bool ServiceIsSelected
        {
            get { return _serviceIsSelected; }
            set
            {
                SetProperty(ref _serviceIsSelected, value);
            }
        }



        public Action<string> ConfirmAction
        {
            get { return confirmAction; }
            set { confirmAction = value; }
        }

        public ICommand CmdConfirm
        {

            get
            {
                return new RelayCommand(() => {

                    confirmAction(SelectedDataSource.Key);
                });
            }
        }




        protected override void OnClick()
        {

            //already open?
            if (_choosebetweenmultipleitems != null)
                return;
            _choosebetweenmultipleitems = new ChooseBetweenMultipleItems();
            _choosebetweenmultipleitems.Owner = FrameworkApplication.Current.MainWindow;
            _choosebetweenmultipleitems.Closed += (o, e) => { _choosebetweenmultipleitems = null; };
            _choosebetweenmultipleitems.Show();
            //uncomment for modal
            //_choosebetweenmultipleitems.ShowDialog();
        }

    }
}
