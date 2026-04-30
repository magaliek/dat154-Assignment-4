using System;
using System.Collections.Generic;

namespace Assignment_4.Models;

public partial class Goal
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string Goal1 { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
