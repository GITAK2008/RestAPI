using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RestAPI.Models;

public partial class Book
{
    public string BookCipher { get; set; } = null!;

    public string? FirstAuthor { get; set; }

    public string BookName { get; set; } = null!;

    public int? YearOfPublication { get; set; }

    public int BookPrice { get; set; }

    public int NumberOfCopies { get; set; }

    public int PublisherId { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    [JsonIgnore]
    [ValidateNever]
    public virtual Publisher Publisher { get; set; } = null!;
}
