using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _logger;
        private readonly PlayerWalletContext.PlayerWalletContext _db;
        private readonly IMapper _mapper;

        // more compact Json serializer settings for database storage
        private readonly JsonSerializerOptions _databaseSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            AllowTrailingCommas = false,
            IgnoreNullValues = false
        };

        /// <summary>
        /// ResponseObject for WalletLog Unprocessable Transactions
        /// </summary>
        public static readonly object UnprocessableWalletLogResponseObject = new
        {
            Code = 4,
            Message = "Transaction would result in Wallet negative balance!"
        };

        // All Transaction types that affect Balance negatively
        private static readonly WalletTransactionType[] NegativeTransactionTypes = new[]
        {
            WalletTransactionType.Confiscation,
            WalletTransactionType.Loss,
            WalletTransactionType.Penalty,
            WalletTransactionType.Withdrawal
        };

        private static readonly WalletTransactionType[] PositiveTranscationTypes = new[]
        {
            WalletTransactionType.Deposit,
            WalletTransactionType.Win
        };

        public WalletController(
            ILogger<WalletController> logger,
            PlayerWalletContext.PlayerWalletContext db,
            IMapper mapper
        )
        {
            _logger = logger;
            _db = db;
            _mapper = mapper;
        }

        /// <summary>
        /// Return Player's Wallet object based on its Guid
        /// </summary>
        /// <param name="walletGuid"></param>
        /// <returns>Wallet object</returns>
        [HttpGet("{walletGuid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WalletModelResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> GetById([FromRoute] Guid walletGuid)
        {
            var result = await _db.Wallet
                .AsNoTracking()
                .Where(wallet => wallet.Id == walletGuid)
                .ProjectTo<WalletModelResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return new NotFoundResult();
            }

            return Ok(result);
        }

        /// <summary>
        /// Attempts operation on Player's Wallet
        /// </summary>
        /// <remarks>
        /// Validations:
        /// * Operation does not produce negative balance
        /// </remarks>
        /// <param name="walletGuid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{walletGuid}")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WalletOperationResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
        // Transaction woould result in integrity corruption
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(UnprocessableEntityObjectResult))]
        // Optimimstic concurrency failed
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ValidateExistingTransaction]
        public async Task<IActionResult> AttemptTransaction([FromRoute] Guid walletGuid, [FromBody] WalletOperationRequest model)
        {
            // At this point Model is validated
            // Get player's Wallet tracked Entity
            var playersWallet = await _db.Wallet
                .Where(wallet => wallet.Id == walletGuid)
                .FirstOrDefaultAsync();

            // If we're dealing with negative transaction type
            if (NegativeTransactionTypes.Contains(model.TransactionType))
            {
                // check if it results in positive Balance in the end
                if ((playersWallet.Balance - model.Amount) < decimal.Zero)
                {
                    return new UnprocessableEntityObjectResult(UnprocessableWalletLogResponseObject);
                }

                // Update Player's Wallet Balance
                playersWallet.Balance -= model.Amount;
            }

            if (PositiveTranscationTypes.Contains(model.TransactionType))
            {
                // Update Player's Wallet Balance
                playersWallet.Balance += model.Amount;
            }

            // perform the operation and log to operations log
            // runs in an implicit transaction

            // prepare OkResult
            var result = new WalletOperationResult
            {
                Id = model.TransactionId,
                Repeated = false,
                Success = true,
                WalletState = new WalletModelResponse
                {
                    Balance = playersWallet.Balance
                }
            };

            // ReSharper disable once MethodHasAsyncOverload
            // Async overload serves only purpose for special HiLo generated PKs
            _db.WalletLogs.Add(
                new WalletLog
                {
                    CreatedAt = DateTime.Now,
                    ResultType = ResultType.Created,
                    WalletId = walletGuid,
                    TransactionId = model.TransactionId,
                    Memento = JsonSerializer.Serialize(result, _databaseSerializerOptions)
                }
            );

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DBConcurrencyException)
            {
                // Optimistic concurrency failed as the object
                // has been changed during this transaction
                // signal this bac to the client which is
                // responsible for retry!
                return new CustomPreconditionFailedResult();
            }

            // TODO: Fill in uri to Get method where the transaction can be found
            return Created("", result);
        }
    }
    
    /// <summary>
    /// Custom implementation of missing PreconditionFailedResult
    /// </summary>
    public sealed class CustomPreconditionFailedResult : IActionResult
    {
        /// <summary>
        /// Custom IActionResult for missing PreconditionFailed HTTP 412 status
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(null)
            {
                StatusCode = StatusCodes.Status412PreconditionFailed
            };

            return objectResult.ExecuteResultAsync(context);
        }
    }
}
