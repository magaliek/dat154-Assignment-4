using Assignment_4.Models;
using SharedLogic;

namespace Assignment_4.Mapping;

public static class CaseMapper
{
    public static CaseDto ToDto(Patient p)
    {
        return new CaseDto
        {
            Id = p.Id,
            Name = p.Name,
            Sex = p.Sex,
            Weight = p.Weight,
            Age = p.Age,
            SystolicBP = p.SystolicBP,
            DiastolicBP = p.DiastolicBP,
            HeartRate = p.HeartRate,
            RespiratoryRate = p.RespiratoryRate,
            OxygenSaturation = p.OxygenSaturation,
            Temperature = p.Temperature,
            IsActive = p.IsActive,
            StudentId = p.StudentId,
            Medications = p.Medications.Select(m => new MedicationDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                Medication1 = m.Medication1,
                Dosage = m.Dosage,
                Administration = m.Administration
            }).ToList(),
            Diagnoses = p.Diagnoses.Select(d => new DiagnosisDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                Diagnosis1 = d.Diagnosis1
            }).ToList(),
            Allergies = p.Allergies.Select(a => new AllergyDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                Allergy = a.Allergy
            }).ToList(),
            Goals = p.Goals.Select(g => new GoalDto
            {
                Id = g.Id,
                PatientId = g.PatientId,
                Goal1 = g.Goal1
            }).ToList()
        };
    }
}
