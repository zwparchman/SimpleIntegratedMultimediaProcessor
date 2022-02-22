using SimpleIntegratedMultimediaProcessor.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegratedMultimediaProcessor.Download
{
    public class DownloadModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string _url;
        public string URL
        {
            get { return _url; }
            set { _url = value; NotifyPropertyChanged(); }
        }

        string _downloadPath;
        public string DownloadPath
        {
            get { return _downloadPath; }
            set { _downloadPath = value; NotifyPropertyChanged(); }
        }

        bool _downloading;
        public bool Downloading
        {
            get { return _downloading; }
            set { _downloading = value; NotifyPropertyChanged(); }
        }

        string _error;
        public string Error
        {
            get { return _error; }
            set { _error = value; NotifyPropertyChanged(); }
        }


        bool _useDefaultFileName;
        public bool UseDefaultFileName {
            get { return _useDefaultFileName; }
            set { _useDefaultFileName = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(DownloadPrompt)); }
        }

        public string DownloadPrompt
        {
            get
            {
                if (UseDefaultFileName)
                {
                    return "Output Directory";
                }
                else
                {
                    return "Output Path";
                }
            }
        }

        Process youtubedlProc;

        public async void DoDownload()
        {
            using (youtubedlProc = new Process())
            {
                if (string.IsNullOrEmpty(DownloadPath))
                {
                    Error = "Output Location required";
                    return;
                }

                if (string.IsNullOrEmpty(URL))
                {
                    Error = "URL is required";
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(URL + " ");
                if (UseDefaultFileName)
                {
                    if (!Directory.Exists(DownloadPath))
                    {
                        Error = "Output location is not a directory";
                        return;
                    }
                    youtubedlProc.StartInfo.WorkingDirectory = DownloadPath;
                }
                else
                {
                    sb.Append($"-o {DownloadPath}");
                }

                //later -- -r=rate limit
                var set = new SettingsModel();
                youtubedlProc.StartInfo.FileName = set.DownloadProgram;
                youtubedlProc.StartInfo.Arguments = sb.ToString();
                youtubedlProc.StartInfo.CreateNoWindow = true;

                try
                {
                    youtubedlProc.Start();
                    Downloading = true;
                    await Task.Run(() =>
                    {
                        youtubedlProc.WaitForExit();
                    });
                }
                catch(Exception ex)
                {
                    Error = ex.ToString();
                }
                finally
                {
                    Downloading = false;
                    youtubedlProc = null;
                }
            }
        }

        public void KillDownload()
        {
            if (youtubedlProc != null)
            {
                youtubedlProc.Kill();
            }
        }
    }
}
