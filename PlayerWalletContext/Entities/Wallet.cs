using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlayerWalletContext.Entities {
	public class Wallet {
		[Required, Key]
		public Guid Id { get; set; }

		[Required]
		[DefaultValue(0)]
		// Use decimal for currency as it has less
		// problems due to rounding errors than float/double
		public decimal Balance { get; set; }

		[Timestamp]
		// Use RowVersion for optimistic concurrency in
		// multi-node horizontally scaled environments
		public byte[] RowVersion { get; set; }
	}
}