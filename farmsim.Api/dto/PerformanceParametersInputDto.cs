using System.ComponentModel.DataAnnotations;

namespace farmsim.Api
{
    public class PerformanceParametersInputDto
    {
        [Required]
        public double FuelPrice { get; set; }

        [Required]
        public double LaborCost { get; set; }

        [Required]
        public double MachineryRental { get; set; }

        [Required]
        public double HumanTransportInvolvementPercent { get; set; }

        [Required]
        public double HumanInterventionDuringOperationPercent { get; set; }
    }
}
