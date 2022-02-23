using SimpleIntegratedMultimediaProcessor.Settings;
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
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIntegratedMultimediaProcessor.Split
{
    public class SplitModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SplitModel()
        {
            RegenerateOutput();
        }

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
            get 
            { 
                return _outputExtension; 
            }
            set 
            {
                _outputExtension = value;
                NotifyPropertyChanged();
            }
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
                maxThreads = Math.Max(1, maxThreads);

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
            set { 
                _startFile = value;
                NotifyPropertyChanged();

                if (File.Exists(_startFile))
                {
                    int rind = _startFile.LastIndexOf('.');
                    if(rind != -1)
                    {
                        rind += 1;
                        OutputExtension = _startFile.Substring(rind);
                    }
                }
            }
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


        ConcurrentQueue<string> _errors = new ConcurrentQueue<string>();

        void SplitTask( SplitRow row )
        {
            var outDir = OutputDirectory;
            var inFile = StartFile;
            var ext = OutputExtension;

            if (!row.Valid) return;

            using (var fproc = new Process())
            {
                string outputFile = Path.Combine(outDir, row.Title + "." + ext);

                inFile = inFile.Replace("\"", "\\\"");
                outputFile = outputFile.Replace("\"", "\\\"");

                string args;
                if (row.EndSeconds >= 0)
                {
                    int duration = row.EndSeconds - row.StartSeconds;
                    args  = $"-i \"{inFile}\" -ss {row.StartSeconds} -t {duration} \"{outputFile}\"";
                }
                else
                {
                    args  = $"-i \"{inFile}\" -ss {row.StartSeconds} \"{outputFile}\"";
                }

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                try
                {
                    var set = new SettingsModel();
                    fproc.StartInfo.FileName = set.FFMpegPath;
                    fproc.StartInfo.Arguments = args;
                    fproc.StartInfo.RedirectStandardError = true;
                    fproc.StartInfo.RedirectStandardOutput = true;
                    fproc.StartInfo.RedirectStandardInput = true;
                    fproc.StartInfo.UseShellExecute = false;
                    fproc.Start();

                    fproc.StandardInput.Close();

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
                    _errors.Enqueue($"While working on [{row.Title}]\n{ex}");
                    fproc.Kill();
                    throw;
                }
                finally
                {
                    fproc.Dispose();
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

            Semaphore sem = new Semaphore(MaxThreads, MaxThreads);
            await Task.Run(() =>
            {
                Output.
                    ToList()
                    .AsParallel()
                    .ForAll((row) => {
                        sem.WaitOne();
                        try
                        {
                            SplitTask(row);
                        }
                        finally
                        {
                            sem.Release();
                        }
                });
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

        string _inputText = @"
00:00 this is an example
# this is a comment that does not get parsed
3:3 this is the second track start time
3:33 this is 30 seconds after the start of the second track.
";
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


                var invalidChars = new HashSet<char>(Path.GetInvalidPathChars());
                var last = new SplitRow();
                foreach(var str in stringData)
                {
                    var sr = new SplitRow();
                    int endSeconds = 0;

                    var parts = str.Split(space, 2);

                    if (parts.Length < 2) continue;

                    endSeconds = TimeToSeconds(parts[0]);
                    sr.Title = parts[1];

                    if (endSeconds < 0)
                    {
                        endSeconds = startSeconds;
                        sr.Valid = false;
                    }
                    else
                    {
                        last.EndSeconds = endSeconds;

                        var titleChars = new HashSet<char>(sr.Title);

                        bool badChars = invalidChars
                            .Any((c) => titleChars.Contains(c));

                        sr.Valid = !badChars;
                    }

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
                else if(i < newRows.Count)
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
