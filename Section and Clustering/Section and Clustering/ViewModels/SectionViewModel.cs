using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MathNet.Spatial.Euclidean;
using Section_and_Clustering.Annotations;

namespace Section_and_Clustering.ViewModels
{
    public class SectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

    #region Properties
        private string _inputFile;
        private double _zDatum;
        private double _tolerance;
        private double _progressPercentage;
        private bool _isNotBusy;

        public string ProgressText
        {
            get { return string.Format("{0:0.00}", ProgressPercentage); }
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

        public double ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                if (value.Equals(_progressPercentage)) return;
                _progressPercentage = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ProgressText");
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

        public double ZDatum
        {
            get { return _zDatum; }
            set
            {
                if (value.Equals(_zDatum)) return;
                _zDatum = value;
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

        public SectionViewModel()
        {
            this.ZDatum = 7.4;
            this.Tolerance = 2.0;
            this.IsNotBusy = true;
        }

        public void ExecuteSectioning()
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
            long fileSize = new FileInfo(this.InputFile).Length;
            long readSize = 0;
            long count = 0;
            List<string> filteredLines = new List<string>();

            Regex extractor = new Regex(@"-{0,1}\d*\.\d+");

            double upperLimit = this.ZDatum + (this.Tolerance/100.0);
            double lowerLimit = this.ZDatum - (this.Tolerance/100.0);

            using (StreamReader reader = new StreamReader(this.InputFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    readSize += ASCIIEncoding.ASCII.GetByteCount(line);

                    var matches = extractor.Matches(line);
                    if (matches.Count != 3)
                    {
                        throw new ArgumentException("Error, wrong number of matches in line: " + line);
                    }

                    Vector3D v = new Vector3D(double.Parse(matches[0].ToString()), double.Parse(matches[1].ToString()), double.Parse(matches[2].ToString()));
                    if (v.Z <= upperLimit && v.Z >= lowerLimit)
                    {
                        filteredLines.Add(line);
                    }

                    if (count++%500 == 0)
                    {
                        WorkerOnProgressChanged(this, new ProgressChangedEventArgs(0, (double)readSize / fileSize));
                    }
                }
            }

            string outputPath = Path.GetDirectoryName(this.InputFile);
            string outputName = Path.GetFileNameWithoutExtension(this.InputFile);
            string outputFile = Path.Combine(outputPath, outputName + "_section.xyz");
            File.WriteAllLines(outputFile, filteredLines);
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            this.IsNotBusy = true;
            this.ProgressPercentage = 0;
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            this.ProgressPercentage = (double) progressChangedEventArgs.UserState * 100;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}