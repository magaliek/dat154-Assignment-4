namespace SharedLogic;

public class CaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sex { get; set; } = "";
    public string Weight { get; set; } = "";
    public string Age { get; set; } = "";

    public double? SystolicBP { get; set; }
    public double? DiastolicBP { get; set; }
    public double? HeartRate { get; set; }
    public double? RespiratoryRate { get; set; }
    public double? OxygenSaturation { get; set; }
    public double? Temperature { get; set; }

    public bool IsActive { get; set; }
    public int? StudentId { get; set; }

    public List<MedicationDto> Medications { get; set; } = new();
    public List<DiagnosisDto> Diagnoses { get; set; } = new();
    public List<AllergyDto> Allergies { get; set; } = new();
    public List<GoalDto> Goals { get; set; } = new();
}

public sealed class MedicationDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Medication1 { get; set; } = "";
    public double Dosage { get; set; }
    public string Administration { get; set; } = "";
}

public sealed class DiagnosisDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Diagnosis1 { get; set; } = "";
}

public sealed class AllergyDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Allergy { get; set; } = "";
}

public sealed class GoalDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Goal1 { get; set; } = "";
}

public class StartSessionDto
{
    public int PatientId { get; set; }
    public int StudentCustomUserId { get; set; }
}

public class RegisterActionDto
{
    public string Kind { get; set; } = "Medication";
    public string? Drug { get; set; }
    public double? DoseMg { get; set; }
    public string? Route { get; set; }
    public string? Notes { get; set; }
}

public class AddObservationDto
{
    public string Text { get; set; } = "";
    public int? RelatedActionId { get; set; }
}

public class SimulationSessionDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int StudentCustomUserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
}

public class SimulationActionDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Kind { get; set; } = "";
    public string? Drug { get; set; }
    public double? DoseMg { get; set; }
    public string? Route { get; set; }
    public string? Notes { get; set; }
    public List<SimulationDeviationDto> Deviations { get; set; } = new();
}

public class TeacherObservationDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int? RelatedActionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Text { get; set; } = "";
}

public class SimulationDeviationDto
{
    public int Id { get; set; }
    public int ActionId { get; set; }
    public string RuleName { get; set; } = "";
    public string Message { get; set; } = "";
}

public class DebriefDto
{
    public int SessionId { get; set; }
    public int PatientId { get; set; }
    public int StudentCustomUserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public List<SimulationActionDto> Actions { get; set; } = new();
    public List<TeacherObservationDto> Observations { get; set; } = new();
}
