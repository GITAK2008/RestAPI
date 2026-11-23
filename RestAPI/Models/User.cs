using System;
using System.Collections.Generic;

namespace RestAPI.Models;

public partial class User
{
    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;
}
