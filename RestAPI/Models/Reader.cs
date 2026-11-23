using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RestAPI.Models;

public partial class Reader
{
    public int ReaderId { get; set; }

    public string? ReaderFullName { get; set; }

    public string? ReaderAddress { get; set; }

    public string? ReaderPhone { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
