﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace GeoPunt.Dockpanes.Gipod
{
    /// <summary>
    /// Interaction logic for GipodView.xaml
    /// </summary>
    public partial class GipodView : UserControl
    {
        public GipodView()
        {
            InitializeComponent();
        }

        private void DataGridHyperlinkColumn_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
    }
}
