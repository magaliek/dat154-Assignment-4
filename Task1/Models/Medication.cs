using System;
using System.Collections.Generic;

namespace Assignment_4.Models;

public partial class Medication
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string Medication1 { get; set; } = null!;

    public double Dosage { get; set; }

    public string Administration { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
