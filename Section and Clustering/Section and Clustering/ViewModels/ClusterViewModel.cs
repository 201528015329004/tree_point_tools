using System.ComponentModel;
using System.Runtime.CompilerServices;
using Section_and_Clustering.Annotations;

namespace Section_and_Clustering.ViewModels
{
    public class ClusterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        private bool _isNotBusy;
        private string _inputFile;
        private string _progressText;
        private double _tolerance;

        public bool IsNotBusy
        {
            get { return _isNotBusy; }
            set
            {
                if (value == _isNotBusy) return;
                _isNotBusy = value;
                RaisePropertyChanged();
            }
        }

        public string InputFile
        {
            get { return _inputFile; }
            set
            {
                if (value == _inputFile) return;
                _inputFile = value;
                RaisePropertyChanged();
            }
        }

        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                if (value == _progressText) return;
                _progressText = value;
                RaisePropertyChanged();
            }
        }

        public double Tolerance
        {
            get { return _tolerance; }
            set
            {
                if (value.Equals(_tolerance)) return;
                _tolerance = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public ClusterViewModel()
        {
            this.Tolerance = 5.0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}