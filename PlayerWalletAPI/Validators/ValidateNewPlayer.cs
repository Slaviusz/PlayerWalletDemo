using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerWalletAPI.Models.Request;
using PlayerWalletAPI.Models.Response;
using PlayerWalletContext.Entities;

namespace PlayerWalletAPI.Validators
{
    /// <summary>
    /// Validator for PlayerController.Add()
    /// </summary>
    public class ValidateNewPlayer : ActionFilterAttribute
    {
        /// Validates if:
        /// * transaction is retried (player already exists)
        /// * new Player name conflicts with existing name in the DB
        ///
        /// <returns>Existing object if transaction is retried</returns>
        /// <returns>ConflictObject if Player Name already exists</returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<PlayerWalletContext.PlayerWalletContext>();
            var mapper = context.HttpContext.RequestServices.GetRequiredService<IMapper>();

            var model = context.ActionArguments["model"] as PlayerAddRequest;
            var transactionId = model?.TransactionId;
            var playerName = model?.PlayerName;

            var existingPlayer = await db.Players
                .AsNoTracking()
                .Where(p =>
                    p.Id == transactionId ||
                    p.PlayerName == playerName
                ) // Combine the condition to avoid double database access
                .FirstOrDefaultAsync();

            // this transaction is repeated
            if (existingPlayer?.Id == transactionId)
            {
                var repeatedResult = mapper.Map<Player, PlayerModelResponse>(existingPlayer);
                repeatedResult.Repeated = true;
                context.Result = new OkObjectResult(repeatedResult);
                return;
            }

            // conflicting PlayerName
            if (existingPlayer?.PlayerName == playerName)
            {
                context.Result = new ConflictObjectResult(new
                {
                    Code = 3,
                    Message = $@"Conflicting PlayerName value of ""{playerName}""."
                });
                return;
            }

            await next();

            // post execution actions
        }
    }

}
