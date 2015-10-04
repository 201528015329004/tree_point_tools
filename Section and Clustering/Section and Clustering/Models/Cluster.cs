using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;

namespace Section_and_Clustering.Models
{
    [JsonConverter(typeof(ClusterConverter))]
    public class Cluster
    {
        private List<Point3D> points;

        public int Count => points.Count;

        public List<Point3D> Points => points;

        /// <summary>
        /// Create an empty cluster
        /// </summary>
        public Cluster()
        {
            this.points = new List<Point3D>();
        }

        /// <summary>
        /// Create a cluster with a single Point3D
        /// </summary>
        /// <param name="p"></param>
        public Cluster(Point3D p)
        {
            this.points = new List<Point3D>{p};
        }

        /// <summary>
        /// Create a cluster from the merge of two clusters
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public Cluster(Cluster a, Cluster b)
        {
            this.points = new List<Point3D>();
            this.points.AddRange(a.points);
            this.points.AddRange(b.points);
        }

        public Cluster(IEnumerable<Cluster> merges)
        {
            this.points = new List<Point3D>();
            foreach (var cluster in merges)
            {
                this.points.AddRange(cluster.points);
            }
        }

        public Cluster(IEnumerable<Point3D> initialPoints)
        {
            this.points = new List<Point3D>(initialPoints);
        }

        public IEnumerable<string> GetTextEnumerable()
        {
            return from x in this.points select string.Format("{0} {1} {2}", x.X, x.Y, x.Z);
        } 

        /// <summary>
        /// Get the minimum distance between two clusters
        /// </summary>
        /// <param name="a">Cluster A</param>
        /// <param name="b">Cluster B</param>
        /// <returns>Minimum straight-line distance between the two clusters of points</returns>
        public static double GetMinDistance(Cluster a, Cluster b)
        {
            // Compute all distances
            List<double> distances = new List<double>();
            foreach (Point3D pointA in a.points)
            {
                foreach (Point3D pointB in b.points)
                {
                    distances.Add(pointA.DistanceTo(pointB));
                }
            }

            return distances.Min();
        }
    }
}