using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using EasyUninstall.Annotations;
using EasyUninstall.Helpers;
using EasyUninstall.Interfaces;
using EasyUninstall.Models;

namespace EasyUninstall.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {


        /// <summary>
        /// helpers
        /// </summary>
        private IRegistryHelper _registryHelper;
        private IUninstallHelper _uninstallHelper;


        /// <summary>
        /// contains a list of string of all notification and error messages
        /// is presented to the user in the tab control
        /// </summary>
        private ObservableCollection<string> _logItems;
        public ObservableCollection<string> LogItems
        {
            get { return _logItems; }
            set
            {
                if (Equals(value, _logItems)) return;
                _logItems = value;
                OnPropertyChanged();
            }
        }




        /// <summary>
        /// Try to run msiexec commands with silent option
        /// This property is passed to uninstallhelper
        /// </summary>
        private bool _silentUninstall;
        public bool SilentUninstall
        {
            get { return _silentUninstall; }
            set
            {
                if (value.Equals(_silentUninstall)) return;
                _silentUninstall = value;
                OnPropertyChanged();
            }
        }






        /// <summary>
        /// commands
        /// </summary>
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }







        /// <summary>
        /// contains the current text of the search textbox
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText) return;
                _searchText = value;

                ApplicationsViewSource.View.Refresh();

                OnPropertyChanged();
            }
        }





        /// <summary>
        /// Shows/hides the publisher filter panel
        /// gets changed by the expander button
        /// </summary>
        private bool _publisherFilterVisible;
        public bool PublisherFilterVisible
        {
            get { return _publisherFilterVisible; }
            set
            {
                if (value.Equals(_publisherFilterVisible)) return;
                _publisherFilterVisible = value;
                OnPropertyChanged();
            }
        }




        /// <summary>
        /// the selected application in the datagrid
        /// </summary>
        private WindowsApplication _selectedApplication;
        public WindowsApplication SelectedApplication
        {
            get { return _selectedApplication; }
            set
            {
                if (Equals(value, _selectedApplication)) return;
                _selectedApplication = value;
                OnPropertyChanged();
            }
        }





        /// <summary>
        /// Contains a list of all publishes of all applications found in the registry
        /// </summary>
        public ObservableCollection<Publisher> Publishers { get; set; }
        public CollectionViewSource PublishersViewSource { get; set; }


        /// <summary>
        /// Contains a list of all applications found in the registry
        /// </summary>
        public ObservableCollection<WindowsApplication> Applications { get; set; }
        public CollectionViewSource ApplicationsViewSource { get; set; }



        /// <summary>
        /// Contains the index of the current selected tabpage
        /// </summary>
        private int _selectedTabPage;
        public int SelectedTabPage
        {
            get { return _selectedTabPage; }
            set
            {
                if (value == _selectedTabPage) return;
                _selectedTabPage = value;
                OnPropertyChanged();
            }
        }






        /// <summary>
        /// This flag allows the mastercheckbox to be checked/unchecked without 
        /// refreshing the list for every publisher checkbox
        /// </summary>
        public bool RefreshAfterPublisherChecked = true;

        


        // empty constructor so that WPF can create an instance of the viewmodel
        // in the view
        public MainWindowViewModel()
        {
            Constructor();
        }


        // constructor with parameters so that dependencies can be injected for unit tests
        public MainWindowViewModel(IRegistryHelper registryHelper = null, IUninstallHelper uninstallHelper = null)
        {
            Constructor(registryHelper, uninstallHelper);
        }


        // Constructor method is called by all constructors
        public void Constructor(IRegistryHelper registryHelper = null, IUninstallHelper uninstallHelper = null)
        {
            // helpers
            _registryHelper = registryHelper ?? new RegistryHelper();
            _uninstallHelper = uninstallHelper ?? new UninstallHelper();


            // all errors raised by uninstallHelper should be logged
            _uninstallHelper.Error += (obj, args) => Log(args.Message);



            // Always try silent uninstall for msiexec commands
            SilentUninstall = true;
            
            
            LogItems = new ObservableCollection<string>();


            // commands
            RemoveCommand = new RelayCommand(RemoveApplications, CanRemoveApplications);
            RefreshCommand = new RelayCommand(Refresh);



            // publishers
            Publishers = new ObservableCollection<Publisher>();
            PublishersViewSource = new CollectionViewSource();
            PublishersViewSource.Source = Publishers;



            // applications
            Applications = new ObservableCollection<WindowsApplication>();
            ApplicationsViewSource = new CollectionViewSource();
            ApplicationsViewSource.Source = Applications;
        }






        /// <summary>
        /// Determines if the user can remove applicaties
        /// Checks if any applications are selected
        /// </summary>
        /// <returns></returns>
        public bool CanRemoveApplications()
        {
            return Applications.Any(i => i.Checked);
        }









        /// <summary>
        /// Removes all selected applications
        /// </summary>
        public void RemoveApplications()
        {
            SelectedTabPage = 1;

            foreach (var application in Applications.Where(i => i.Checked))
            {
                Log("Bezig met verwijderen van: " + application.Name);

                _uninstallHelper.Uninstall(application, SilentUninstall);

                Log(application.Name + " is verwijderd");
            }


            Refresh();

            Log("Done!");
        }






        /// <summary>
        /// Logs an error to the LogItems collection
        /// adds a timestamp
        /// </summary>
        /// <param name="error"></param>
        public void Log(string error)
        {
            LogItems.Add(string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, error));
        }














        /// <summary>
        /// Forces the registryhelper to read new applications and publishers and
        /// refreshes all viewsources
        /// </summary>
        public void Refresh()
        {
            var selectedPublishers = Publishers.Where(i => i.Checked).Select(i => i.Name).ToList();

            _registryHelper.Refresh();

            Applications = new ObservableCollection<WindowsApplication>(_registryHelper.Applications);
            Publishers = new ObservableCollection<Publisher>(_registryHelper.Publishers);

            foreach (var publisher in Publishers)
            {
                if (selectedPublishers.Contains(publisher.Name))
                {
                    publisher.Checked = true;
                }

                publisher.PropertyChanged += OnPublisherPropertyChanged;
            }


            RefreshPublisherViewSource();
            RefreshApplicationsViewSource();
        }



        /// <summary>
        /// Refreshes the viewsource when publishers are checked and
        /// RefreshAfterPublisherChecked is true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPublisherPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Checked" && RefreshAfterPublisherChecked)
            {
                RefreshApplicationsViewSource();
            }
        }


        /// <summary>
        /// Forces the PublishersViewSource to refresh the datasource
        /// Also adds the sortdescriptions
        /// </summary>
        public void RefreshPublisherViewSource()
        {
            PublishersViewSource.Source = Publishers;
            PublishersViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }










        /// <summary>
        /// Forces the ApplicationsViewSource to refresh the datasource
        /// and adds the sortdescriptions.
        /// 
        /// Also handles the filter event of the viewsource
        /// </summary>
        public void RefreshApplicationsViewSource()
        {
            ApplicationsViewSource.Source = Applications;
            ApplicationsViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            ApplicationsViewSource.SortDescriptions.Add(new SortDescription("InstallDate", ListSortDirection.Descending));

            ApplicationsViewSource.View.Filter += OnApplicationFilter;
        }




        /// <summary>
        /// Determines wheter or not to show the application in the datagrid
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool OnApplicationFilter(object item)
        {
            var application = item as WindowsApplication;

            if (application == null) return true;


            // if any publishers are selected
            // then hide all applications of publishers that are not selected
            if (Publishers.Any(i => i.Checked))
            {
                if (application.Publisher == null) return false;

                
                var publisher = Publishers.First(i => i.Name == application.Publisher.Name);
                if (!publisher.Checked) return false;
            }


            // no search text provided? -> show everything
            if (string.IsNullOrEmpty(SearchText)) return true;

            // show application if application name contains the searchtext
            if (application.Name.ToLower().Contains(SearchText.ToLower()))
                return true;


            return false;
        }

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
