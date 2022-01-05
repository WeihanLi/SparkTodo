// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace SparkTodo.Models.AccountViewModels;

/// <summary>
/// LoginViewModel
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Email
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
