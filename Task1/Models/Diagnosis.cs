using System;
using System.Collections.Generic;

namespace Assignment_4.Models;

public partial class Diagnosis
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string Diagnosis1 { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
