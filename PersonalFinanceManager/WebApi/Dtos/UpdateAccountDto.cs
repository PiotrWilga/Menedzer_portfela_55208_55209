﻿using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Dtos;

public class UpdateAccountDto
{
    [MinLength(2, ErrorMessage = "Account name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Account name cannot contain only whitespace.")]
    public string? Name { get; set; }

    public AccountType? Type { get; set; }

    public string? CurrencyCode { get; set; }

    public decimal? Balance { get; set; }

    public bool? ShowInSummary { get; set; }
}