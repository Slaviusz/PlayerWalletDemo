using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlayerWalletAPI.Models.Response;

namespace PlayerWalletAPI.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    [AllowAnonymous]
    public class PlayerController : ControllerBase
    {
        private readonly ILogger<PlayerController> _logger;
        private readonly PlayerWalletContext.PlayerWalletContext _db;
        private readonly IMapper _mapper;

        public PlayerController(
            ILogger<PlayerController> logger,
            PlayerWalletContext.PlayerWalletContext dbContext,
            IMapper mapper
            )
        {
            _logger = logger;
            _db = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of all Players
        /// </summary>
        /// <returns>list of all Players</returns>
        [HttpGet("GetAll")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlayerModelResponse>))]
        public async Task<IActionResult> GetAllPlayers()
        {
            var result = await _db.Players
                .ProjectTo<PlayerModelResponse>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return Ok(result);
        }
    }
}
