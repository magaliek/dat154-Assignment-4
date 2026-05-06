using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Assignment_4.Models;

public class MedicationFormRow
{
    [MaxLength(100)]
    public string Name { get; set; } = "";

    public double Dosage { get; set; }

    [MaxLength(100)]
    public string Route { get; set; } = "";
}

public class CaseFormViewModel
{
    /// <summary>0 when creating a new case.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Sex is required")]
    [RegularExpression("^(Female|Male|Other)$", ErrorMessage = "Sex must be Female, Male, or Other.")]
    [MaxLength(100)]
    public string Sex { get; set; } = "";

    [Display(Name = "Age (years)")]
    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    public int AgeYears { get; set; }

    [Display(Name = "Weight (kg)")]
    [Range(0, 400, ErrorMessage = "Weight must be between 0 and 400 kg.")]
    public double WeightKg { get; set; }

    public double? SystolicBP { get; set; }
    public double? DiastolicBP { get; set; }
    public double? HeartRate { get; set; }
    public double? RespiratoryRate { get; set; }
    public double? OxygenSaturation { get; set; }
    public double? Temperature { get; set; }

    public bool IsActive { get; set; }

    public List<string> Diagnoses { get; set; } = new();
    public List<string> Goals { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public List<MedicationFormRow> Medications { get; set; } = new();

    public static CaseFormViewModel ForNewCase() => new()
    {
        Id = 0,
        IsActive = false,
        AgeYears = 0,
        WeightKg = 0,
        Diagnoses = new List<string> { "" },
        Goals = new List<string> { "" },
        Allergies = new List<string> { "" },
        Medications = new List<MedicationFormRow> { new() }
    };

    public static CaseFormViewModel FromPatient(Patient p)
    {
        var diagnoses = p.Diagnoses.Select(d => d.Diagnosis1).ToList();
        if (diagnoses.Count == 0)
            diagnoses.Add("");
        var goals = p.Goals.Select(g => g.Goal1).ToList();
        if (goals.Count == 0)
            goals.Add("");
        var allergies = p.Allergies.Select(a => a.Allergy).ToList();
        if (allergies.Count == 0)
            allergies.Add("");
        var meds = p.Medications
            .Select(m => new MedicationFormRow
            {
                Name = m.Medication1,
                Dosage = m.Dosage,
                Route = m.Administration
            })
            .ToList();
        if (meds.Count == 0)
            meds.Add(new MedicationFormRow());

        var ageYears = 0;
        if (!string.IsNullOrWhiteSpace(p.Age))
            int.TryParse(p.Age.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ageYears);

        var weightKg = 0d;
        if (!string.IsNullOrWhiteSpace(p.Weight))
        {
            var t = p.Weight.Trim();
            if (t.EndsWith("kg", StringComparison.OrdinalIgnoreCase))
                t = t[..^2].Trim();
            double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out weightKg);
        }

        var sex = NormalizeSexForForm(p.Sex);

        return new CaseFormViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Sex = sex,
            WeightKg = weightKg,
            AgeYears = ageYears,
            SystolicBP = p.SystolicBP,
            DiastolicBP = p.DiastolicBP,
            HeartRate = p.HeartRate,
            RespiratoryRate = p.RespiratoryRate,
            OxygenSaturation = p.OxygenSaturation,
            Temperature = p.Temperature,
            IsActive = p.IsActive,
            Diagnoses = diagnoses,
            Goals = goals,
            Allergies = allergies,
            Medications = meds
        };
    }

    /// <summary>DB stores age as a string (e.g. "58") and weight as "72 kg" — matches SeedData.</summary>
    public void ApplyToPatient(Patient p)
    {
        p.Name = Name;
        p.Sex = Sex;
        p.Age = AgeYears.ToString(CultureInfo.InvariantCulture);
        p.Weight = $"{WeightKg.ToString(CultureInfo.InvariantCulture)} kg";
        p.SystolicBP = SystolicBP;
        p.DiastolicBP = DiastolicBP;
        p.HeartRate = HeartRate;
        p.RespiratoryRate = RespiratoryRate;
        p.OxygenSaturation = OxygenSaturation;
        p.Temperature = Temperature;
        p.IsActive = IsActive;
    }

    private static string NormalizeSexForForm(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
            return "";
        return stored.Trim() switch
        {
            "Female" or "female" or "F" => "Female",
            "Male" or "male" or "M" => "Male",
            "Other" or "other" or "O" => "Other",
            _ => stored.Trim()
        };
    }
}
