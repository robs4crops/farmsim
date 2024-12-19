using CommandLine;

namespace FarmSim
{
    public class Options
    {
        [Option('i', "inputdir", Required = false, Default = ".", HelpText = "Directory where input files are located.")]
        public string WorkDir { get; set; }

        [Option('j', "jsoninputfile", Required = false, HelpText = "name of the file which contains json input that defines the simulation")]
        public string? JsonInputFileName { get; set; }

        [Option('r', "vehicleroute", Required = false, Default = "vehicleroute_cartesian.geojson", HelpText = "name of the file which contains the route that the vehicle will follo")]
        public string VehicleRouteFileName { get; set; }

        [Option('z', "zones", Required = false, Default = "zones_cartesian.geojson", HelpText = "name of the file which contains zones with restrictions on the vehicle")]
        public string ZonesFileName { get; set; }

        [Option('t', "timestep", Required = false, Default = 2, HelpText = "time step (seconds) for dynamic simulation")]
        public int TimeStep { get; set; }

        [Option('v', "velocity", Required = false, Default = 1.0, HelpText = "desired velocity (m/s) of the vehicle")]
        public double TargetVelocity { get; set; }

        [Option('o', "outdir", Required = false, Default = ".", HelpText = "Directory where output files will be saved.")]
        public string OutputDir { get; set; }
    }
}
