namespace Assignment_4.Models;

public class SimulationAction
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Kind { get; set; } = null!;
    public string? Drug { get; set; }
    public double? DoseMg { get; set; }
    public string? Route { get; set; }
    public string? Notes { get; set; }

    public SimulationSession Session { get; set; } = null!;
    public ICollection<SimulationDeviation> Deviations { get; set; } = new List<SimulationDeviation>();
}
