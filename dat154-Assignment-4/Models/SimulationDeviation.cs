namespace Assignment_4.Models;

public class SimulationDeviation
{
    public int Id { get; set; }
    public int ActionId { get; set; }
    public string RuleName { get; set; } = null!;
    public string Message { get; set; } = null!;

    public SimulationAction Action { get; set; } = null!;
}
