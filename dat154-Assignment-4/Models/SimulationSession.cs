namespace Assignment_4.Models;

public class SimulationSession
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int StudentCustomUserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<SimulationAction> Actions { get; set; } = new List<SimulationAction>();
    public ICollection<TeacherObservation> Observations { get; set; } = new List<TeacherObservation>();
}
