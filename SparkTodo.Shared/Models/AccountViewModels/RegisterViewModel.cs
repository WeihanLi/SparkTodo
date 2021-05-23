using System.ComponentModel.DataAnnotations;

namespace SparkTodo.API.Models.AccountViewModels
{
    /// <summary>
    /// RegisterViewModel
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
