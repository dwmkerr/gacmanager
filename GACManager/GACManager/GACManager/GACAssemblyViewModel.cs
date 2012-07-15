using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Apex;
using Apex.MVVM;
using GACManager.Models;
using GACManagerApi;
using GACManagerApi.Fusion;
using System.Windows;

namespace GACManager
{
    [ViewModel]
    public class GACAssemblyViewModel : ViewModel<AssemblyDetails>
    {
        public override void FromModel(AssemblyDetails model)
        {
            InternalAssemblyDetails = model;

            DisplayName = model.Name;
            FullName = model.FullName;
            Path = model.Path;
            Version = model.MajorVersion + "." + model.MinorVersion + "." +
                                model.BuildNumber + "." + model.RevisionNumber;
            Custom = model.Custom;
            ProcessorArchitecture = model.ProcessorArchitecture;
            Culture = model.Culture;
            RuntimeVersion = model.RuntimeVersion;
            if(model.PublicKeyToken != null)
                PublicKeyToken = BitConverter.ToString(model.PublicKeyToken).Replace("-", string.Empty);

            LoadExtendedPropertiesCommand = new AsynchronousCommand(DoLoadExtendedPropertiesCommand);
            CopyDisplayNameCommand = new Command(DoCopyDisplayNameCommand);
            ShowFilePropertiesCommand = new Command(DoShowFilePropertiesCommand);
        }

        public override void ToModel(AssemblyDetails model)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the internal assembly details, which is wht we're displaying data for.
        /// </summary>
        public AssemblyDetails InternalAssemblyDetails { get; private set; }

        /// <summary>
        /// The NotifyingProperty for the FullName property.
        /// </summary>
        private readonly NotifyingProperty DisplayNameProperty =
          new NotifyingProperty("DisplayName", typeof(string), default(string));

        /// <summary>
        /// Gets or sets FullName.
        /// </summary>
        /// <value>The value of FullName.</value>
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the Name property.
        /// </summary>
        private readonly NotifyingProperty NameProperty =
          new NotifyingProperty("Name", typeof(string), default(string));

        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        /// <value>The value of Name.</value>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the Culture property.
        /// </summary>
        private readonly NotifyingProperty CultureProperty =
          new NotifyingProperty("Culture", typeof(string), default(string));

        /// <summary>
        /// Gets or sets Culture.
        /// </summary>
        /// <value>The value of Culture.</value>
        public string Culture
        {
            get { return (string)GetValue(CultureProperty); }
            set { SetValue(CultureProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the FullName property.
        /// </summary>
        private readonly NotifyingProperty FullNameProperty =
          new NotifyingProperty("FullName", typeof(string), default(string));

        /// <summary>
        /// Gets or sets FullName.
        /// </summary>
        /// <value>The value of FullName.</value>
        public string FullName
        {
            get { return (string)GetValue(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the PublicKeyToken property.
        /// </summary>
        private readonly NotifyingProperty PublicKeyTokenProperty =
          new NotifyingProperty("PublicKeyToken", typeof(string), default(string));

        /// <summary>
        /// Gets or sets PublicKeyToken.
        /// </summary>
        /// <value>The value of PublicKeyToken.</value>
        public string PublicKeyToken
        {
            get { return (string)GetValue(PublicKeyTokenProperty); }
            set { SetValue(PublicKeyTokenProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the Version property.
        /// </summary>
        private readonly NotifyingProperty VersionProperty =
          new NotifyingProperty("Version", typeof(string), default(string));

        /// <summary>
        /// Gets or sets Version.
        /// </summary>
        /// <value>The value of Version.</value>
        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the FrameworkVersion property.
        /// </summary>
        private readonly NotifyingProperty FrameworkVersionProperty =
          new NotifyingProperty("FrameworkVersion", typeof(string), default(string));

        /// <summary>
        /// Gets or sets FrameworkVersion.
        /// </summary>
        /// <value>The value of FrameworkVersion.</value>
        public string FrameworkVersion
        {
            get { return (string)GetValue(FrameworkVersionProperty); }
            set { SetValue(FrameworkVersionProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the Path property.
        /// </summary>
        private readonly NotifyingProperty PathProperty =
          new NotifyingProperty("Path", typeof(string), default(string));

        /// <summary>
        /// Gets or sets Path.
        /// </summary>
        /// <value>The value of Path.</value>
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the ProcessorArchitecture property.
        /// </summary>
        private readonly NotifyingProperty ProcessorArchitectureProperty =
          new NotifyingProperty("ProcessorArchitecture", typeof(string), default(string));

        /// <summary>
        /// Gets or sets ProcessorArchitecture.
        /// </summary>
        /// <value>The value of ProcessorArchitecture.</value>
        public string ProcessorArchitecture
        {
            get { return (string)GetValue(ProcessorArchitectureProperty); }
            set { SetValue(ProcessorArchitectureProperty, value); }
        }
        
        /// <summary>
        /// The NotifyingProperty for the Custom property.
        /// </summary>
        private readonly NotifyingProperty CustomProperty =
          new NotifyingProperty("Custom", typeof(string), default(string));

        /// <summary>
        /// Gets or sets Custom.
        /// </summary>
        /// <value>The value of Custom.</value>
        public string Custom
        {
            get { return (string)GetValue(CustomProperty); }
            set { SetValue(CustomProperty, value); }
        }

        
        /// <summary>
        /// The NotifyingProperty for the RuntimeVersion property.
        /// </summary>
        private readonly NotifyingProperty RuntimeVersionProperty =
          new NotifyingProperty("RuntimeVersion", typeof(string), default(string));

        /// <summary>
        /// Gets or sets RuntimeVersion.
        /// </summary>
        /// <value>The value of RuntimeVersion.</value>
        public string RuntimeVersion
        {
            get { return (string)GetValue(RuntimeVersionProperty); }
            set { SetValue(RuntimeVersionProperty, value); }
        }

        

        /// <summary>
        /// Performs the LoadExtendedProperties command.
        /// </summary>
        /// <param name="parameter">The LoadExtendedProperties command parameter.</param>
        private void DoLoadExtendedPropertiesCommand(object parameter)
        {
            InternalAssemblyDetails.LoadExtendedProperties();

            //  Set the extended properties.
            LoadExtendedPropertiesCommand.ReportProgress(
                ()
                =>
                {
                    RuntimeVersion = InternalAssemblyDetails.RuntimeVersion;

                    InstallReferences.Clear();
                    foreach (var installReference in InternalAssemblyDetails.InstallReferences)
                        InstallReferences.Add(new InstallReferenceViewModel()
                                                  {
                                                      Identifier = installReference.Identifier,
                                                      Description = installReference.Description
                                                  });
                    });
        }

        /// <summary>
        /// Gets the LoadExtendedProperties command.
        /// </summary>
        /// <value>The value of .</value>
        public AsynchronousCommand LoadExtendedPropertiesCommand
        {
            get;
            private set;
        }

        
        /// <summary>
        /// The InstallReferences observable collection.
        /// </summary>
        private ObservableCollection<InstallReferenceViewModel> InstallReferencesProperty =
          new ObservableCollection<InstallReferenceViewModel>();

        /// <summary>
        /// Gets the InstallReferences observable collection.
        /// </summary>
        /// <value>The InstallReferences observable collection.</value>
        public ObservableCollection<InstallReferenceViewModel> InstallReferences
        {
            get { return InstallReferencesProperty; }
        }



        /// <summary>
        /// Performs the CopyDisplayName command.
        /// </summary>
        /// <param name="parameter">The CopyDisplayName command parameter.</param>
        private void DoCopyDisplayNameCommand(object parameter)
        {
            Clipboard.SetText(InternalAssemblyDetails.DisplayName);
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
            Interop.Shell.Shell32.ShowFileProperties(InternalAssemblyDetails.Path);
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
