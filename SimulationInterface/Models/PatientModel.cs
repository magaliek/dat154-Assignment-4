using System.Collections.Generic;

namespace SimulationInterface.Models
{
    public class PatientModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Sex { get; set; }
        public string Weight { get; set; }
        public double? SystolicBP { get; set; }
        public double? DiastolicBP { get; set; }
        public double? HeartRate { get; set; }
        public double? RespiratoryRate { get; set; }
        public double? OxygenSaturation { get; set; }
        public double? Temperature { get; set; }
        public bool IsActive { get; set; }
        public int? StudentId { get; set; }

        public List<Diagnosis> Diagnoses { get; set; } = new();
        public List<Medication> Medications { get; set; } = new();
        public List<Allergy> Allergies { get; set; } = new();
        public List<Goal> Goals { get; set; } = new();
    }

    public class Diagnosis
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Diagnosis1 { get; set; }
    }

    public class Medication
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Medication1 { get; set; }
        public double Dosage { get; set; }
        public string Administration { get; set; }
    }

    public class Allergy
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Allergy1 { get; set; }
    }

    public class Goal
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Goal1 { get; set; }
    }
}