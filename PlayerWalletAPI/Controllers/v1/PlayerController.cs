using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlayerWalletAPI.Models.Request;
using PlayerWalletAPI.Models.Response;
using PlayerWalletAPI.Validators;
using PlayerWalletContext.Entities;

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

        /// <summary>
        /// Returns Player object based on its Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Player object</returns>
        [HttpGet("{guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerModelResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
        public async Task<IActionResult> Get([FromRoute] Guid guid)
        {
            var result = await _db.Players
                .Where(p => p.Id == guid)
                .ProjectTo<PlayerModelResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return new NotFoundResult();
            }

            return Ok(result);
        }

        /// <summary>
        /// Adds new Player and his empty wallet.
        /// </summary>
        /// <remarks>
        /// Validations:
        /// * PlayerName length 8-20 characters
        /// * PlayerAge >= 18 y
        /// * PlayerName does not exist yet
        /// </remarks>
        /// <param name="model"></param>
        /// <returns><see cref="PlayerModelResponse"/> object if succesfull</returns>
        [HttpPost("[action]")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerModelResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ValidateNewPlayer] // Perform new Player validations (PlayerWalletAPI\Validators\ValidateNewPlayer.cs)
        public async Task<IActionResult> Add([FromBody] PlayerAddRequest model)
        {
            // at this point model has been validated
            var newPlayer = _mapper.Map<Player>(model);
            newPlayer.Active = true; // set initial status of player to Active

            // ReSharper disable once MethodHasAsyncOverload
            // AddAsync() overload has special use-case for client-side HiLo generated PKs
            _db.Players.Add(newPlayer);

            // Save Unit-of-Work
            // do not handle exceptions here to let other
            // services know we failed and they need to retry
            await _db.SaveChangesAsync();

            // TODO: More structured logging with NLog
            _logger.LogInformation($@">>>PlayerController.Add() success - Id ""{model.TransactionId}"", Name ""{model.PlayerName}"".");

            var result = _mapper.Map<Player, PlayerModelResponse>(newPlayer);

            return Ok(result);
        }
    }
}
