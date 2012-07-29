using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Apex;

namespace GACManager
{
    /// <summary>
    /// Interaction logic for AssemblyView.xaml
    /// </summary>
    public partial class AssemblyView : UserControl
    {
        public AssemblyView()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            ApexBroker.GetShell().ClosePopup(this, null);
        }
    }
}
