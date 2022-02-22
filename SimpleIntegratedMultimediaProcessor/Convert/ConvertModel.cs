using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegratedMultimediaProcessor.Convert
{
    public class ConvertModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string _startFile, _endFile;
        public string StartFile
        {
            get { return _startFile; }
            set { _startFile = value; NotifyPropertyChanged(); }
        }

        public string EndFile
        {
            get { return _endFile; }
            set { _endFile = value; NotifyPropertyChanged(); }
        }

        bool _converting;
        public bool Converting
        {
            get { return _converting; }
            set { _converting = value; NotifyPropertyChanged(); }
        }

        Process ffmpegProc;

        string _error;
        public string Error
        {
            get { return _error; }
            set { _error = value; NotifyPropertyChanged(); }
        }

        bool _twitterVideo;
        public bool TwitterVideo
        {
            get { return _twitterVideo; }
            set 
            { 
                _twitterVideo = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("CanEditOptions");
                NotifyPropertyChanged("ExtraOptions");
            }
        }

        public bool ReadOnlyOptions
        {
            get
            {
                return TwitterVideo;
            }
        }

        string _extraOptions;
        public string ExtraOptions
        {
            get 
            {
                if (TwitterVideo)
                {
                    _extraOptions = "-c:v libx264 -crf 20 -preset slow -vf format=yuv420p -c:a aac -movflags +faststart";
                }

                return _extraOptions;
            }
            set { _extraOptions = value; NotifyPropertyChanged(); }
        }

        public async void DoConversion()
        {
            Error = "Starting Conversion";
            if (string.IsNullOrEmpty(StartFile))
            {
                Error = "You need a starting file";
                return;
            }

            if (string.IsNullOrEmpty(StartFile))
            {
                Error = "You need an ending file";
                return;
            }

            try
            {
                if (File.Exists(EndFile))
                {
                    File.Delete(EndFile);
                }
            }
            catch(Exception ex)
            {
                Error = ex.ToString();
                return;
            }

            using (ffmpegProc = new Process())
            {

                var sb = new StringBuilder();
                sb.Append($"-i {StartFile} ");
                sb.Append(ExtraOptions + " ");
                sb.Append($"{EndFile} ");

                //later -- -r=rate limit
                ffmpegProc.StartInfo.FileName = $"ffmpeg.exe";
                ffmpegProc.StartInfo.Arguments = sb.ToString();
                ffmpegProc.StartInfo.CreateNoWindow = true;
                ffmpegProc.StartInfo.RedirectStandardOutput = true;
                ffmpegProc.StartInfo.UseShellExecute = false;

                try
                {
                    ffmpegProc.Start();
                    Converting = true;
                    Error = "Converting";
                    await Task.Run(() =>
                    {
                        ffmpegProc.WaitForExit();
                        Error = "";
                        while(!ffmpegProc.StandardOutput.EndOfStream)
                        {
                            Error += ffmpegProc.StandardOutput.ReadLine() + "\n";
                        }
                        Error += "Finished";
                    });
                }
                catch(Exception ex)
                {
                    Error = ex.ToString();
                }
                finally
                {
                    Converting = false;
                    ffmpegProc = null;
                }
            }
        }

        public void KillConversion()
        {
            if (ffmpegProc != null)
            {
                ffmpegProc.Kill();
            }
        }


    }
}
