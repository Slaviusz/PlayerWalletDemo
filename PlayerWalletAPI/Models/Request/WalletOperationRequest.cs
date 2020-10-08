using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlayerWalletAPI.Models.Request
{
    /// <summary>
    /// Request model for WalletController.AttemptTransaction()
    /// </summary>
    public sealed class WalletOperationRequest //: IValidatableObject
    {
        /// <summary>
        /// Transaction and Correlation Id
        /// </summary>
        [Required]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Type of transaction
        /// </summary>
        [Required]
        public WalletTransactionType TransactionType { get; set; }

        /// <summary>
        /// Amount attributed to the transaction
        /// Limited to 99 999,99€ due to legal reasons
        /// </summary>
        [Required]
        [Range(minimum: 0.01, maximum: 99_999.99, ErrorMessage = "Transaction Amount must fit between 0,01 and 99.999,99!")]
        public decimal Amount { get; set; }

        /// <summary>Determines whether the specified object is valid.</summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
         public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
         {
             if (decimal.Subtract(Amount, decimal.Floor(Amount)) < 0.01m)
             {
                 yield return new ValidationResult(
                     "The lowest possible fraction of any transaction Amount is 1/100!",
                     new[] { nameof(Amount) });
             }
         }
    }

    /// <summary>
    /// Enum describing various possible transaction
    /// types on Player's account Wallet
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
#pragma warning disable 1591
    public enum WalletTransactionType
    {
        Deposit,
        Withdrawal,
        Win,
        Loss,
        Penalty,
        Confiscation
    }
#pragma warning restore 1591
}
