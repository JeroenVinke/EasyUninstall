using System.Collections.Generic;
using System.Linq;
using EasyUninstall.Interfaces;
using EasyUninstall.Models;
using Microsoft.Win32;

namespace EasyUninstall.Helpers
{
    public class RegistryHelper : IRegistryHelper
    {
        public List<Publisher> Publishers { get; set; }
        public List<WindowsApplication> Applications { get; set; }


        /// <summary>
        /// Opens the registry and lists all applications in the Applications collection
        /// and also lists all publishers in the Publishers collection
        /// </summary>
        public void Refresh()
        {
            Publishers = new List<Publisher>();
            Applications = new List<WindowsApplication>();


            // 32 bits
            ReadSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            // 64 bits
            ReadSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
        }



        /// <summary>
        /// Reads all underlying subkey and parses every subkey to a WindowsApplication model
        /// </summary>
        /// <param name="subKey"></param>
        private void ReadSubKey(string subKey)
        {
            using (var rk = Registry.LocalMachine.OpenSubKey(subKey))
            {

                if (rk == null) return;


                foreach (var skName in rk.GetSubKeyNames())
                {
                    using (var sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            var name = (string)sk.GetValue("DisplayName") ?? "";

                            var application = new WindowsApplication()
                            {
                                Name = name.Trim(),
                                UninstallString = (string)sk.GetValue("UninstallString"),
                                Version = (string)sk.GetValue("DisplayVersion"),
                                Guid = skName
                            };



                            // don't do anything with applications that don't have a name
                            if (string.IsNullOrEmpty(application.Name)) continue;


                            application.Publisher = AddPublisherToCollectionWhenNotExists((string)sk.GetValue("Publisher"));


                            // parse installation date to DateTime
                            var installDate = (string)sk.GetValue("InstallDate");
                            application.SetInstallDate(installDate);


                            Applications.Add(application);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            
        }







        /// <summary>
        /// Adds new publisher to Publishers collection when it does not exist yet
        /// Also registers to the PropertyChange event so that the list refreshes when you check a publisher
        /// </summary>
        /// <param name="publisherName"></param>
        private Publisher AddPublisherToCollectionWhenNotExists(string publisherName)
        {
            if (string.IsNullOrEmpty(publisherName)) return null;

            // only add to collection if it does not already exist in the collection
            if (Publishers.All(i => i.Name != publisherName))
            {
                if (!string.IsNullOrEmpty(publisherName))
                {

                    var publisher = new Publisher
                    {
                        Name = publisherName
                    };

                    publisher.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName != "Checked") return;

                        //if (RefreshAfterPublisherChecked)
                        //{
                        //    RefreshApplicationsViewSource();
                        //}
                    };


                    Publishers.Add(publisher);

                    return publisher;
                }
            }
            else
            {
                return Publishers.FirstOrDefault(i => i.Name == publisherName);
            }

            return null;
        }
    }
}
