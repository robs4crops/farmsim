using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Features;

namespace farmsim.Api
{
    public struct SimulationInputDto
    {
        [Required]
        public FeatureCollection Route { get; set; }

        [Required]
        public FeatureCollection Zones { get; set; }

        [Required]
        public int TimeStep { get; set; }

        [Required]
        public double TargetVelocity { get; set; }

        public PerformanceParametersInputDto PerformanceParameters { get; set; }
    }
}
