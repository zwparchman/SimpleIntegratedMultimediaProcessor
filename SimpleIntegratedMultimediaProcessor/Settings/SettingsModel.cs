using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegratedMultimediaProcessor.Settings
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DownloadProgram
        {
            get { return Properties.Settings.Default["DownloadProgram"].ToString(); }
            set { Properties.Settings.Default["DownloadProgram"] = value; NotifyPropertyChanged(); }
        }

        public string FFMpegPath
        {
            get { return Properties.Settings.Default["FFMpegPath"].ToString(); }
            set { Properties.Settings.Default["FFMpegPath"] = value; NotifyPropertyChanged(); }
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        public void Revert()
        {
            Properties.Settings.Default.Reload();
            NotifyPropertyChanged(null);
        }
    }
}
