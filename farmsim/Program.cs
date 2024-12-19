using libFarmSim;
using libFarmWork;
using libFarmSimTSP;
using CommandLine;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace FarmSim
{
    internal class Program
    {

        // use commandline parameters: -r vehicleroute1_cartesian.geojson  -z zones_cartesian.geojson  -t 2  -v 2 -j simple_input.json
        // use working directory: ../data/simple
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Console.WriteLine("Hello from farmsim!");

            // parse command-line parameters
            ParserResult<Options> pr = Parser.Default.ParseArguments<Options>(args);
            Options o = pr.Value;

            // if errors, terminate the program
            if (o == null)
                return;


            OptimizeByTrialAndError(o);

            OptimizeWithEvolAlg(o);
        }

        static void OptimizeByTrialAndError(Options o)
        {
            Console.WriteLine("Optimizing by trial-and-error");

            string workdir = o.WorkDir;
            string fWorkingLanes = Path.Combine(workdir, "workinglanes.geojson");
            string fTransportLanes = Path.Combine(workdir, "transportlanes.geojson");

            string zones = File.ReadAllText(Path.Combine(workdir, "zones_cartesian.geojson"));

            FarmField farmField = new FarmField();
            farmField.ReadFiles(fWorkingLanes, fTransportLanes);

            // random starting path
            farmField.Shuffle();

            // simple trial-and-error to find a reasonable path
            int count = 0;
            double time = Double.MaxValue;
            PathConfig config;
            string bestPath = farmField.WritePathText();

            List<string> mypaths = new List<string>();
            

            if (!Directory.Exists(o.OutputDir))
            {
                Directory.CreateDirectory(o.OutputDir);
            }

            StreamWriter sw = new(Path.Combine(o.OutputDir, "farmsim.out.csv"));
            sw.WriteLine("count,timeTotal,timeTransport,timeWorking,distanceTransport,distanceWorking,fuelUsed");

            bool iterate;
            do
            {
                config = farmField.GetPathConfig();
                string key = config.ToString();

                if (mypaths.Contains(key))
                {
                    //Console.WriteLine($"skipping {key}");
                }
                else
                {
                    //Console.WriteLine($"new {key}");
                    // store
                    mypaths.Add(key);

                    // calculate
                    string geojson = farmField.WritePath();
                    string filename = Path.Combine(o.OutputDir, "route." + count.ToString("00#") + ".geojson");
                    File.WriteAllText(filename, geojson);

                    string result = Sim.Simulate(geojson, zones, 2, 1.5);

                    JObject jo = JObject.Parse(result);
                    double time2 = Convert.ToDouble(jo.SelectToken("TotalTime").ToString());
                    double timeTransport = Convert.ToDouble(jo.SelectToken("TimeTransport").ToString());
                    double timeWorking = Convert.ToDouble(jo.SelectToken("TimeWorking").ToString());
                    double distanceTransport = Convert.ToDouble(jo.SelectToken("DistanceTransport").ToString());
                    double distanceWorking = Convert.ToDouble(jo.SelectToken("DistanceWorking").ToString());
                    double fuelUsed = Convert.ToDouble(jo.SelectToken("FuelUsed").ToString());

                    sw.WriteLine($"{count},{time2},{timeTransport},{timeWorking},{distanceTransport},{distanceWorking}{fuelUsed}");

                    // evaluate
                    if (time2 < time)
                    {
                        time = time2;
                        bestPath = farmField.WritePathText();
                        Console.WriteLine($"it={count},{bestPath},time = {time}");
                    }
                    else
                        farmField.SetPathConfig(config);
                }

                iterate = ++count < 200;

                if (iterate)
                    farmField.Shuffle();

            } while (iterate);

            sw.Close();

            Console.WriteLine($"best path after {count} tries: {bestPath}");
        }


        static void OptimizeWithEvolAlg(Options o)
        {
            Console.WriteLine("Optimizing with evolutionary algorithm");

            // evaluate vehicle route 
            EvaluateVehicleRoute(o);

            // create a simple field with 9 lanes
            string workdir = o.WorkDir;
            string fWorkingLanes = Path.Combine(workdir, "workinglanes9.geojson");
            string fTransportLanes = Path.Combine(workdir, "transportlanes9.geojson");
            int lane_n = 9;
            CreateField(fWorkingLanes, fTransportLanes, lane_n);

            // find optimal route
            string zones = File.ReadAllText(Path.Combine(workdir, "zones_cartesian.geojson"));
            int timestep = 2; // s
            double speed = 1.5; // m/s
            FindBestRoute(fWorkingLanes, fTransportLanes, zones, timestep, speed);
        }

        static void EvaluateVehicleRoute(Options o)
        {
            // variable to store simulation output
            string jsonoutput;

            // read input file(s) and run the simulation
            if (o.JsonInputFileName == null)
            {

                string routeJson = File.ReadAllText(o.VehicleRouteFileName);
                string zonesJson = File.ReadAllText(o.ZonesFileName);
                jsonoutput = Sim.Simulate(routeJson, zonesJson, o.TimeStep, o.TargetVelocity);
            }
            else
            {
                string jsoninput = File.ReadAllText(Path.Combine(o.WorkDir, o.JsonInputFileName));
                jsonoutput = Sim.Simulate(jsoninput);
            }

            Console.WriteLine(jsonoutput);
        }

        static void CreateField(string fWorkingLanes, string fTransportLanes, int lane_n)
        {
            FieldMaker fieldMaker = new()
            {
                fnWorkingLanes = fWorkingLanes,
                fnTransportLanes = fTransportLanes,
                lane_len = 100,// length of lanes, m
                lane_width = 3, // distance between lanes, m
                headland_width = 6 // width of headland, m
            };

            fieldMaker.Make();
        }


        static void FindBestRoute(string fWorkingLanes, string fTransportLanes, string zones, int timestep, double speed)
        {
            // load data 
            FarmField farmField = new FarmField();
            farmField.ReadFiles(fWorkingLanes, fTransportLanes);

            RouteEvaluator routeEvaluator = new RouteEvaluator(farmField, zones, timestep, speed);

            FarmSimTSP.routeEvaluator = routeEvaluator;
            FarmSimTSP tsp = new FarmSimTSP();

            tsp.Solve();

        }

      
    }

}



