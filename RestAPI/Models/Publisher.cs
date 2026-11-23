using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RestAPI.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string? PublisherName { get; set; }

    public string? PublisherCity { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
