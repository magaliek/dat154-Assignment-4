using System;
using System.Collections.Generic;

namespace Assignment_4.Models;

public partial class CustomUser
{
    public int Id { get; set; }

    public string Role { get; set; } = null!;

    public string Email { get; set; } = null!;
}
