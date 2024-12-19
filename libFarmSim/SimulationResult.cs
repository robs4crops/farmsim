public struct SimulationResult
{
    public double TimeLapsedSecs { get; set; }
    public double TimeWorking { get; set; }
    public double TimeNonWorking { get; set; }
    public double TimeTransport { get; set; }
    public double DistanceWorking { get; set; }
    public double DistanceNonWorking { get; set; }
    public double DistanceTransport { get; set; }
    public double FuelUsed { get; set; }
}
