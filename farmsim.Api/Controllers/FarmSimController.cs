using libFarmSim;
using farmsim.CostCalculation;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;

namespace farmsim.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FarmSimController : ControllerBase
{
    private readonly ILogger<FarmSimController> _logger;

    public FarmSimController(ILogger<FarmSimController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "Simulate")]
    public SimulationOutputDto Simulate(SimulationInputDto simulationData)
    {
        var writer = new GeoJsonWriter();
        var result = Sim.SimulateNew(writer.Write(simulationData.Route), writer.Write(simulationData.Zones), simulationData.TimeStep, simulationData.TargetVelocity);

        return new SimulationOutputDto
        {
            TimeLapsedSecs = result.TimeLapsedSecs,
            TimeWorking = result.TimeWorking,
            TimeNonWorking = result.TimeNonWorking,
            TimeTransport = result.TimeTransport,
            DistanceWorking = result.DistanceWorking,
            DistanceNonWorking = result.DistanceNonWorking,
            DistanceTransport = result.DistanceTransport,
            FuelUsed = result.FuelUsed,
            OperationalCosts = simulationData.PerformanceParameters != null ? GetOperationalCosts(simulationData, result) : null
        };

    }

    private OperationalCostsDto GetOperationalCosts(SimulationInputDto simulationData, SimulationResult result)
    {
        var costs = CostCalculator.CalculateCost(result, MapPerformanceParameters(simulationData.PerformanceParameters));
        return MapOperationalCosts(costs);
    }

    private PerformanceParameters MapPerformanceParameters(PerformanceParametersInputDto performanceParameters)
    {
        return new PerformanceParameters
        {
            FuelPrice = performanceParameters.FuelPrice,
            LaborCost = performanceParameters.LaborCost,
            MachineryRental = performanceParameters.MachineryRental,
            HumanInterventionDuringOperationPercent = performanceParameters.HumanInterventionDuringOperationPercent,
            HumanTransportInvolvementPercent = performanceParameters.HumanTransportInvolvementPercent
        };
    }

    private OperationalCostsDto MapOperationalCosts(OperationalCosts operationalCosts)
    {
        return new OperationalCostsDto
        {
            LabourCostFieldOperation = operationalCosts.LabourCostFieldOperation,
            LabourCostTransport = operationalCosts.LabourCostTransport,
            FuelCostTotal = operationalCosts.FuelCostTotal,
            MachineryCost = operationalCosts.MachineryCost
        };
    }
}

