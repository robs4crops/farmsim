using System;
using System.Collections.Generic;
using System.Text;

namespace LibRobotPath
{

    public class RobotPath
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="heading">heading, degrees (0=N, 90=E</param>
        /// <param name="length">length of leg, m</param>
        /// <param name="radius">radius of circle, m</param>
        /// <param name="leftFirst">first turn is to the left?</param>
        /// <returns></returns>
        public static PathSegment CreateRuudPath(Point currentPosition, double heading, double length, double radius, bool leftFirst, int nLegs)
        {
            List<PathSegment> segments = new List<PathSegment>();

            PathSegment segment;
            Point p2;
            Point c;
            double turnSize = Math.PI / 2;
            bool turnLeft = leftFirst;
            double currTurn;

            heading = heading * Math.PI / 180;

            for (int i = 0; i < nLegs; i++)
            {

                // line
                p2 = new LibRobotPath.Point(
                    currentPosition.X + length * Math.Sin(heading),
                    currentPosition.Y + length * Math.Cos(heading)
                );
                LibRobotPath.Point pointOnRight = new Point(
                    currentPosition.X + length * Math.Sin(heading + Math.PI / 4),
                    currentPosition.Y + length * Math.Cos(heading + Math.PI / 4)
                );
                segment = new Line(currentPosition, p2, pointOnRight);
                segments.Add(segment);
                currentPosition = segment.pEnd;

                // turn
                if (turnLeft)
                    currTurn = -turnSize;
                else
                    currTurn = turnSize;

                heading += currTurn;
                c = new Point(
                    currentPosition.X + 1 * radius * Math.Sin(heading),
                    currentPosition.Y + 1 * radius * Math.Cos(heading)
                );
                p2 = new Point(
                    currentPosition.X + 2 * radius * Math.Sin(heading),
                    currentPosition.Y + 2 * radius * Math.Cos(heading)
                );
                segment = new Circle(c, segment.pEnd, p2, !turnLeft);
                segments.Add(segment);
                currentPosition = segment.pEnd;
                heading += currTurn;

                turnLeft = !turnLeft;

            }

            for (int i = 0; i < segments.Count - 1; i++)
                segments[i].Next = segments[i + 1];
            segments[segments.Count - 1].Next = null;

            return segments[0];

        }



        List<PathSegment> segments = new List<PathSegment>();

        public void CreatePath(Point currentPosition, double heading, double length, double radius, bool leftFirst)
        {
            segments.Clear();

            PathSegment newSegment;
            Point p2;
            Point c;
            double turnSize = Math.PI / 2;
            bool turnLeft = leftFirst;
            double currTurn;


            for (int i = 0; i < 5; i++)
            {

                // line
                p2 = new Point(
                    currentPosition.X + length * Math.Sin(heading),
                    currentPosition.Y + length * Math.Cos(heading)
                );
                LibRobotPath.Point pointOnRight = new Point(
                    currentPosition.X + length * Math.Sin(heading + Math.PI / 4),
                    currentPosition.Y + length * Math.Cos(heading + Math.PI / 4)
                );
                newSegment = new Line(currentPosition, p2, pointOnRight);
                segments.Add(newSegment);
                currentPosition = newSegment.pEnd;

                // turn
                if (turnLeft)
                    currTurn = -turnSize;
                else
                    currTurn = turnSize;
                // generate a more-or-less random point in the half-plane 
                // in which the half-circle lies that we need to traverse 
                LibRobotPath.Point px = new Point(
                    currentPosition.X + 2 * radius * Math.Sin(heading + currTurn / 2),
                    currentPosition.Y + 2 * radius * Math.Cos(heading + currTurn / 2)
                );

                heading += currTurn;
                c = new Point(
                    currentPosition.X + 1 * radius * Math.Sin(heading),
                    currentPosition.Y + 1 * radius * Math.Cos(heading)
                );
                p2 = new Point(
                    currentPosition.X + 2 * radius * Math.Sin(heading),
                    currentPosition.Y + 2 * radius * Math.Cos(heading)
                );
                newSegment = new Circle(c, newSegment.pEnd, p2, !turnLeft);
                segments.Add(newSegment);
                currentPosition = newSegment.pEnd;
                heading += currTurn;

                turnLeft = !turnLeft;

            }
        }

        public List<PathSegment> Segments
        {
            get { return segments; }
        }
    }


}
