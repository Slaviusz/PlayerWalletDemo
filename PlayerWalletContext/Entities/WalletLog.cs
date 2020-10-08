using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#nullable disable

namespace PlayerWalletContext.Entities
{
    public class WalletLog
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        public Guid WalletId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DefaultValue("now()")]
        public DateTime CreatedAt { get; set; }

        [Required]
        public ResultType ResultType { get; set; }

        [Required]
        public string Memento { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResultType
    {
        Created = 200,
        UnprocessableEntity = 422,
        BadRequest = 400
    }

}
