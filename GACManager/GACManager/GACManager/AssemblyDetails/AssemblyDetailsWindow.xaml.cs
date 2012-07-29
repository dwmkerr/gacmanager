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
using System.Windows.Shapes;

namespace GACManager.AssemblyDetails
{
    /// <summary>
    /// Interaction logic for AssemblyDetailsWindow.xaml
    /// </summary>
    public partial class AssemblyDetailsWindow : Window
    {
        public AssemblyDetailsWindow()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(AssemblyDetailsWindow_Loaded);
        }

        void AssemblyDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            assemblyView.DataContext = AssemblyViewModel;
        }

        public GACAssemblyViewModel AssemblyViewModel { get; set; }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
