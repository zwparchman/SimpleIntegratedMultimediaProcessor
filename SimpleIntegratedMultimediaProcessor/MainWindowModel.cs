using SimpleIntegratedMultimediaProcessor.Convert;
using SimpleIntegratedMultimediaProcessor.Download;
using SimpleIntegratedMultimediaProcessor.Settings;
using SimpleIntegratedMultimediaProcessor.Split;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegratedMultimediaProcessor
{
    public class MainWindowModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        DownloadModel _downloadContext;
        public DownloadModel DownloadContext
        {
            get { return _downloadContext; }
            set { _downloadContext = value; NotifyPropertyChanged(); }
        }

        ConvertModel _convertContext;
        public ConvertModel ConvertContext
        {
            get { return _convertContext; }
            set { _convertContext = value; NotifyPropertyChanged(); }
        }

        SplitModel _splitContext;
        public SplitModel SplitContext
        {
            get { return _splitContext; }
            set { _splitContext = value; NotifyPropertyChanged(); }
        }

        SettingsModel _settingsContext;
        public SettingsModel SettingsContext
        {
            get { return _settingsContext; }
            set { _settingsContext = value; NotifyPropertyChanged(); }
        }

        public MainWindowModel()
        {
            DownloadContext = new DownloadModel();
            ConvertContext = new ConvertModel();
            SplitContext = new SplitModel();
            SettingsContext = new SettingsModel();
        }
    }
}
