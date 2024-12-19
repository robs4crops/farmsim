using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NModcom;
using System.Text;

namespace libFarmSim
{
    public class Sim
    {
        ISimEnv? simEnv;
        Vehicle? vehicle;
        Sprayer? sprayer;
        SpatialDataLayers? spatialData;
        List<SimOutputLine>? simOutputLines;

        public string VehicleRouteJson { get; set; }
        public string ZonesJson { get; set; }

        public int TimestepSec { get; set; }

        public double TargetVelocity { get; set; }
        public Sim()
        {
        }

        private void Setup()
        {
            count = 0;

            double timeStep = (double)TimestepSec / 86400; // convert from seconds to fractional day

            simEnv = new SimEnv();

            spatialData = new SpatialDataLayers() { TimeStep = timeStep };
            simEnv.Add(spatialData);

            vehicle = new Vehicle() { TimeStep = timeStep };
            simEnv.Add(vehicle);

            sprayer = new Sprayer() { TimeStep = timeStep };
            simEnv.Add(sprayer);

            vehicle.Inputs["MaxVehicleSpeed"].Data = spatialData.Outputs["MaxVehicleSpeed"].Data;
            vehicle.Inputs["VehicleRouteJson"].Data = new ConstStringSimData(VehicleRouteJson);
            vehicle.Inputs["TargetVelocity"].Data = new ConstFloatSimData(TargetVelocity);

            spatialData.Inputs["X"].Data = vehicle.Outputs["X"].Data;
            spatialData.Inputs["Y"].Data = vehicle.Outputs["Y"].Data;
            spatialData.Inputs["ZonesJson"].Data = new ConstStringSimData(ZonesJson);

            sprayer.Inputs["InfestationLevel"].Data = spatialData.Outputs["InfestationLevel"].Data;
            sprayer.Inputs["CurrentSpeed"].Data = vehicle.Outputs["CurrentSpeed"].Data;
            sprayer.Inputs["ApplicationRate"].Data.AsFloat = 200;

            DateTime simStart = new(2023, 3, 25, 11, 15, 55);
            DateTime simStop = new(2023, 3, 25, 12, 17, 25);

            simEnv.StartTime = CalendarTime.ToDouble(simStart);
            simEnv.StopTime = CalendarTime.ToDouble(simStop);

        }
        public string Run()
        {
            simOutputLines = new List<SimOutputLine>();

            Setup();

            simEnv.OutputEvent += SimEnv_OutputEvent;
            simEnv.Run();

            double timeLapsedSecs = (simEnv.CurrentTime - simEnv.StartTime) * 86400;
            double timeWorking = simEnv["Vehicle"].Outputs["TimeWorking"].Data.AsFloat;
            double timeNonWorking = simEnv["Vehicle"].Outputs["TimeNonWorking"].Data.AsFloat;
            double timeTransport = simEnv["Vehicle"].Outputs["TimeTransport"].Data.AsFloat;
            double distanceWorking = simEnv["Vehicle"].Outputs["DistanceWorking"].Data.AsFloat;
            double distanceNonWorking = simEnv["Vehicle"].Outputs["DistanceNonWorking"].Data.AsFloat;
            double distanceTransport = simEnv["Vehicle"].Outputs["DistanceTransport"].Data.AsFloat;
            double fuelUsed = simEnv["Vehicle"].Outputs["FuelUsed"].Data.AsFloat;

            simEnv.OutputEvent -= SimEnv_OutputEvent;
            simEnv = null;

            string jsonoutput = JsonConvert.SerializeObject(simOutputLines, Formatting.Indented);

            simOutputLines.Clear();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"Daily\":" + jsonoutput);
            sb.AppendLine(string.Format(", \"TotalTime\": {0}", timeLapsedSecs));
            sb.AppendLine(string.Format(", \"TimeWorking\": {0}", timeWorking));
            sb.AppendLine(string.Format(", \"TimeNonWorking\": {0}", timeNonWorking));
            sb.AppendLine(string.Format(", \"TimeTransport\": {0}", timeTransport));
            sb.AppendLine(string.Format(", \"DistanceWorking\": {0}", distanceWorking));
            sb.AppendLine(string.Format(", \"DistanceNonWorking\": {0}", distanceNonWorking));
            sb.AppendLine(string.Format(", \"DistanceTransport\": {0}", distanceTransport));
            sb.AppendLine(string.Format(", \"FuelUsed\": {0}", fuelUsed));
            sb.AppendLine("}");
            return sb.ToString();
        }

        public SimulationResult RunNew()
        {
            simOutputLines = new List<SimOutputLine>();

            Setup();

            simEnv.OutputEvent += SimEnv_OutputEvent;
            simEnv.Run();


            var result = new SimulationResult()
            {
                TimeLapsedSecs = (simEnv.CurrentTime - simEnv.StartTime) * 86400,
                TimeWorking = simEnv["Vehicle"].Outputs["TimeWorking"].Data.AsFloat,
                TimeNonWorking = simEnv["Vehicle"].Outputs["TimeNonWorking"].Data.AsFloat,
                TimeTransport = simEnv["Vehicle"].Outputs["TimeTransport"].Data.AsFloat,
                DistanceWorking = simEnv["Vehicle"].Outputs["DistanceWorking"].Data.AsFloat,
                DistanceNonWorking = simEnv["Vehicle"].Outputs["DistanceNonWorking"].Data.AsFloat,
                DistanceTransport = simEnv["Vehicle"].Outputs["DistanceTransport"].Data.AsFloat,
                FuelUsed = simEnv["Vehicle"].Outputs["FuelUsed"].Data.AsFloat
            };

            simEnv.OutputEvent -= SimEnv_OutputEvent;
            simEnv = null;
            return result;
        }

        static int count = 0;
        private void SimEnv_OutputEvent(object sender, EventArgs e)
        {
            SimOutputLine outputLine = new SimOutputLine()
            {
                c = count++,
                datetime = CalendarTime.ToDateTime(simEnv.CurrentTime),
                x = vehicle.Outputs["X"].Data.AsFloat,
                y = vehicle.Outputs["Y"].Data.AsFloat,
                speed = vehicle.Outputs["CurrentSpeed"].Data.AsFloat,
                pathlength = vehicle.Outputs["PathLength"].Data.AsFloat,
                distance_traveled = vehicle.Outputs["DistanceTraveled"].Data.AsFloat,
                distance_to_go = vehicle.Outputs["DistanceToGo"].Data.AsFloat,
                time_traveled = vehicle.Outputs["TimeTraveled"].Data.AsFloat,
                time_to_go = vehicle.Outputs["TimeToGo"].Data.AsFloat,
                infestation = spatialData.Outputs["InfestationLevel"].Data.AsFloat,
                flow_rate = sprayer.Outputs["FlowRate"].Data.AsFloat,
                tank_level = sprayer.Outputs["TankLevel"].Data.AsFloat
            };

            simOutputLines.Add(outputLine);
        }

        public static string Simulate(string route, string zones, int timestep, double targetVelocity)
        {
            Sim sim = new Sim()
            {
                VehicleRouteJson = route,
                ZonesJson = zones,
                TimestepSec = timestep,
                TargetVelocity = targetVelocity
            };

            // run the simulation
            return sim.Run();
        }

        public static string Simulate(string json)
        {
            JObject jo = JObject.Parse(json);

            string routeJson = jo.SelectToken("route").ToString();
            string zonesJson = jo.SelectToken("zones").ToString();
            int timestep = Convert.ToInt32(jo.SelectToken("TimeStep").ToString());
            double targetVelocity = Convert.ToDouble(jo.SelectToken("TargetVelocity").ToString());

            return Sim.Simulate(routeJson, zonesJson, timestep, targetVelocity);
        }

        public static SimulationResult SimulateNew(string route, string zones, int timestep, double targetVelocity)
        {
            var sim = new Sim()
            {
                VehicleRouteJson = route,
                ZonesJson = zones,
                TimestepSec = timestep,
                TargetVelocity = targetVelocity
            };

            // run the simulation
            return sim.RunNew();
        }

    }
}
