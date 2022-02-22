using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegratedMultimediaProcessor.Split
{
    public class SplitModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        string _outputDirectory;
        public string OutputDirectory
        {
            get { return _outputDirectory; }
            set { _outputDirectory = value; NotifyPropertyChanged(); }
        }

        string _outputExtension;
        public string OutputExtension
        {
            get { return _outputExtension; }
            set { _outputExtension = value; NotifyPropertyChanged(); }
        }

        string _maxThreadsString = "8";
        public string MaxThreadsString
        {
            get { return _maxThreadsString; }
            set { _maxThreadsString = value; NotifyPropertyChanged(); }
        }

        int MaxThreads
        {
            get
            {
                int maxThreads;
                if(!int.TryParse(MaxThreadsString, out maxThreads))
                {
                    maxThreads = 1;
                }
                maxThreads = Math.Min(1, maxThreads);

                return maxThreads;
            }
        }

        ObservableCollection<SplitRow> _output = new ObservableCollection<SplitRow>();
        public ObservableCollection<SplitRow> Output
        {
            get { return _output; }
            set { _output = value; NotifyPropertyChanged(); }
        }


        string _startFile;
        public string StartFile
        {
            get { return _startFile; }
            set { _startFile = value; NotifyPropertyChanged(); }
        }

        bool _splitting;
        public bool Splitting
        {
            get { return _splitting; }
            set { _splitting = value; NotifyPropertyChanged(); }
        }

        string _error;
        public string Error
        {
            get { return _error; }
            set { _error = value; NotifyPropertyChanged(); }
        }

        ConcurrentQueue<SplitRow> _splittingQueue = new ConcurrentQueue<SplitRow>();

        List<Task> _splittingTasks = new List<Task>();

        ConcurrentQueue<string> _errors = new ConcurrentQueue<string>();

        void SplitTask()
        {
            var outDir = OutputDirectory;
            var inFile = StartFile;
            var ext = OutputExtension;

            SplitRow row;

            while (_splittingQueue.TryDequeue(out row))
            {
                if (!row.Valid) continue;

                using (var fproc = new Process())
                {
                    string outputFile = Path.Combine(outDir, row.Title + "." + ext);

                    inFile = inFile.Replace("\"", "\\\"");
                    outputFile = outputFile.Replace("\"", "\\\"");

                    string args;
                    if (row.EndSeconds >= 0)
                    {
                        args  = $"-i \"{inFile}\" -ss {row.StartSeconds} -t {row.EndSeconds} -y \"{outputFile}\"";
                    }
                    else
                    {
                        args  = $"-i \"{inFile}\" -ss {row.StartSeconds} -y \"{outputFile}\"";
                    }

                    try
                    {
                        fproc.StartInfo.FileName = "ffmpeg.exe";
                        fproc.StartInfo.Arguments = args;
                        fproc.StartInfo.RedirectStandardError = true;
                        fproc.StartInfo.RedirectStandardOutput = true;
                        fproc.StartInfo.UseShellExecute = false;
                        fproc.Start();

                        fproc.WaitForExit();
                        if(fproc.ExitCode != 0)
                        {
                            string er, ot;

                            ot = fproc.StandardOutput.ReadToEnd();
                            er = fproc.StandardError.ReadToEnd();

                            _errors.Enqueue($"While working on [{row.Title}] a status of [{fproc.ExitCode}] was returned. StdErr [{er}] StdOut [{ot}]");
                        }
                        else
                        {
                            _errors.Enqueue($"Finished [{row.Title}]");
                        }
                    }
                    catch(Exception ex)
                    {
                        _errors.Enqueue($"While working on [{row.Title}]\n{ex.ToString()}");
                    }
                }
            }
        }

        internal async void StartSplitting()
        {
            if (Splitting) return;

            Splitting = true;

            if (string.IsNullOrEmpty(OutputDirectory))
            {
                Error = "No output directory";
                return;
            }

            if (string.IsNullOrEmpty(StartFile))
            {
                Error = "A starting file is required";
                return;
            }

            Error = "";

            Output.ToList().ForEach(_splittingQueue.Enqueue);

            for (int i = 0; i < MaxThreads; i++) {
                _splittingTasks.Add(Task.Run(SplitTask));
            }

            await Task.Run(() =>
            {
                while( _splittingTasks.Count != 0)
                {
                    _splittingTasks[0].Wait();
                    _splittingTasks.RemoveAt(0);
                }
            });

            string err;
            while (_errors.TryDequeue(out err)) {
                Error += err + "\n";
            }

            Splitting = false;
        }

        internal void AbortSplitting()
        {
            if (!Splitting) return;

            SplitRow sr;
            while (_splittingQueue.TryDequeue(out sr)) { }

        }

        string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set 
            {
                if (_inputText == value) return;

                _inputText = value;
                NotifyPropertyChanged();

                RegenerateOutput();
            }
        }

        /// <summary>
        /// Regenerate output without clearing the collection
        /// </summary>
        private async void RegenerateOutput()
        {
            var space = new char[] { ' ' };

            int startSeconds = 0;

            var newRows = new List<SplitRow>();

            await Task.Run(() =>
            {
                IEnumerable<string> stringData = InputText
                    .Split('\n')
                    //remove empty lines
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    //remove comment lines
                    .Where(x => !x.StartsWith("#"));


                var last = new SplitRow();
                foreach(var str in stringData)
                {
                    var sr = new SplitRow();
                    int endSeconds = 0;

                    var parts = str.Split(space, 2);

                    if (parts.Length < 2) continue;

                    endSeconds = TimeToSeconds(parts[0]);
                    if (endSeconds < 0)
                    {
                        endSeconds = startSeconds;
                        sr.Valid = false;
                    }
                    else
                    {
                        last.EndSeconds = endSeconds;
                        sr.Valid = true;
                    }

                    sr.Title = parts[1];
                    sr.StartSeconds = last.EndSeconds;
                    sr.EndSeconds = -1;
                    startSeconds = endSeconds;

                    newRows.Add(sr);

                    last = sr;
                }
            });

            for(int i=0; i< Math.Max(newRows.Count, Output.Count); i++)
            {
                if(i >= Output.Count)
                {
                    Output.Add(newRows[i]);
                } 
                else if(i <= newRows.Count)
                {
                    Output[i].StartSeconds = newRows[i].StartSeconds;
                    Output[i].EndSeconds = newRows[i].EndSeconds;
                    Output[i].Title = newRows[i].Title;
                    Output[i].Valid = newRows[i].Valid;
                }
                else
                {
                    Output.RemoveAt(i);
                }
            }
        }

        private int TimeToSeconds(string v)
        {
            string hoursString = string.Empty, minutesString = string.Empty, secondsString = string.Empty;

            var parts = v.Split(':');
            if(parts.Length == 1)
            {
                secondsString = v;
            } 
            else if(parts.Length == 2)
            {
                minutesString = parts[0];
                secondsString = parts[1];
            }
            else if(parts.Length == 3)
            {
                hoursString = parts[0];
                minutesString = parts[1];
                secondsString = parts[2];
            }

            int hours=0, minutes=0, seconds=0;

            if(string.Empty != hoursString && !int.TryParse(hoursString, out hours))
            {
                return -1;
            }

            if(string.Empty != minutesString && !int.TryParse(minutesString, out minutes))
            {
                return -1;
            }

            if(string.Empty != secondsString && !int.TryParse(secondsString, out seconds))
            {
                return -1;
            }

            return hours * 3600 + minutes * 60 + seconds;
        }
    }
}
