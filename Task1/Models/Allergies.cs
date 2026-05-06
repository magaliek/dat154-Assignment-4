using System;
using System.Collections.Generic;

namespace Assignment_4.Models;

public partial class Allergies
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string Allergy { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
