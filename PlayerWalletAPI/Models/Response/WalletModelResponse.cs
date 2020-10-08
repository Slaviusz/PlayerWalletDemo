namespace PlayerWalletAPI.Models.Response {
	/// <summary>
	/// Response model from WalletController.GetById()
	/// </summary>
	public class WalletModelResponse {
		/// <summary>
		/// Current Wallet balance in €
		/// </summary>
		/// <example>1510.50</example>
		public decimal Balance { get; set; }
	}
}
