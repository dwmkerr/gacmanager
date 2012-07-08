using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Apex;
using Apex.MVVM;
using GACManager.Models;
using GACManagerApi;
using GACManagerApi.Fusion;

namespace GACManager
{
    [ViewModel]
    public class GACManagerViewModel : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GACManagerViewModel"/> class.
        /// </summary>
        public GACManagerViewModel()
        {
            //  Create the refresh assemblies command.
            RefreshAssembliesCommand = new AsynchronousCommand(DoRefreshAssembliesCommand, true);
            UninstallAssemblyCommand = new Command(DoUninstallAssemblyCommand, false);
            OpenAssemblyLocationCommand = new Command(() => {}, false);
            InstallAssemblyCommand = new Command(() => {});

        }

        /// <summary>
        /// Performs the RefreshAssemblies command.
        /// </summary>
        /// <param name="parameter">The RefreshAssemblies command parameter.</param>
        private void DoRefreshAssembliesCommand(object parameter)
        {
            Assemblies.Clear();

            //  Set the status text.
            RefreshAssembliesCommand.ReportProgress(
                () => { StatusInfo = "Loading Assemblies..."; });
            
            //  Start the enumeration.
            var timeTaken = ApexBroker.GetModel<IGACManagerModel>().EnumerateAssemblies(
                (assemblyDetails) =>
                    {
                        //  Create an assembly view model from the detials.
                        var viewModel = new GACAssemblyViewModel();
                        viewModel.FromModel(assemblyDetails);

                        //  Add it to the collection.
                        Assemblies.Add(viewModel);
                        
                    });

            //  Set the resulting status info.
            RefreshAssembliesCommand.ReportProgress(
                () =>
                    {
            AssembliesCollectionView = new ListCollectionView(Assemblies.ToList());

            AssembliesCollectionView.SortDescriptions.Add(new SortDescription("FullName", ListSortDirection.Ascending));
                        AssembliesCollectionView.Filter += Filter;
                        StatusInfo = "Loaded " + Assemblies.Count + " assemblies in " + timeTaken.TotalMilliseconds +
                                     " milliseconds";
                        
                    });


        }

        private bool Filter(object o)
        {
            var assemblyViewModel = o as GACAssemblyViewModel;
            if (assemblyViewModel == null)
                return false;

            return string.IsNullOrEmpty(SearchText) ||
                assemblyViewModel.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1 ||
                assemblyViewModel.PublicKeyToken.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>
        /// The Assemblies observable collection.
        /// </summary>
        private SafeObservableCollection<GACAssemblyViewModel> AssembliesProperty =
          new SafeObservableCollection<GACAssemblyViewModel>();

        /// <summary>
        /// Gets the Assemblies observable collection.
        /// </summary>
        /// <value>The Assemblies observable collection.</value>
        public SafeObservableCollection<GACAssemblyViewModel> Assemblies
        {
            get { return AssembliesProperty; }
        }

        
        /// <summary>
        /// The NotifyingProperty for the StatusInfo property.
        /// </summary>
        private readonly NotifyingProperty StatusInfoProperty =
          new NotifyingProperty("StatusInfo", typeof(string), default(string));

        /// <summary>
        /// Gets or sets StatusInfo.
        /// </summary>
        /// <value>The value of StatusInfo.</value>
        public string StatusInfo
        {
            get { return (string)GetValue(StatusInfoProperty); }
            set { SetValue(StatusInfoProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the SelectedAssembly property.
        /// </summary>
        private readonly NotifyingProperty SelectedAssemblyProperty =
          new NotifyingProperty("SelectedAssembly", typeof(GACAssemblyViewModel), default(GACAssemblyViewModel));

        /// <summary>
        /// Gets or sets SelectedAssembly.
        /// </summary>
        /// <value>The value of SelectedAssembly.</value>
        public GACAssemblyViewModel SelectedAssembly
        {
            get { return (GACAssemblyViewModel)GetValue(SelectedAssemblyProperty); }
            set 
            { 
                SetValue(SelectedAssemblyProperty, value);
                UninstallAssemblyCommand.CanExecute = value != null;
                OpenAssemblyLocationCommand.CanExecute = value != null;
                if(SelectedAssembly != null)
                    SelectedAssembly.LoadExtendedPropertiesCommand.DoExecute(null);
            }
        }

        
        /// <summary>
        /// The NotifyingProperty for the AssembliesCollectionView property.
        /// </summary>
        private readonly NotifyingProperty AssembliesCollectionViewProperty =
          new NotifyingProperty("AssembliesCollectionView", typeof(CollectionView), default(CollectionView));

        /// <summary>
        /// Gets or sets AssembliesCollectionView.
        /// </summary>
        /// <value>The value of AssembliesCollectionView.</value>
        public CollectionView AssembliesCollectionView
        {
            get { return (CollectionView)GetValue(AssembliesCollectionViewProperty); }
            set { SetValue(AssembliesCollectionViewProperty, value); }
        }

        /// <summary>
        /// Gets the RefreshAssemblies command.
        /// </summary>
        /// <value>The value of the RefreshAssemblies Command.</value>
        public AsynchronousCommand RefreshAssembliesCommand
        {
            get;
            private set;
        }

        
        /// <summary>
        /// The NotifyingProperty for the SearchText property.
        /// </summary>
        private readonly NotifyingProperty SearchTextProperty =
          new NotifyingProperty("SearchText", typeof(string), default(string));

        /// <summary>
        /// Gets or sets SearchText.
        /// </summary>
        /// <value>The value of SearchText.</value>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set 
            { 
                SetValue(SearchTextProperty, value);
                if(AssembliesCollectionView != null)
                    AssembliesCollectionView.Refresh();
            }
        }

        

        /// <summary>
        /// Performs the UninstallAssembly command.
        /// </summary>
        /// <param name="parameter">The UninstallAssembly command parameter.</param>
        private void DoUninstallAssemblyCommand(object parameter)
        {
            //  The parameter must be an assembly.
            var assemblyViewModel = (GACAssemblyViewModel)parameter;

            //  Create an assembly cache.
            IASSEMBLYCACHE_UNINSTALL_DISPOSITION disposition = IASSEMBLYCACHE_UNINSTALL_DISPOSITION.Unknown;
            AssemblyCache.UninstallAssembly(assemblyViewModel.InternalAssemblyDetails.QualifiedAssemblyName,
                assemblyViewModel.InternalAssemblyDetails.InstallReferences.FirstOrDefault(), out disposition);
        }

        /// <summary>
        /// Gets the UninstallAssembly command.
        /// </summary>
        /// <value>The value of .</value>
        public Command UninstallAssemblyCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the InstallAssembly command.
        /// </summary>
        /// <value>The value of .</value>
        public Command InstallAssemblyCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the open assembly location command.
        /// </summary>
        public Command OpenAssemblyLocationCommand { get; private set; }
    }
}
