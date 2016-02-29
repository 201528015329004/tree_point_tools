using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace Large_File_Transformations
{
    public class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties 
        private string _transformationString;
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

        public string TransformationString
        {
            get { return _transformationString; }
            set
            {
                if (value == _transformationString) return;
                _transformationString = value;
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
            this.TransformationString = "0, 0, 0, 0";
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
            string newName = filename + "_transformed" + extension;
            this.OutputFileName = Path.Combine(directory, newName);
        }

        public void ExecuteTransformation()
        {
            if (!TransformationStringParser.Parse(this.TransformationString).IsValid)
                return;

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

            // Load the transforms
            var transform = TransformationStringParser.Parse(this.TransformationString);
            Angle angle = new Angle(transform.Rz, new Degrees());
            Vector3D rotationAxis = new Vector3D(0, 0, 1);
            Vector3D shift = new Vector3D(transform.Tx, transform.Ty, transform.Tz);

            Regex extractor = new Regex(@"-{0,1}\d*\.\d+");

            using (StreamReader reader = new StreamReader(this.InputFileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    readSize += ASCIIEncoding.ASCII.GetByteCount(line);

                    // Do the work here and add it to buffer
                    var matches = extractor.Matches(line);
                    if (matches.Count != 3)
                    {
                        throw new ArgumentException("Error, wrong number of matches in line: " + line);
                    }

                    var p = new Point3D(double.Parse(matches[0].ToString()), double.Parse(matches[1].ToString()), double.Parse(matches[2].ToString()));
                    // Point3D t = (p.Rotate(rotationAxis, angle)) + shift;
                    Point3D t = (p + shift).Rotate(rotationAxis, angle);
                    buffer.Add(t.X + " " + t.Y + " " + t.Z);
                    
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
            this.DisplayText = "Done";
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            this.DisplayText = string.Format("{0:0.00}%", (double)progressChangedEventArgs.UserState * 100);

        }


        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}