using AutoMapper;
using PlayerWalletAPI.Models.Response;
using PlayerWalletContext.Entities;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace PlayerWalletAPI.AutoMap
{
    public class AutoMapperConfig : Profile
    {
        public void AutoMapping()
        {
            // Define AutoMapper mapping configurations here
            CreateMap<Player, PlayerModelResponse>();
        }
    }
}
