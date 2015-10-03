using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Thinning_Tool.Annotations;

namespace Thinning_Tool
{
    public class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties 
        private int _thinFactor;
        private string _outputFileName;
        private string _inputFileName;
        private bool _isNotBusy;
        private string _displayText;

        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                if (value == _displayText) return;
                _displayText = value;
                RaisePropertyChanged();
            }
        }

        public string InputFileName
        {
            get { return _inputFileName; }
            set
            {
                if (value == _inputFileName) return;
                _inputFileName = value;
                this.SetOutputFileName();
                RaisePropertyChanged();
            }
        }

        public string OutputFileName
        {
            get { return _outputFileName; }
            set
            {
                if (value == _outputFileName) return;
                _outputFileName = value;
                RaisePropertyChanged();
            }
        }

        public int ThinFactor
        {
            get { return _thinFactor; }
            set
            {
                if (value == _thinFactor) return;
                _thinFactor = value;
                RaisePropertyChanged();
            }
        }

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

        #endregion

        public AppViewModel()
        {
            this.IsNotBusy = true;
            this.ThinFactor = 10;
        }

        /// <summary>
        /// Called when the input filename has been changed in the property setter, this method automatically
        /// creates a new output file name in the same directory with a suffix before the file extension
        /// </summary>
        private void SetOutputFileName()
        {
            string extension = Path.GetExtension(this.InputFileName);
            string filename = Path.GetFileNameWithoutExtension(this.InputFileName);
            string directory = Path.GetDirectoryName(this.InputFileName);
            string newName = filename + "_thinned" + extension;
            this.OutputFileName = Path.Combine(directory, newName);
        }

        public void ExecuteThinning()
        {
            this.IsNotBusy = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += WorkerOnProgressChanged;
            worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            worker.DoWork += WorkerOnDoWork;
            worker.RunWorkerAsync();
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            int lineCount = 0;

            long fileSize = new FileInfo(this.InputFileName).Length;
            long readSize = 0;
            List<string> buffer = new List<string>();

            using (StreamReader reader = new StreamReader(this.InputFileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    readSize += ASCIIEncoding.ASCII.GetByteCount(line);

                    lineCount++;
                    if (lineCount % this.ThinFactor == 0)
                    {
                        buffer.Add(line);
                    }

                    if (buffer.Count >= 500)
                    {
                        File.AppendAllLines(this.OutputFileName, buffer);
                        buffer.Clear();
                        this.WorkerOnProgressChanged(sender, new ProgressChangedEventArgs(1, (double)(readSize) / fileSize));
                    }
                }
                File.AppendAllLines(this.OutputFileName, buffer);

            }
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            this.IsNotBusy = true;
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            this.DisplayText = string.Format("{0:0.00}%", (double) progressChangedEventArgs.UserState*100);
            
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}