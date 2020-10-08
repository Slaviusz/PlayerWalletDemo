using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerWalletAPI.Controllers.v1;
using PlayerWalletAPI.Models.Request;
using PlayerWalletAPI.Models.Response;
using PlayerWalletContext.Entities;

namespace PlayerWalletAPI.Validators
{
    /// <summary>
    /// Validator for PlayerController.Add()
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class ValidateExistingTransaction : ActionFilterAttribute
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

            var model = context.ActionArguments["model"] as WalletOperationRequest;

            var transactionId = model?.TransactionId;

            var existingTransaction = await db.WalletLogs
                .AsNoTracking()
                .Where(p =>
                    p.TransactionId == transactionId
                ) // Combine the condition to avoid double database access
                .FirstOrDefaultAsync();

            if (existingTransaction == null)
            {
                // continue normal execution
                await next();
                return;
            }

            // this transaction is repeated
            if (existingTransaction.TransactionId == transactionId)
            {
                // Get previous return object stored in WalletLog Memento
                var repeatedResultFromMemento = RepeatedWalletTransactionFactory.GetRepeatedResult(existingTransaction);
                context.Result = repeatedResultFromMemento;
            }
        }
    }

    /// <summary>
    /// Factory for building repeated WalletLog result
    /// </summary>
    public static class RepeatedWalletTransactionFactory
    {
        /// <summary>
        /// Method for building ObjectResult from WalletLog's Memento
        /// </summary>
        /// <param name="logItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IActionResult GetRepeatedResult(WalletLog logItem)
        {
            switch (logItem.ResultType)
            {
                case ResultType.Created:
                    var createdResult = JsonSerializer.Deserialize<WalletOperationResult>(logItem.Memento);
                    createdResult.Repeated = true;
                    // TODO: Fill in uri to Get method where the transaction can be found
                    return new CreatedResult("", createdResult);
                case ResultType.UnprocessableEntity:
                    return new UnprocessableEntityObjectResult(WalletController.UnprocessableWalletLogResponseObject);
                case ResultType.BadRequest:
                    var badRequestResult = JsonSerializer.Deserialize<ValidationProblemDetails>(logItem.Memento);
                    return new BadRequestObjectResult(badRequestResult);
                default:
                    // throws Exception which returns 500 Internal Server Error
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
