using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
        private string _pointFile;

        public string PointFile
        {
            get { return _pointFile; }
            set
            {
                if (value == _pointFile) return;
                _pointFile = value;
                RaisePropertyChanged();
            }
        }

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
            this.MaxRadius = 20;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            string outputPath = Path.GetDirectoryName(this.InputFile);
            string outputName = Path.GetFileNameWithoutExtension(this.InputFile) + "_filtered_clusters.xyz";
            string outputFile = Path.Combine(outputPath, outputName);

            var filtered = this.clusters.Where(x => x.Count >= this.MinCount).ToList();
            List<TrunkData> trunks = new List<TrunkData>();

           /* var output = new List<string>();
            foreach (var cluster in filtered)
            {
               output.AddRange(cluster.GetTextEnumerable());
            }
            File.WriteAllLines(outputFile, output);*/

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

                count++;
                trunks.Add(new TrunkData {Center = fit.CircleCenter, Radius = fit.CircleRadius, Points = cluster.Points, IdNumber = count});

                double scale = 1000;//39.37069644322352;
                string csvLine =
                    string.Format(
                        "'circle';'Circle {0}';{1};{2};{3};'';'';'';0.00;0.00;1.00;'';'';'';'';'';'';'';'';{4};'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';'';''",
                        count, fit.CircleCenter.X * scale, fit.CircleCenter.Y * scale, fit.CircleCenter.Z * scale, fit.CircleRadius * scale);
                csvOutput.Add(csvLine.Replace('\'', '"'));
            }

            File.WriteAllLines(outputFile + ".csv", csvOutput);

            string display = string.Format("{0} filtered trunks, no points to process", trunks.Count);
            WorkerOnProgressChanged(sender, new ProgressChangedEventArgs(0, display));

            // Start writing results 
            string resultPath = Path.Combine(outputPath, "results");
            if (!Directory.Exists(resultPath))
                Directory.CreateDirectory(resultPath);
            File.WriteAllText(Path.Combine(resultPath, "trunks.json"), JsonConvert.SerializeObject(trunks, Formatting.Indented));

            // If the point source file exists, start sorting through that
            if (!File.Exists(this.PointFile))
                return;

            long fileSize = new FileInfo(this.PointFile).Length;
            long readSize = 0;
            Regex extractor = new Regex(@"-{0,1}\d*\.\d+");
            int pointCount = 0;
            int sorted = 0;
            Dictionary<int, List<Point3D>> sortedPoints = new Dictionary<int, List<Point3D>>();
            using (StreamReader reader = new StreamReader(this.PointFile))
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

                    Point3D v = new Point3D(double.Parse(matches[0].ToString()), double.Parse(matches[1].ToString()), double.Parse(matches[2].ToString()));
                    Point2D p = new Point2D(v.X, v.Y);
                    foreach (var trunkData in trunks)
                    {
                        Point2D c = new Point2D(trunkData.Center.X, trunkData.Center.Y);
                        if (c.DistanceTo(p) < 0.75)
                        {
                            sorted++;
                            if (sortedPoints.ContainsKey(trunkData.IdNumber))
                                sortedPoints[trunkData.IdNumber].Add(v);
                            else
                            {
                                sortedPoints[trunkData.IdNumber] = new List<Point3D> {v};
                            }
                        }
                    }

                    if (pointCount++%50 == 0)
                    {
                        display = string.Format("{0} filtered trunks, {2} sorted points, {1:0.00}% processed", trunks.Count, (double)readSize/fileSize * 100, sorted);
                        WorkerOnProgressChanged(sender, new ProgressChangedEventArgs(0, display));
                    }

                }
            }

            // Save the sorted points
            foreach (KeyValuePair<int, List<Point3D>> keyValuePair in sortedPoints)
            {
                string saveName = Path.Combine(resultPath, "Trunk " + keyValuePair.Key.ToString() + ".xyz");
                var lines = from x in keyValuePair.Value select string.Format("{0} {1} {2}", x.X, x.Y, x.Z);
                File.WriteAllLines(saveName, lines);
            }

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