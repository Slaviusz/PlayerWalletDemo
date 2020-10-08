using System;
using System.ComponentModel;
#nullable disable

namespace PlayerWalletAPI.Models.Response
{
    /// <summary>
    /// Response from WalletController.AttemptTransaction()
    /// </summary>
    public class WalletOperationResult
    {
        /// <summary>
        /// Unique transaction Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Whether the result of the operation was success or not
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Whether this operation was actually
        /// performed or the result was retrieved
        /// from the log due to repeted attempt
        /// </summary>
        /// <example>false</example>
        [DefaultValue(false)]
        public bool Repeated { get; set; }

        /// <summary>
        /// State of the Wallet at the end of the Operation
        /// </summary>
        /// <example>{ Balance = 327,50 }</example>
        public WalletModelResponse WalletState { get; set; }
    }
}
