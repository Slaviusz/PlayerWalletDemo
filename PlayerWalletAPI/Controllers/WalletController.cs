using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlayerWalletAPI.Models.Response;

namespace PlayerWalletAPI.Controllers {
	[ApiController]
	[Route("v1/[controller]")]
	[AllowAnonymous]
	public class WalletController : ControllerBase
	{
		private readonly ILogger<WalletController> _logger;
		private readonly PlayerWalletContext.PlayerWalletContext _db;
		private readonly IMapper _mapper;

		public WalletController(
			ILogger<WalletController> logger,
			PlayerWalletContext.PlayerWalletContext db,
			IMapper mapper
			) {
			_logger = logger;
			_db = db;
			_mapper = mapper;
		}

		/// <summary>
		/// Return Player's Wallet object based on its Guid
		/// </summary>
		/// <param name="walletGuid"></param>
		/// <returns>Wallet object</returns>
		[HttpGet("{walletGuid")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WalletModelResponse))]
		[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResult))]
		public async Task<IActionResult> GetById([FromRoute] Guid walletGuid) {
			var result = await _db.Wallet
				.Where(wallet => wallet.Id == walletGuid)
				.ProjectTo<WalletModelResponse>(_mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();

			if (result == null) {
				return new NotFoundResult();
			}

			return Ok(result);		}
	}
}