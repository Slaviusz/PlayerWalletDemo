using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace PlayerWalletContext.Entities
{
    public sealed class Player
    {
        [Required, Key]
        public Guid Id { get; set; }
        [Required, DefaultValue("true")]
        public bool Active { get; set; }
        [Required, MinLength(4), MaxLength(32)]
        public string PlayerName { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
