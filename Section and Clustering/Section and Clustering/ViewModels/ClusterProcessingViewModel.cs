using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using Section_and_Clustering.Annotations;
using Section_and_Clustering.Models;

namespace Section_and_Clustering.ViewModels
{
    public class ClusterProcessingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties 
        private int _minCount;
        private string _inputFile;
        private bool _isNotBusy;
        private List<Cluster> clusters;
        private string _displayText;
        private int _maxRadius;
        private int _minRadius;

        public int MinRadius
        {
            get { return _minRadius; }
            set
            {
                if (value == _minRadius) return;
                _minRadius = value;
                RaisePropertyChanged();
            }
        }

        public int MaxRadius
        {
            get { return _maxRadius; }
            set
            {
                if (value == _maxRadius) return;
                _maxRadius = value;
                RaisePropertyChanged();
            }
        }

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

        public int ClusterCount
        {
            get
            {
                if (this.clusters != null)
                    return this.clusters.Count;
                else
                    return 0;
            }
        }

        public int ClusterPoints
        {
            get
            {
                if (this.clusters != null)
                {
                    return this.clusters.Sum(cluster => cluster.Count);
                }
                else
                    return 0;
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
                this.LoadClusters();
            }
        }

        public int MinCount
        {
            get { return _minCount; }
            set
            {
                if (value == _minCount) return;
                _minCount = value;
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

        public ClusterProcessingViewModel()
        {
            this.MinCount = 10;
            this.IsNotBusy = true;

            this.MinRadius = 5;
            this.MaxRadius = 50;
        }

        public void Process()
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
            string outputPath = Path.GetDirectoryName(this.InputFile);
            string outputName = Path.GetFileNameWithoutExtension(this.InputFile) + "_filtered_clusters.xyz";
            string outputFile = Path.Combine(outputPath, outputName);

            var filtered = this.clusters.Where(x => x.Count >= this.MinCount).ToList();

            var output = new List<string>();
            foreach (var cluster in filtered)
            {
               output.AddRange(cluster.GetTextEnumerable());
            }
            File.WriteAllLines(outputFile, output);

            // Fit the circles
            List<string> csvOutput = new List<string>();
            csvOutput.Add("'type';'name';'coord-x';'coord-y';'coord-z';'coord-x2';'coord-y2';'coord-z2';'normal-x';'normal-y';'normal-z';'trimming-x';'trimming-y';'trimming-z';'dir-x';'dir-y';'dir-z';'length';'width';'radius';'radius2';'angle';'orientation';'edge-radius';'num-points';'tol-x-lower';'tol-x-upper';'tol-y-lower';'tol-y-upper';'tol-z-lower';'tol-z-upper';'tol-all-lower';'tol-all-upper';'tol-normal-lower';'tol-normal-upper';'tol-trimming-lower';'tol-trimming-upper';'tol-inplane-lower';'tol-inplane-upper';'tol-length-lower';'tol-length-upper';'tol-width-lower';'tol-width-upper';'tol-diameter-lower';'tol-diameter-upper';'tol-angle-lower';'tol-angle-upper'".Replace('\'', '"'));
            int count = 0;
            foreach (var cluster in filtered)
            {
                var fit = new CircleFitter2D(cluster.Points);

                if (fit.CircleRadius < this.MinRadius/100.0)
                    continue;
                if (fit.CircleRadius > this.MaxRadius/100.0)
                    continue;


                double scale = 1000;//39.37069644322352;
                string csvLine =
                    string.Format(
                        "'circle';'Circle {0}';{1};{2};{3};'';'';'';0.00;0.00;1.00;'';'';'';'';'';'';'';'';{4};'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';''",
                        count++, fit.CircleCenter.X * scale, fit.CircleCenter.Y * scale, fit.CircleCenter.Z * scale, fit.CircleRadius * scale);
                csvOutput.Add(csvLine.Replace('\'', '"'));
            }

            File.WriteAllLines(outputFile + ".csv", csvOutput);

            string display = string.Format("{0} clusters min size", filtered.Count);
            WorkerOnProgressChanged(sender, new ProgressChangedEventArgs(0, display));
        }

        private void LoadClusters()
        {
            this.clusters = JsonConvert.DeserializeObject<List<Cluster>>(File.ReadAllText(this.InputFile));
            this.RaisePropertyChanged("ClusterCount");
            this.RaisePropertyChanged("ClusterPoints");
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            this.IsNotBusy = true;
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            this.DisplayText = (string) progressChangedEventArgs.UserState;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}