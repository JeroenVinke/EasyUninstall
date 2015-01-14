using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyUninstall.ViewModels;

namespace EasyUninstall.Views
{
    public partial class MainWindow
    {
        /// <summary>
        /// Returns the viewmodel
        /// </summary>
        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
        }





        public MainWindow()
        {
            InitializeComponent();

            ViewModel.Refresh();
        }



        /// <summary>
        /// Closes the mainwindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SluitenButton_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }



        /// <summary>
        /// When typing in the datagrid it will focus the searchtextbox
        /// When pressing space in the datagrid it will inverse the checked state of
        /// the selected application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var application = ViewModel.SelectedApplication;
                application.Checked = !application.Checked;
            }
            else
            {
                SearchTextBox.Focus();
            }
        }

        /// <summary>
        /// Toggles all publisher checkboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;

            // prevents the viewsource from refreshing a lot of times
            ViewModel.RefreshAfterPublisherChecked = false;

            // changes the checked state of all publisher checkboxes 
            // to the state of the master checkbox
            foreach (var publisher in ViewModel.Publishers)
            {
                publisher.Checked = checkbox.IsChecked ?? false;
            }

            // refresh the viewsource so the changes are made visibile
            ViewModel.RefreshApplicationsViewSource();

            ViewModel.RefreshAfterPublisherChecked = true;
        }
    }
}
