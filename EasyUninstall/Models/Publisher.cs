using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasyUninstall.Annotations;

namespace EasyUninstall.Models
{
    public class Publisher : INotifyPropertyChanged
    {
        public string Name { get; set; }

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
