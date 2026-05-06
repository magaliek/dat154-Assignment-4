namespace Assignment_4.Models;

public class TeacherObservation
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int? RelatedActionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Text { get; set; } = null!;

    public SimulationSession Session { get; set; } = null!;
}
