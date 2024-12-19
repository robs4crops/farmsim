using LibRobotPath;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NModcom;

namespace libFarmSim
{
    public class Vehicle : UpdateableSimObj
    {

        public const string LANETYPE_WORKING = "working";
        public const string LANETYPE_NONWORKING = "non-working";
        public const string LANETYPE_TRANSPORT = "transport";

        [Input("MaxVehicleSpeed")]
        IData MaxSpeed;

        [Input("VehicleRouteJson")]
        IData RouteJson;

        [Input("TargetVelocity")]
        IData TargetVelocity;

        [Output("X")]
        readonly IData X = new ConstFloatSimData(0);

        [Output("Y")]
        readonly IData Y = new ConstFloatSimData(0);

        [Output("CurrentSpeed")]
        readonly IData CurrentSpeed = new ConstFloatSimData(0);

        [Output("PathLength")]
        readonly IData PathLength = new ConstFloatSimData(0); // total length of the path, m

        [Output("DistanceTraveled")]
        readonly IData DistanceTraveled = new ConstFloatSimData(0); // total distance traveled, m

        [Output("DistanceToGo")]
        readonly IData DistanceToGo = new ConstFloatSimData(0);//  distance still to be traveled, m

        [Output("TimeTraveled")]
        readonly IData TimeTraveled = new ConstFloatSimData(0); // time traveled, s

        [Output("TimeWorking")]
        readonly IData TimeWorking = new ConstFloatSimData(0); // time spent in working lane, s

        [Output("TimeNonWorking")]
        readonly IData TimeNonWorking = new ConstFloatSimData(0); // time spent in the field but not in a working lane, s

        [Output("TimeTransport")]
        readonly IData TimeTransport = new ConstFloatSimData(0); // time spent in transport lane, s

        [Output("TimeToGo")]
        readonly IData TimeToGo = new ConstFloatSimData(0); // time needed to complete the path, extrapolated from realized average speed and distance-to-go, s

        [Output("DistanceWorking")]
        readonly IData DistanceWorking = new ConstFloatSimData(0); // distance traveled in working lane, m

        [Output("DistanceNonWorking")]
        readonly IData DistanceNonWorking = new ConstFloatSimData(0); // distance traveled in the field but not in a working lane, m

        [Output("DistanceTransport")]
        readonly IData DistanceTransport = new ConstFloatSimData(0); // distance traveled in transport lane, m

        [Output("FuelUsed")]
        readonly IData FuelUsed = new ConstFloatSimData(0); // Fuel used, l

        private double speed;  // m/s
        private RobotPath? robotPath;
        private int currSegment;
        private double distanceInSegment; // total distance traveled in current segment
        private double distanceWorking, distanceNonWorking, distanceTransport;
        private double timeWorking, timeNonWorking, timeTransport;

        const double FUEL_L_PER_HOUR = 2.5;
        const double FUEL_L_PER_SECOND = FUEL_L_PER_HOUR / 3600;

        public override void StartRun()
        {
            base.StartRun();

            CreateSimplePath();

            currSegment = 0;
            distanceInSegment = 0;

            distanceWorking = 0;
            distanceNonWorking = 0;
            distanceTransport = 0;

            timeWorking = 0;
            timeNonWorking = 0;
            timeTransport = 0;

            PathSegment startSegment = robotPath.Segments[currSegment];
            X.AsFloat = startSegment.pStart.X;
            Y.AsFloat = startSegment.pStart.Y;

            speed = TargetVelocity.AsFloat;

            CurrentSpeed.AsFloat = speed;
            DistanceTraveled.AsFloat = 0;
            DistanceToGo.AsFloat = PathLength.AsFloat;
            TimeTraveled.AsFloat = 0;
            TimeToGo.AsFloat = PathLength.AsFloat / TargetVelocity.AsFloat;

            TimeWorking.AsFloat = 0;
            TimeNonWorking.AsFloat = 0;
            TimeTransport.AsFloat = 0;

            DistanceWorking.AsFloat = 0;
            DistanceNonWorking.AsFloat = 0;
            DistanceTransport.AsFloat = 0;

            FuelUsed.AsFloat = 0;
        }
        private void CreateSimplePath()
        {
            robotPath = new RobotPath();
            double plength = 0;

            GeoJsonReader geoReader = new GeoJsonReader();
            FeatureCollection fc = geoReader.Read<FeatureCollection>(RouteJson.ToString());
            foreach (Feature f in fc)
            {
                var ls = f.Geometry as MultiLineString;
                Coordinate[] coords = ls.Coordinates;
                for (int i = 1; i < coords.Length; i++)
                {
                    Line line = new Line(
                            new LibRobotPath.Point(coords[i - 1].X, coords[i - 1].Y),
                            new LibRobotPath.Point(coords[i].X, coords[i].Y)
                            );
                    string lanetype = f.Attributes["lanetype"].ToString();
                    line.LaneType = lanetype;
                    robotPath.Segments.Add(line);
                }

                plength += ls.Length;
            }

            PathLength.AsFloat = plength;
        }

        public override void HandleEvent(ISimEvent simEvent)
        {
            ISimEnv simEnv = simEvent.SimEnv;
            double timestepInSeconds = TimeStep * 86400;

            if (currSegment >= robotPath.Segments.Count)
            {
                simEnv.RequestStop();
            }
            else
            {
                // calculate speed
                speed = Math.Min(TargetVelocity.AsFloat, MaxSpeed.AsFloat);
                CurrentSpeed.AsFloat = speed;

                LibRobotPath.Point p = new LibRobotPath.Point(0, 0);

                // move the robot
                double distance = speed * timestepInSeconds;
                double remainingDistance = distance;
                double remainingTime = timestepInSeconds;

                // is this a working lane?
                string lanetype = robotPath.Segments[currSegment].LaneType;

                while (remainingDistance > 0 && currSegment < robotPath.Segments.Count)
                {
                    // putative location of the robot after this time step
                    p = robotPath.Segments[currSegment].PointAtDistance(distanceInSegment + remainingDistance);

                    // does the new location overshoot the current segment?
                    double u;
                    robotPath.Segments[currSegment].Distance(p, out u);

                    if (u <= 1) // new location is on current segment
                    {
                        if (lanetype.Equals(LANETYPE_WORKING))
                        {
                            distanceWorking += remainingDistance;
                            timeWorking += remainingTime;
                        }
                        else if (lanetype.Equals(LANETYPE_NONWORKING))
                        {
                            distanceNonWorking += remainingDistance;
                            timeNonWorking += remainingTime;
                        }
                        else if (lanetype.Equals(LANETYPE_TRANSPORT))
                        {
                            distanceTransport += remainingDistance;
                            timeTransport += remainingTime;
                        }
                        else
                            throw new Exception($"Unknown lanetype '{lanetype}'");

                        distanceInSegment += remainingDistance;
                        remainingDistance = 0;
                        remainingTime = 0;
                    }
                    else // new location overshoots the current segment
                    {
                        // only allot that part of the distance travelled that can fit in the current segment
                        double room = robotPath.Segments[currSegment].Length - distanceInSegment;
                        double f = room / remainingDistance;
                        double time_now = f * remainingTime;

                        //
                        if (lanetype.Equals(LANETYPE_WORKING))
                        {
                            distanceWorking += room;
                            timeWorking += time_now;
                        }
                        else if (lanetype.Equals(LANETYPE_NONWORKING))
                        {
                            distanceNonWorking += room;
                            timeNonWorking += time_now;
                        }
                        else if (lanetype.Equals(LANETYPE_TRANSPORT))
                        {
                            distanceTransport += room;
                            timeTransport += time_now;
                        }
                        else
                            throw new Exception($"Unknown lanetype '{lanetype}'");

                        remainingDistance -= room;
                        remainingTime -= time_now;

                        // now switch to next segment
                        currSegment += 1;
                        distanceInSegment = 0;
                    }

                };

                // output new robot position
                X.AsFloat = p.X;
                Y.AsFloat = p.Y;

                // summarize
                DistanceTraveled.AsFloat = DistanceTraveled.AsFloat + distance;
                TimeTraveled.AsFloat = TimeTraveled.AsFloat + timestepInSeconds;
                double avgSpeed = DistanceTraveled.AsFloat / TimeTraveled.AsFloat;

                DistanceToGo.AsFloat = PathLength.AsFloat - DistanceTraveled.AsFloat;
                TimeToGo.AsFloat = DistanceToGo.AsFloat / avgSpeed;

                TimeWorking.AsFloat = timeWorking;
                TimeNonWorking.AsFloat = timeNonWorking;
                TimeTransport.AsFloat = timeTransport;

                DistanceWorking.AsFloat = distanceWorking;
                DistanceNonWorking.AsFloat = distanceNonWorking;
                DistanceTransport.AsFloat = distanceTransport;

                FuelUsed.AsFloat += timestepInSeconds * FUEL_L_PER_SECOND;

                //double errTime = timeWorking + timeTransport - TimeTraveled.AsFloat;
                //if (Math.Abs(errTime) > 1)
                //    throw new Exception("djflkdjfalk pui2");

            }
        }
    }
}

