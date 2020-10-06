using System;
using System.ComponentModel;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using PlayerWalletContext.Entities;
#nullable disable

namespace PlayerWalletAPI.Models.Response
{
    /// <summary>
    /// Response model from PlayerController Get* methods
    /// </summary>
    [AutoMap(typeof(Player))]
    public class PlayerModelResponse
    {
        public Guid Id { get; set; }
        public bool Active { get; set; }

        /// <summary>
        /// Player name
        /// </summary>
        /// <example>ExamplePlayerName</example>
        [SourceMember("PlayerName")]
        public string Name { get; set; }

        [DefaultValue(false)]
        public bool Repeated { get; set; }
    }
}
