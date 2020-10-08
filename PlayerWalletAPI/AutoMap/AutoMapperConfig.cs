using AutoMapper;
using PlayerWalletAPI.Models.Request;
using PlayerWalletAPI.Models.Response;
using PlayerWalletContext.Entities;
#pragma warning disable 1591

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace PlayerWalletAPI.AutoMap
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            #region Player
            // Player Entity to ResponseModel DTO
            CreateMap<Player, PlayerModelResponse>();

            // PlayerAddRequest from PlayerController.Add() to Player DTO
            var map1 = CreateMap<PlayerAddRequest, Player>();
            map1.ForMember(player => player.Id, opt => opt.MapFrom(add => add.TransactionId));
            map1.ForMember(player => player.PlayerName, opt => opt.MapFrom(add => add.PlayerName));
            #endregion

            #region Wallet
            // Wallet Entity to WalletModel DTO
            CreateMap<Wallet, WalletModelResponse>();
            #endregion
        }
    }
}
