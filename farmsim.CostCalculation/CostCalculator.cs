namespace farmsim.CostCalculation;

public class CostCalculator
{

    private static readonly int SecondsInHour = 3600;

    private static double CalculateFieldOperationLabourCost(SimulationResult simulationResult, PerformanceParameters parameters)
    {
        var fieldTime = simulationResult.TimeWorking + simulationResult.TimeNonWorking;
        return parameters.LaborCost * fieldTime * parameters.HumanInterventionDuringOperationPercent / SecondsInHour;
    }

    private static double CalculateTransportLabourCost(SimulationResult simulationResult, PerformanceParameters parameters)
    {
        return parameters.HumanTransportInvolvementPercent * simulationResult.TimeTransport * parameters.LaborCost / SecondsInHour;
    }

    private static double CalculateFuelCost(SimulationResult simulationResult, PerformanceParameters parameters)
    {
        return simulationResult.FuelUsed * parameters.FuelPrice;
    }

    private static double CalculateMachineryCost(SimulationResult simulationResult, PerformanceParameters parameters)
    {
        var totalTime = simulationResult.TimeWorking + simulationResult.TimeNonWorking + simulationResult.TimeTransport;
        return totalTime * parameters.MachineryRental / SecondsInHour;
    }

    public static OperationalCosts CalculateCost(SimulationResult simulationResult, PerformanceParameters parameters)
    {
        return new OperationalCosts
        {
            LabourCostFieldOperation = CalculateFieldOperationLabourCost(simulationResult, parameters),
            LabourCostTransport = CalculateTransportLabourCost(simulationResult, parameters),
            FuelCostTotal = CalculateFuelCost(simulationResult, parameters),
            MachineryCost = CalculateMachineryCost(simulationResult, parameters)
        };

    }
}
