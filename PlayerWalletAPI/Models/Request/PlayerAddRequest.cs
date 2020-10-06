using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace PlayerWalletAPI.Models.Request
{
    /// <summary>
    /// Request model for PlayerController.Add()
    /// </summary>
    public sealed class PlayerAddRequest : IValidatableObject
    {
        /// <summary>
        /// Transaction Id that becomes user's Id if successfull
        /// </summary>
        [Required]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Player name
        /// </summary>
        /// <example>LittlePanda88</example>
        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Player name must be between {2} - {1} characters long!")]
        public string PlayerName { get; set; }

        /// <summary>
        /// Player birth date
        /// </summary>
        /// <example>19.12.1991</example>
        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; } // Let's ignore timezone differences for now

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BirthDate.AddYears(18) > DateTime.Now)
            {
                yield return new ValidationResult(
                    "Player must be at least 18 years old!",
                    new[] { nameof(BirthDate) });
            }
        }
    }
}
