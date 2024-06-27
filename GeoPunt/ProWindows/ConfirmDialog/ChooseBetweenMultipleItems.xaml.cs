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

namespace GeoPunt.ProWindows.ConfirmDialog
{
    /// <summary>
    /// Interaction logic for ChooseBetweenMultipleItems.xaml
    /// </summary>
    public partial class ChooseBetweenMultipleItems : ArcGIS.Desktop.Framework.Controls.ProWindow
    {

        public ChooseBetweenMultipleItems()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
