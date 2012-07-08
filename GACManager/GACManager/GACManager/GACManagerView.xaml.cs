using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GACManager.Popups;

namespace GACManager
{
    /// <summary>
    /// Interaction logic for GACManagerView.xaml
    /// </summary>
    public partial class GACManagerView : UserControl
    {
        public GACManagerView()
        {
            InitializeComponent();

            //  If RefreshOnStartup is set, we can refresh now.
            if(Properties.Settings.Default.RefreshOnStartup)
                ViewModel.RefreshAssembliesCommand.DoExecute(null);

            //  Wait for key commands that we can handle but a viewmodel can't.
            ViewModel.OpenAssemblyLocationCommand.Executed += new Apex.MVVM.CommandEventHandler(OpenAssemblyLocationCommand_Executed);
        }

        /// <summary>
        /// Handles the Executed event of the OpenAssemblyLocationCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.</param>
        void OpenAssemblyLocationCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            //  Get the assembly.
            var assemblyViewModel = (GACAssemblyViewModel) args.Parameter;
            
            //  Try and open it's path.
            try
            {
                Process.Start(System.IO.Path.GetDirectoryName(assemblyViewModel.Path));
            }
            catch (Exception exception)
            {
                Trace.WriteLine("Failed to open directory: " + exception);
            }
        }

        public GACManagerViewModel ViewModel
        {
            get { return DataContext as GACManagerViewModel; }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as GACAssemblyViewModel;
            if (item != null)
            {
                ApexBroker.GetShell().ShowPopup(new AssemblyView() {DataContext = item});
            }
        }
    }
}
