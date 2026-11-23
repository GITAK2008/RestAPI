using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RestAPI.Models;

public partial class Loan
{
    public int ReaderId { get; set; }

    public string BookCipher { get; set; } = null!;

    public DateOnly DateOfIssue { get; set; }

    public string Signature { get; set; } = null!;

    public int LoanId { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual Book BookCipherNavigation { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual Reader Reader { get; set; } = null!;
}
