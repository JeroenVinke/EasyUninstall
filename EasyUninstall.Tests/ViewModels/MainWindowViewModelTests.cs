using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using EasyUninstall.Helpers;
using EasyUninstall.Interfaces;
using EasyUninstall.Models;
using EasyUninstall.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyUninstall.Tests.ViewModels
{
    [TestClass]
    public class MainWindowViewModelTests
    {
        [TestMethod]
        public void Log()
        {
            var viewModel = new MainWindowViewModel();

            viewModel.Log("Test");

            Assert.IsTrue(viewModel.LogItems.Any());
        }








        [TestMethod]
        public void RemoveApplications_OnlyCheckdApplications()
        {

            var app =
                new WindowsApplication()
                {
                    Name = "NotCheckedApplication",
                    Checked = false
                };


            var mock = new Mock<IUninstallHelper>();
            mock.Setup(i => i.Uninstall(app)).Verifiable();

            var viewModel = new MainWindowViewModel(null, mock.Object);



            viewModel.Applications = new ObservableCollection<WindowsApplication>
            {
                app
            };

            viewModel.RemoveApplications();

            mock.Verify(i => i.Uninstall(app), Times.Never);
        }


        [TestMethod]
        public void RemoveApplications_SelectedTabChanged()
        {
            var mock = new Mock<IUninstallHelper>();
            var viewModel = new MainWindowViewModel(null, mock.Object);

            viewModel.SelectedTabPage = 0;

            viewModel.RemoveApplications();

            Assert.IsTrue(viewModel.SelectedTabPage != 0);
        }


        [TestMethod]
        public void RemoveApplications_MessagesAreLogged()
        {

            var app =
                new WindowsApplication()
                {
                    Name = "Application",
                    Checked = true
                };


            var mock = new Mock<IUninstallHelper>();

            var viewModel = new MainWindowViewModel(null, mock.Object);



            viewModel.Applications = new ObservableCollection<WindowsApplication>
            {
                app
            };

            viewModel.RemoveApplications();

            Assert.IsTrue(viewModel.LogItems.Count == 3);
        }



        [TestMethod]
        public void CanRemoveApplications_NoCheckedApplicationReturnsFalse()
        {
            var viewModel = new MainWindowViewModel();
            viewModel.Applications = new ObservableCollection<WindowsApplication>();

            Assert.IsFalse(viewModel.CanRemoveApplications());
        }



        [TestMethod]
        public void CanRemoveApplications_CheckedApplicationReturnsTrue()
        {
            var viewModel = new MainWindowViewModel();
            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication()
                {
                    Name = "Test",
                    Checked = true
                }
            };

            Assert.IsTrue(viewModel.CanRemoveApplications());
        }




        [TestMethod]
        public void AreUninstallErrorsLogged()
        {

            var mock = new Mock<IUninstallHelper>();




            var viewModel = new MainWindowViewModel(null, mock.Object);



            mock.Raise(i => i.Error += null, new ExtendedEventArgs { Message = "Test" });

            Assert.IsTrue(viewModel.LogItems.Count == 1 && viewModel.LogItems[0].Contains("Test"));
        }





        [TestMethod]
        public void INPC_Raised()
        {
            var viewModel = new MainWindowViewModel();


            var raised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                raised = true;
            };

            viewModel.PublisherFilterVisible = true;


            Assert.IsTrue(raised);
        }









        [TestMethod]
        public void SelectedApplication_Getter()
        {
            var viewModel = new MainWindowViewModel();

            var app = new WindowsApplication()
            {
                Name = "Test"
            };

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_selectedApplication",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            pInfo.SetValue(viewModel, app);

            Assert.IsTrue(Equals(app, viewModel.SelectedApplication));
        }







        [TestMethod]
        public void SelectedApplication_Setter()
        {
            var viewModel = new MainWindowViewModel();

            var app = new WindowsApplication()
            {
                Name = "Test"
            };

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_selectedApplication",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            pInfo.SetValue(viewModel, app);



            viewModel.SelectedApplication = new WindowsApplication();

            Assert.IsFalse(Equals(app, viewModel.SelectedApplication));
        }




        [TestMethod]
        public void SelectedApplication_Setter_INPCNotRaisedWhenSameValuePassed()
        {
            var viewModel = new MainWindowViewModel();

            var app = new WindowsApplication()
            {
                Name = "Test"
            };

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_selectedApplication",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            pInfo.SetValue(viewModel, app);


            var raised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedApplication")
                {
                    raised = true;
                }
            };

            viewModel.SelectedApplication = app;

            Assert.IsFalse(raised);
        }







        [TestMethod]
        public void OnPublisherPropertyChanged_NotRefreshingWhenFlagIsFalse()
        {
            var viewModel = new MainWindowViewModel();

            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication()
            };
            viewModel.RefreshApplicationsViewSource();





            viewModel.RefreshAfterPublisherChecked = false;

            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication(),
                new WindowsApplication()
            };

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnPublisherPropertyChanged",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);

            pInfo.Invoke(viewModel, new object[] {null,new PropertyChangedEventArgs("Checked")});


            var viewSource = (ObservableCollection<WindowsApplication>) viewModel.ApplicationsViewSource.Source;

            // viewsource is not refreshed because it has the old list (2 items instead of 3)
            Assert.IsTrue(viewSource.Count == 2);
        }


        [TestMethod]
        public void OnPublisherPropertyChanged_NotRefreshingWhenEventIsNotChecked()
        {
            var viewModel = new MainWindowViewModel();

            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication()
            };
            viewModel.RefreshApplicationsViewSource();



            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication(),
                new WindowsApplication()
            };

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnPublisherPropertyChanged",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);

            pInfo.Invoke(viewModel, new object[] { null, new PropertyChangedEventArgs("Abcd") });


            var viewSource = (ObservableCollection<WindowsApplication>)viewModel.ApplicationsViewSource.Source;

            // viewsource is not refreshed because it has the old list (2 items instead of 3)
            Assert.IsTrue(viewSource.Count == 2);
        }




        [TestMethod]
        public void OnPublisherPropertyChanged_ConfirmRefresh()
        {
            var viewModel = new MainWindowViewModel();

            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication()
            };
            viewModel.RefreshApplicationsViewSource();



            viewModel.Applications = new ObservableCollection<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication(),
                new WindowsApplication()
            };

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnPublisherPropertyChanged",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);

            pInfo.Invoke(viewModel, new object[] { null, new PropertyChangedEventArgs("Checked") });


            var viewSource = (ObservableCollection<WindowsApplication>)viewModel.ApplicationsViewSource.Source;

            // viewsource is refreshed because it has now 3 items
            Assert.IsTrue(viewSource.Count == 3);
        }


        [TestMethod]
        public void OnApplicationFilter_TrueIfNull()
        {
            var viewModel = new MainWindowViewModel();


            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnApplicationFilter",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);



            bool returnValue = (bool)pInfo.Invoke(viewModel, new object[] { null });

            Assert.IsTrue(returnValue);
        }







        [TestMethod]
        public void OnApplicationFilter_FalseIfPublishersSelectedAndPublisherIsNull()
        {
            var viewModel = new MainWindowViewModel();


            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnApplicationFilter",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            viewModel.Publishers = new ObservableCollection<Publisher>()
            {
                new Publisher()
                {
                    Checked = true,
                    Name = "Test publisher"
                }
            };

            bool returnValue = (bool)pInfo.Invoke(viewModel, new object[] { new WindowsApplication() { Publisher = null } });

            Assert.IsFalse(returnValue);
        }





        [TestMethod]
        public void OnApplicationFilter_FalseIfPublisherIsNotInSelectedPublishers()
        {
            var viewModel = new MainWindowViewModel();


            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnApplicationFilter",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            viewModel.Publishers = new ObservableCollection<Publisher>()
            {
                new Publisher()
                {
                    Checked = true,
                    Name = "CheckedPublisher"
                },
                new Publisher()
                {
                    Checked = false,
                    Name = "NotCheckPublished"
                }
            };



            var application = new WindowsApplication() { Publisher = new Publisher() { Name = "NotCheckPublished" } };

            bool returnValue = (bool)pInfo.Invoke(viewModel, new object[] { application });

            Assert.IsFalse(returnValue);
        }






        [TestMethod]
        public void OnApplicationFilter_TrueIfNoSearchTextIsProvided()
        {
            var viewModel = new MainWindowViewModel();


            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnApplicationFilter",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            viewModel.SearchText = null;



            var application = new WindowsApplication() { Name = "Test name" };

            bool returnValue = (bool)pInfo.Invoke(viewModel, new object[] { application });

            Assert.IsTrue(returnValue);
        }





        [TestMethod]
        public void OnApplicationFilter_FalseIfSearchTextProvidedAndNotFoundInApplicationName()
        {
            var viewModel = new MainWindowViewModel();


            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnApplicationFilter",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            viewModel.SearchText = "Abcd";



            var application = new WindowsApplication() { Name = "Test name" };

            bool returnValue = (bool)pInfo.Invoke(viewModel, new object[] { application });

            Assert.IsFalse(returnValue);
        }








        [TestMethod]
        public void OnApplicationFilter_TrueIfNameInSearchText()
        {
            var viewModel = new MainWindowViewModel();


            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetMethod("OnApplicationFilter",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            viewModel.SearchText = "Abcd";



            var application = new WindowsApplication() { Name = "Abcd" };

            bool returnValue = (bool)pInfo.Invoke(viewModel, new object[] { application });

            Assert.IsTrue(returnValue);
        }



        [TestMethod]
        public void PublisherFilterVisibile_Get()
        {
            var viewModel = new MainWindowViewModel();

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_publisherFilterVisible",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            pInfo.SetValue(viewModel, false);

            //



            Assert.IsFalse(viewModel.PublisherFilterVisible);


            pInfo.SetValue(viewModel, true);



            Assert.IsTrue(viewModel.PublisherFilterVisible);
        }






        [TestMethod]
        public void PublisherFilterVisibile_SetNewValue()
        {
            var viewModel = new MainWindowViewModel();

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_publisherFilterVisible",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            viewModel.PublisherFilterVisible = true;

            Assert.IsTrue((bool)pInfo.GetValue(viewModel));


            viewModel.PublisherFilterVisible = false;

            Assert.IsFalse((bool)pInfo.GetValue(viewModel));
        }






        [TestMethod]
        public void PublisherFilterVisibile_SetOldValue()
        {
            var viewModel = new MainWindowViewModel();

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_publisherFilterVisible",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);




            viewModel.PublisherFilterVisible = true;

            bool raised = false;

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "PublisherFilterVisibile")
                {
                    raised = false;
                }
            };

            viewModel.PublisherFilterVisible = true;

            Assert.IsFalse(raised);
        }











        [TestMethod]
        public void Refresh_RegistryHelperCalled()
        {
            var mock = new Mock<IRegistryHelper>();
            mock.Setup(i => i.Refresh()).Verifiable();
            mock.Setup(i => i.Publishers).Returns(new List<Publisher>());
            mock.Setup(i => i.Applications).Returns(new List<WindowsApplication>());

            var viewModel = new MainWindowViewModel(mock.Object);

            viewModel.Refresh();

            mock.Verify(i => i.Refresh());
        }





        [TestMethod]
        public void Refresh_ConfirmCollectionCopy()
        {
            var mock = new Mock<IRegistryHelper>();
            mock.Setup(i => i.Refresh()).Verifiable();
            mock.Setup(i => i.Publishers).Returns(new List<Publisher>()
            {
                new Publisher(),
                new Publisher()
            });
            mock.Setup(i => i.Applications).Returns(new List<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication()
            });

            var viewModel = new MainWindowViewModel(mock.Object);

            viewModel.Refresh();


            Assert.IsTrue(viewModel.Applications.Count == 2 && viewModel.Publishers.Count == 2);
        }









        [TestMethod]
        public void Refresh_CheckPublisherIfWasChecked()
        {
            var mock = new Mock<IRegistryHelper>();
            mock.Setup(i => i.Refresh()).Verifiable();
            mock.Setup(i => i.Publishers).Returns(new List<Publisher>()
            {
                new Publisher()
                {
                    Checked = false,
                    Name = "Test"
                }
            });
            mock.Setup(i => i.Applications).Returns(new List<WindowsApplication>()
            {
                new WindowsApplication(),
                new WindowsApplication()
            });

            var viewModel = new MainWindowViewModel(mock.Object);

            viewModel.Publishers.Add(new Publisher
            {
                Name = "Test",
                Checked = true
            });

            viewModel.Refresh();




            Assert.IsTrue(viewModel.Publishers[0].Checked);
        }


        [TestMethod]
        public void LogItems_Set()
        {
            var viewModel = new MainWindowViewModel();

            Type tInfo = viewModel.GetType();
            var pInfo = tInfo.GetField("_logItems",
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);



            var list = new ObservableCollection<string>()
            {
                "",""
            };


            viewModel.LogItems = list;

            var raised = false;

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "LogItems")
                {
                    raised = true;
                }
            };


            viewModel.LogItems = list;

            Assert.IsFalse(raised);
        }



        [TestMethod]
        public void RefreshApplicationsViewSource()
        {
            var viewModel = new MainWindowViewModel();



            viewModel.Applications = new ObservableCollection<WindowsApplication>
            {
                new WindowsApplication(),
                new WindowsApplication()
            };

            viewModel.RefreshApplicationsViewSource();


            // check if source is set on viewsource
            Assert.IsTrue(viewModel.Applications.Count == ((ObservableCollection<WindowsApplication>)viewModel.ApplicationsViewSource.Source).Count);


            // check if sortdescriptions are set
            Assert.IsTrue(viewModel.ApplicationsViewSource.SortDescriptions.Count == 2);
        }





        [TestMethod]
        public void RefreshPublishersViewSource()
        {
            var viewModel = new MainWindowViewModel();



            viewModel.Publishers = new ObservableCollection<Publisher>
            {
                new Publisher(),
                new Publisher()
            };

            viewModel.RefreshPublisherViewSource();


            // check if source is set on viewsource
            Assert.IsTrue(viewModel.Publishers.Count == ((ObservableCollection<Publisher>)viewModel.PublishersViewSource.Source).Count);

            // check if sortdescriptions are set
            Assert.IsTrue(viewModel.PublishersViewSource.SortDescriptions.Count == 1);
        }
    }
}
