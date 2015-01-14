using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using EasyUninstall.Annotations;

namespace EasyUninstall.Models
{
    public class WindowsApplication : INotifyPropertyChanged
    {
        private bool _checked;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (value.Equals(_checked)) return;
                _checked = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string UninstallString { get; set; }
        public string Guid { get; set; }
        public Publisher Publisher { get; set; }

        public DateTime? InstallDate { get; set; }

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void SetInstallDate(string installDate)
        {
            if (string.IsNullOrEmpty(installDate))
            {
                return;
            }

            DateTime date;

            var success = DateTime.TryParseExact(installDate, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out date);

            if (success)
            {
                InstallDate = date;
            }
            else
            {
                success = DateTime.TryParseExact(installDate, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out date);

                if (success)
                {
                    InstallDate = date;
                }
                else
                {
                    success = DateTime.TryParseExact(installDate, "d-m-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out date);

                    if (success)
                    {
                        InstallDate = date;
                    }
                }
            }


        }
    }
}