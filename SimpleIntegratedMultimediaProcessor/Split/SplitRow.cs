using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleIntegratedMultimediaProcessor.Split
{
    public class SplitRow:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        int _startSeconds;
        public int StartSeconds
        {
            get { return _startSeconds; }
            set { _startSeconds = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(StartTimeString)); }
        }

        int _endSeconds;
        public int EndSeconds
        {
            get { return _endSeconds; }
            set { _endSeconds = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(EndTimeString)); }
        }

        public string StartTimeString
        {
            get
            {
                int hours = StartSeconds / 3600;
                int minutes = (StartSeconds / 60 ) % 60;
                int seconds = StartSeconds % 60;

                return $"{hours:00}:{minutes:00}:{seconds:00}";
            }
        }

        public string EndTimeString
        {
            get
            {
                if(EndSeconds < 0)
                {
                    return "";
                }

                int hours = EndSeconds / 3600;
                int minutes = (EndSeconds / 60 ) % 60;
                int seconds = EndSeconds % 60;

                return $"{hours:00}:{minutes:00}:{seconds:00}";
            }
        }

        string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(); }
        }

        bool _valid;
        public bool Valid
        {
            get { return _valid; }
            set { _valid = value; NotifyPropertyChanged(); }
        }
    }
}