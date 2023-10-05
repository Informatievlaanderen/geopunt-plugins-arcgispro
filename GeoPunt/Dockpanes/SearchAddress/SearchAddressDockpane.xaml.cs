using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace GeoPunt.Dockpanes.SearchAddress
{
    /// <summary>
    /// Interaction logic for SearchAddressDockpaneView.xaml
    /// </summary>
    public partial class SearchAddressDockpaneView : UserControl
    {
        public SearchAddressDockpaneView()
        {
            InitializeComponent();
            comboboxCities.IsDropDownOpen = true;
        }

   
    }
}
