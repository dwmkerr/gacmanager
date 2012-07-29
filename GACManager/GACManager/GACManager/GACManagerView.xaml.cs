using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            ViewModel.UninstallAssemblyCommand.Executing += new Apex.MVVM.CancelCommandEventHandler(UninstallAssemblyCommand_Executing);
            ViewModel.UninstallAssemblyCommand.Executed += new Apex.MVVM.CommandEventHandler(UninstallAssemblyCommand_Executed);
            ViewModel.HelpCommand.Executed += new Apex.MVVM.CommandEventHandler(HelpCommand_Executed);
            ViewModel.ShowAssemblyDetailsCommand.Executed += new Apex.MVVM.CommandEventHandler(ShowAssemblyDetailsCommand_Executed);
        }

        void ShowAssemblyDetailsCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            //  Get the assembly view model.
            var assemblyViewModel = args.Parameter as GACAssemblyViewModel;

            //  If we don't have one, bail.
            if (assemblyViewModel == null)
                return;

            //  Create a new assembly details window.
            var assemblyDetailsWindow = new AssemblyDetails.AssemblyDetailsWindow();
            assemblyDetailsWindow.AssemblyViewModel = assemblyViewModel;

            //  Show the window.
            assemblyDetailsWindow.ShowDialog();
        }

        void HelpCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            System.Diagnostics.Process.Start(Properties.Resources.ProjectHomePageUrl);
        }

        void UninstallAssemblyCommand_Executing(object sender, Apex.MVVM.CancelCommandEventArgs args)
        {
            //  Double check with the user.
            args.Cancel = MessageBox.Show("Are you sure you want to uninstall this assembly?", "Are you sure?",
                                          MessageBoxButton.YesNoCancel)
                          != MessageBoxResult.Yes;
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

            //  Remove the assembly from the vm.
            ViewModel.Assemblies.Remove(assemblyViewModel);
            ViewModel.AssembliesCollectionView.Refresh();

            //  Show the message box.
            MessageBox.Show(message, "Uninstall");
        }

        void InstallAssemblyCommand_Executed(object sender, Apex.MVVM.CommandEventArgs args)
        {
            //  Create an open file dialog.
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Assembly to Install";
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

                //  Load the assembly.


                //var enumerator =
                //    new AssemblyCacheEnumerator(System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                //var assembly = enumerator.GetNextAssembly();
                //var vm = new GACAssemblyViewModel();
                //vm.FromModel(new AssemblyDescription(assembly));
                //ViewModel.Assemblies.Add(vm);
                //ViewModel.AssembliesCollectionView.
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
            ViewModel.ShowAssemblyDetailsCommand.DoExecute(((FrameworkElement)e.OriginalSource).DataContext as GACAssemblyViewModel);
        }
    }
}
