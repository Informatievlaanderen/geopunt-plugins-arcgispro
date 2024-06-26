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

namespace GeoPunt
{
    /// <summary>
    /// Interaction logic for ProWindowInfo.xaml
    /// </summary>
    public partial class ProWindowInfo : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProWindowInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Open Hyperlink in default browser when clicked.
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
    }
}
