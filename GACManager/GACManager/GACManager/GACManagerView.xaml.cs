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
using Microsoft.Win32;
using GACManagerApi.Fusion;
using GACManagerApi;

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
            ViewModel.InstallAssemblyCommand.Executed += new Apex.MVVM.CommandEventHandler(InstallAssemblyCommand_Executed);
            ViewModel.UninstallAssemblyCommand.Executed += new Apex.MVVM.CommandEventHandler(UninstallAssemblyCommand_Executed);
        }

        void UninstallAssemblyCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            //  The parameter must be an assembly.
            var assemblyViewModel = (GACAssemblyViewModel)args.Parameter;

            //  Create an assembly cache.
            IASSEMBLYCACHE_UNINSTALL_DISPOSITION disposition = IASSEMBLYCACHE_UNINSTALL_DISPOSITION.Unknown;
            AssemblyCache.UninstallAssembly(assemblyViewModel.InternalAssemblyDescription.DisplayName,
                null, out disposition);

            //  Depending on the result, show the appropriate message.
            string message = string.Empty;
            switch (disposition)
            {
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.Unknown:
                    message = "Failed to uninstall assembly.";
                    break;
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED:
                    message = "The assembly was uninstalled successfully!";
                    break;
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_STILL_IN_USE:
                    message = "Cannot uninstall this assembly - it is in use.";
                    break;
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED:
                    message = "Cannot uninstall this assembly - it has already been uninstalled.";
                    break;
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_DELETE_PENDING:
                    message = "Cannot uninstall this assembly - it has has a delete pending.";
                    break;
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_HAS_INSTALL_REFERENCES:
                    message = "Cannot uninstall this assembly - it was installed as part of another product.";
                    break;
                case IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_REFERENCE_NOT_FOUND:
                    message = "Cannot uninstall this assembly - cannot find the assembly.";
                    break;
                default:
                    break;
            }

            //  Show the message box.
            MessageBox.Show(message, "Uninstall");
        }

        void InstallAssemblyCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            //  Create an open file dialog.
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Assemblies (*.dll)|*.dll";
            if (openFileDialog.ShowDialog() == true)
            {
                //  Get the assembly path.
                var assemblyPath = openFileDialog.FileName;

                //  Install the assembly.
                try
                {
                    AssemblyCache.InstallAssembly(assemblyPath, null, GACManagerApi.Fusion.AssemblyCommitFlags.Force);
                }
                catch (AssemblyMustBeStronglyNamedException)
                {
                    MessageBox.Show("Failed to install the assembly - it is not strongly named.", "Install");
                }
                catch
                {
                    MessageBox.Show("Failed to install the assembly.", "Install");
                }
            }
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
            get { return (GACManagerViewModel) this.FindResource("MainViewModel"); }
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
