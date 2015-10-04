using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using Section_and_Clustering.Annotations;
using Section_and_Clustering.Models;

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
        private int _minCount;

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
            this.Tolerance = 2.5;
            this.MinCount = 12;
            this.IsNotBusy = true;
        }

        public void ExecuteClustering()
        {
            this.IsNotBusy = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += WorkerOnProgressChanged;
            worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            worker.DoWork += WorkerOnDoWork;
            worker.RunWorkerAsync();
        }

        private List<Point3D> LoadPointsFromFile(string filename)
        {
            Regex extractor = new Regex(@"-{0,1}\d*\.\d+");

            List<Point3D> points = new List<Point3D>();
            foreach (string line in File.ReadAllLines(filename))
            {
                var matches = extractor.Matches(line);
                if (matches.Count != 3)
                {
                    throw new ArgumentException("Error, wrong number of matches in line: " + line);
                }

                Point3D p = new Point3D(double.Parse(matches[0].ToString()), double.Parse(matches[1].ToString()), double.Parse(matches[2].ToString()));
                points.Add(p);
            }
            return points;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            // Prepare the output file path
            string outputPath = Path.GetDirectoryName(this.InputFile);
            string outputName = Path.GetFileNameWithoutExtension(this.InputFile) + ".cluster";
            string outputFile = Path.Combine(outputPath, outputName);


            // Load the points 
            List<Point3D> rawPoints = LoadPointsFromFile(this.InputFile);

            // Collapse the points to the central Z
            var pointZValues = from x in rawPoints select x.Z;
            double centralZ = (pointZValues.Max() + pointZValues.Min())/2.0;
            List<Point3D> freePoints = (from x in rawPoints select new Point3D(x.X, x.Y, centralZ)).ToList();

            // Begin the clustering!
            List<Cluster> clusters = (from x in freePoints select new Cluster(x)).ToList();

            bool action = true;
            int iteration = 0;
            int merges = 0;

            double toleranceInMeters = this.Tolerance/100.0;

            while (action)
            {
                action = false;

                // Iterate through all of the clusters, checking each one against all of the other clusters to see 
                // if any valid merges can be made.
                for (int i = 0; i < clusters.Count; i++)
                {
                    // We start with an empty list of valid merges.  As we go through the clusters will we check each one for
                    // clusters that are below the minimum tolerance distance and add them to the list of valid merges.  When 
                    // we have finished going through the clusters we will see if there are any valid merges, and if there are 
                    // we will perform the merges and construct a new list of clusters, beginning the process again.
                    List<int> validMerges = new List<int>();
                    for (int j = 0; j < clusters.Count; j++)
                    {
                        if (i == j)
                            continue;
                        if (Cluster.GetMinDistance(clusters[i], clusters[j]) < toleranceInMeters)
                        {
                            validMerges.Add(j);
                        }
                    }

                    // If there are valid merges we must now perform them.
                    if (validMerges.Any())
                    {
                        merges += validMerges.Count;
                        validMerges.Add(i);

                        var clustersToMerge = from x in validMerges select clusters[x];
                        Cluster merged = new Cluster(clustersToMerge);

                        // Construct the new list of clusters without any of the ones that were merged

                        List<Cluster> newClusterList = new List<Cluster> {merged};

                        // Now add every cluster after i
                        for (int j = i; j < clusters.Count; j++)
                        {
                            if (!validMerges.Contains(j))
                                newClusterList.Add(clusters[j]);
                        }

                        // Now add every cluster before i, which presumably has no merges
                        for (int j = 0; j < i; j++)
                        {
                            if (!validMerges.Contains(j))
                                newClusterList.Add(clusters[j]);
                        }

                        clusters = newClusterList;
                        action = true;
                        break;
                    }
                }

                // Count the total points
                int totalPoints = 0;
                foreach (var cluster in clusters)
                {
                    totalPoints += cluster.Count;
                }

                iteration++;
                string updateText = string.Format("Clusters: {0}, Merges: {1}, Total Points: {3}, Iteration {2}", clusters.Count, merges, iteration, totalPoints);
                WorkerOnProgressChanged(sender, new ProgressChangedEventArgs(0, updateText));

            }

            File.WriteAllText(outputFile, JsonConvert.SerializeObject(clusters, Formatting.Indented));
        }

        


        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            this.IsNotBusy = true;
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            this.ProgressText = (string) progressChangedEventArgs.UserState;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}