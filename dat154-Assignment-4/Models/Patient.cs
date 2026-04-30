using System;
using System.Collections.Generic;

namespace Assignment_4.Models;

public partial class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Sex { get; set; } = null!;
    public string Weight { get; set; } = null!;
    public string Age { get; set; } = null!;
    public double? SystolicBP { get; set; }
    public double? DiastolicBP { get; set; }
    public double? HeartRate { get; set; }
    public double? RespiratoryRate { get; set; }
    public double? OxygenSaturation { get; set; }
    public double? Temperature { get; set; }
    public bool IsActive { get; set; }
    public int? StudentId { get; set; }

    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public virtual ICollection<Allergies> Allergies { get; set; } = new List<Allergies>();
}