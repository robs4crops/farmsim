using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace LibRobotPath
{
    public class Point
    {
        public readonly double X, Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point pStart
        {
            get { return this; }
        }

        public Point pEnd
        {
            get { return this; }
        }

        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public double Distance(Point p, out double U)
        {
            U = 0;
            return Point.Distance(this, p);
        }
    }

    public interface PathSegment
    {
        double Distance(Point p, out double U);
        Point pStart { get; }
        Point pEnd { get; }
        PathSegment Next { get; set; }

        Point PointAtDistance(double distance);

        double Length { get; }

        string LaneType { get; set; }

    }

    /// <summary>
    /// An instance of this kind of segment will always tell you 
    /// that you are right on course (distance is 0), so 
    /// if you are following this segment, you will not do any steering.
    /// </summary>
    public class NullSegment : PathSegment
    {
        private PathSegment nextSegment;

        public PathSegment Next
        {
            get { return nextSegment; }
            set { nextSegment = value; }
        }

        public virtual double Distance(Point p, out double U)
        {
            U = 0.5;
            return 0;
        }

        public virtual Point PointAtDistance(double distance)
        {
            throw new NotImplementedException();
        }

        public virtual Point pStart { get { return null; } }

        public virtual Point pEnd { get { return null; } }

        public virtual double Length { get { return 0; } }

        private string laneType = "";
        public virtual string LaneType { get { return laneType; } set { laneType = value; } }
    }

    public enum IntersectResult
    {
        Parallel,
        Coincident,
        SegmentIntersection,
        LineIntersection,
        LineSegmentIntersection,
        SegmentLineIntersection
    }

    public class Line : NullSegment
    {
        public readonly Point P1, P2;
        private double length;
        private readonly Point pointOnRight;
        private double theta;

        public Line(Point p1, Point p2, Point pointOnRight)
        {
            P1 = p1;
            P2 = p2;
            this.pointOnRight = pointOnRight;

            length = Point.Distance(P1, P2);
            theta = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }

        public Line(Point p1, Point p2)
            : this(p1, p2, null)
        {
        }

        public override Point pStart
        {
            get { return P1; }
        }

        public override Point pEnd
        {
            get { return P2; }
        }

        public override double Length { get { return length; } }

        public override Point PointAtDistance(double distance)
        {
            double x = P1.X + Math.Cos(theta) * distance;
            double y = P1.Y + Math.Sin(theta) * distance;
            return new Point(x, y);
        }

        public override double Distance(Point point, out double U)
        {
            // After http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
            // seems to have moved to  http://paulbourke.net/geometry/pointlineplane/ 
            U = (((point.X - P1.X) * (P2.X - P1.X)) + ((point.Y - P1.Y) * (P2.Y - P1.Y))) / (Length * Length);

            Point intersection = new Point(
                P1.X + U * (P2.X - P1.X),
                P1.Y + U * (P2.Y - P1.Y)
            );

            double distance = Point.Distance(point, intersection);

            if (pointOnRight != null)
            {
                IntersectResult ir = Intersect(this, new Line(point, pointOnRight));
                if (ir == IntersectResult.SegmentIntersection || ir == IntersectResult.LineSegmentIntersection)
                    distance = -distance;
            }

            return distance;

        }

        public static IntersectResult Intersect(Line l1, Line l2)
        {
            //After http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/example.cpp            

            const double tol = 0.000001;

            double denom = ((l2.pEnd.Y - l2.pStart.Y) * (l1.pEnd.X - l1.pStart.X)) -
                          ((l2.pEnd.X - l2.pStart.X) * (l1.pEnd.Y - l1.pStart.Y));

            double nume_a = ((l2.pEnd.X - l2.pStart.X) * (l1.pStart.Y - l2.pStart.Y)) -
                           ((l2.pEnd.Y - l2.pStart.Y) * (l1.pStart.X - l2.pStart.X));

            double nume_b = ((l1.pEnd.X - l1.pStart.X) * (l1.pStart.Y - l2.pStart.Y)) -
                           ((l1.pEnd.Y - l1.pStart.Y) * (l1.pStart.X - l2.pStart.X));

            if (Math.Abs(denom) < tol)
            {
                if (Math.Abs(nume_a) < tol && Math.Abs(nume_b) < tol)
                    return IntersectResult.Coincident;
                else
                    return IntersectResult.Parallel;
            }

            double ua = nume_a / denom;
            double ub = nume_b / denom;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                //// Get the intersection point.
                //intersection.X = l1.pStart.X + ua * (l1.pEnd.X - l1.pStart.X);
                //intersection.Y = l1.pStart.Y + ua * (l1.pEnd.Y - l1.pStart.Y);

                return IntersectResult.SegmentIntersection;
            }
            else if (ua >= 0 && ua <= 1)
                return IntersectResult.SegmentLineIntersection;
            else if (ub >= 0 && ub <= 1)
                return IntersectResult.LineSegmentIntersection;
            else
                return IntersectResult.LineIntersection;

        }
    }

    public class Circle : NullSegment
    {
        public readonly Point Center, P1, P2;
        public readonly double Radius;
        private bool rightTurn;
        private Line line;
        private double length;

        private Coordinate c_center, c_p1, c_p2;

        public Circle(Point center, Point p1, Point p2, bool rightTurn)
        {
            this.Center = center;
            this.P1 = p1;
            this.P2 = p2;
            this.rightTurn = rightTurn;

            line = new Line(p1, p2);
            Radius = Point.Distance(Center, P1);

            c_center = new Coordinate(Center.X, Center.Y);
            c_p1 = new Coordinate(p1.X, p1.Y);
            c_p2 = new Coordinate(p2.X, p2.Y);
            double angle = AngleUtility.AngleBetweenOriented(c_p1, c_center, c_p2);
            length = angle / (Math.PI * 2) * 2 * Math.PI * Radius;

        }

        public override Point PointAtDistance(double distance)
        {
            double u = distance / (2 * Math.PI * Radius);
            double angle = u * 2 * Math.PI;
            Coordinate c = AngleUtility.Project(c_p1, angle, Radius);
            return new Point (c.X, c.Y);    
        }

        public override double Distance(Point p, out double U)
        {
            double d = Center.Distance(p, out U) - Radius;
            // if d>0 then robot is outside the circle
            // for a right-turn, this is a negative error
            if (rightTurn)
                d = -d;
            
            
            U = 0.5;// TODO - but not currently used


            return d;
        }

        public override Point pStart
        {
            get { return P1; }
        }

        public override Point pEnd
        {
            get { return P2; }
        }

        public override double Length { get { return length; } }
    }

}



