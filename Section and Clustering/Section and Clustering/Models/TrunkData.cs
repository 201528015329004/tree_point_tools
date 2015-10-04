using System.Collections.Generic;
using System.Windows;
using MathNet.Spatial.Euclidean;

namespace Section_and_Clustering.Models
{
    public class TrunkData
    {
        public Point3D Center { get; set; }
        public double Radius { get; set; }
        public List<Point3D> Points { get; set; }
        public int IdNumber { get; set; }

        public TrunkData()
        {
            this.Points = new List<Point3D>();
        }
    }
}