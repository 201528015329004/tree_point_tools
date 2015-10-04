using System.Collections.Generic;
using System.Linq;
using LMDotNet;
using MathNet.Spatial.Euclidean;

namespace Section_and_Clustering.Models
{
    public class CircleFitter2D
    {
        private List<Point3D> points;

        public Point3D CircleCenter { get; set; }
        public double CircleRadius { get; set; }

        public CircleFitter2D(IEnumerable<Point3D> initialPoints)
        {
            this.points = initialPoints.ToList();

            // Compute point centroid
            Point3D center = Point3D.Centroid(this.points);
            var distances = from x in this.points select x.DistanceTo(center);
            double radius = distances.Average();

            var solver = new LMSolver();

            // Guess for x, y, and r
            double[] guess = new[] {center.X, center.Y, radius};

            var fit = solver.Minimize((p, r) =>
            {
                // unpack the minimization parameters, x, y, and r
                double x = p[0];
                double y = p[1];
                double rad = p[2];

                Point3D testCenter = new Point3D(x, y, this.points.First().Z);
                // compute the residuals
                for (int i = 0; i < this.points.Count; i++)
                {
                    r[i] = this.points[i].DistanceTo(testCenter) - rad;
                }

            }, guess, this.points.Count);

            this.CircleCenter = new Point3D(fit.OptimizedParameters[0], fit.OptimizedParameters[1], this.points.First().Z);
            this.CircleRadius = fit.OptimizedParameters[2];

        }


    }
}