using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
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
            CopyDisplayNameCommand = new Command(DoCopyDisplayNameCommand, false);
            ShowFilePropertiesCommand = new Command(DoShowFilePropertiesCommand, false);
            UninstallAssemblyCommand = new Command(() => { }, false);
            OpenAssemblyLocationCommand = new Command(() => {}, false);
            InstallAssemblyCommand = new Command(() => {});

            //  Update Gac Util Status info.
            GacUtilStatusInfo = GacUtil.CanFindExecutable() ? "GacUtil is installed" :
                "Cannot find GacUtil";

        }

        /// <summary>
        /// Performs the RefreshAssemblies command.
        /// </summary>
        /// <param name="parameter">The RefreshAssemblies command parameter.</param>
        private void DoRefreshAssembliesCommand(object parameter)
        {
            //  Clear the assemblies.
            Assemblies.Clear();

            //  Set the status text.
            RefreshAssembliesCommand.ReportProgress(
                () =>
                    {
                        StatusInfo = "Loading Assemblies...";
                    });
            
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

            AssembliesCollectionView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
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
        /// The NotifyingProperty for the GacUtilStatusInfo property.
        /// </summary>
        private readonly NotifyingProperty GacUtilStatusInfoProperty =
          new NotifyingProperty("GacUtilStatusInfo", typeof(string), default(string));

        /// <summary>
        /// Gets or sets GacUtilStatusInfo.
        /// </summary>
        /// <value>The value of GacUtilStatusInfo.</value>
        public string GacUtilStatusInfo
        {
            get { return (string)GetValue(GacUtilStatusInfoProperty); }
            set { SetValue(GacUtilStatusInfoProperty, value); }
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
                ShowFilePropertiesCommand.CanExecute = value != null;
                CopyDisplayNameCommand.CanExecute = value != null;
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
        public Command OpenAssemblyLocationCommand { get; private set; }/// <summary>
        /// Performs the CopyDisplayName command.
        /// </summary>
        /// <param name="parameter">The CopyDisplayName command parameter.</param>
        private void DoCopyDisplayNameCommand(object parameter)
        {
            var assembly = (GACAssemblyViewModel)parameter;
            Clipboard.SetText(assembly.InternalAssemblyDescription.DisplayName);
        }

        /// <summary>
        /// Gets the CopyDisplayName command.
        /// </summary>
        /// <value>The value of .</value>
        public Command CopyDisplayNameCommand
        {
            get;
            private set;
        }



        /// <summary>
        /// Performs the ShowFileProperties command.
        /// </summary>
        /// <param name="parameter">The ShowFileProperties command parameter.</param>
        private void DoShowFilePropertiesCommand(object parameter)
        {
            var assembly = (GACAssemblyViewModel) parameter;
            Interop.Shell.Shell32.ShowFileProperties(assembly.InternalAssemblyDescription.Path);
        }

        /// <summary>
        /// Gets the ShowFileProperties command.
        /// </summary>
        /// <value>The value of .</value>
        public Command ShowFilePropertiesCommand
        {
            get;
            private set;
        }
    }
}
