using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerWalletAPI.Models.Request;
using PlayerWalletAPI.Models.Response;
using Shouldly;
using Xunit;

namespace PlayerWalletTests
{
    public class WalletApiTests : IClassFixture<WebApplicationFactory<PlayerWalletAPI.Startup>>
    {
        private static readonly Guid SeedWalletId = Guid.ParseExact("11111111111111111111111111111111", "N");

        private readonly WebApplicationFactory<PlayerWalletAPI.Startup> _factory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly PlayerWalletContext.PlayerWalletContext _db;

        public WalletApiTests(WebApplicationFactory<PlayerWalletAPI.Startup> factory)
        {
            _factory = factory;
            var scope = factory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<PlayerWalletContext.PlayerWalletContext>();
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        private async Task TestWalletGetbyId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"v1/Wallet/{SeedWalletId}");

            Should.NotThrow(() => { response.EnsureSuccessStatusCode(); });

            WalletModelResponse wallet = null;

            Should.NotThrow(async () => { wallet = JsonSerializer.Deserialize<WalletModelResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            wallet.ShouldNotBeNull();
            
            // get correct balance from the database
            var correctBalance = _db.Wallet
                .First(w => w.Id == SeedWalletId)
                .Balance;
            
            wallet.Balance.ShouldBe(correctBalance);
        }

        [Fact]
        private async Task TestWalletGetbyIdNotFound()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"v1/Wallet/{Guid.NewGuid()}");

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Withdrawal, Loss, Penalty, Confiscation to negative Balance is not allowed
        /// </summary>
        /// <returns></returns>
        [Fact]
        private async Task TestWalletNegativeAmount()
        {
            var client = _factory.CreateClient();

            var currentBalance = (await _db.Wallet
                .FirstAsync(wallet => wallet.Id == SeedWalletId))
                .Balance;

            var json = JsonSerializer
                .Serialize(new WalletOperationRequest
                {
                    TransactionId = Guid.NewGuid(),
                    TransactionType = WalletTransactionType.Withdrawal,
                    Amount = currentBalance + 1m // try to withdraw more than current balance
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"v1/Wallet/{SeedWalletId}", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        }

        /// <summary>
        /// Performing an operation with Amounts smaller than 0.01€ (1 cent) is not allowed
        /// </summary>
        /// <returns></returns>
        [Fact]
        private async Task TestWalletTooSmallFraction()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new WalletOperationRequest
                {
                    TransactionId = Guid.NewGuid(),
                    TransactionType = WalletTransactionType.Withdrawal,
                    Amount = 0.001m
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"v1/Wallet/{SeedWalletId}", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        private async Task TestWalletDepositMoneyAndRepeat()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new WalletOperationRequest
                {
                    TransactionId = Guid.NewGuid(),
                    TransactionType = WalletTransactionType.Deposit,
                    Amount = 100m
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"v1/Wallet/{SeedWalletId}", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            WalletOperationResult walletOperationResult = null;

            Should.NotThrow(async () => { walletOperationResult = JsonSerializer.Deserialize<WalletOperationResult>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            walletOperationResult.Success.ShouldBeTrue();
            walletOperationResult.Repeated.ShouldBeFalse();

            // get correct balance from the database
            var correctBalance = (await _db.Wallet
                .FirstAsync(w => w.Id == SeedWalletId))
                .Balance;

            walletOperationResult.WalletState.Balance.ShouldBe(correctBalance);

            // repeat the same request

            var client2 = _factory.CreateClient();

            var response2 = await client2.PutAsync($"v1/Wallet/{SeedWalletId}", requestModel);

            response2.StatusCode.ShouldBe(HttpStatusCode.Created);

            WalletOperationResult walletOperationResult2 = null;
            Should.NotThrow(async () => { walletOperationResult2 = JsonSerializer.Deserialize<WalletOperationResult>(await response2.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            walletOperationResult2.Success.ShouldBeTrue();
            walletOperationResult2.Repeated.ShouldBeTrue();

            // balance should not be changed and should be the same as after the first transaction!
            walletOperationResult2.WalletState.Balance.ShouldBe(correctBalance);
        }

        [Fact]
        private async Task TestWalletWithdrawMoney()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new WalletOperationRequest
                {
                    TransactionId = Guid.NewGuid(),
                    TransactionType = WalletTransactionType.Withdrawal,
                    Amount = 1m
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"v1/Wallet/{SeedWalletId}", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            WalletOperationResult walletOperationResult = null;

            Should.NotThrow(async () => { walletOperationResult = JsonSerializer.Deserialize<WalletOperationResult>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            walletOperationResult.Success.ShouldBeTrue();
            walletOperationResult.Repeated.ShouldBeFalse();

            // get correct balance from the database
            var correctBalance = _db.Wallet
                .First(w => w.Id == SeedWalletId)
                .Balance;

            walletOperationResult.WalletState.Balance.ShouldBe(correctBalance);
        }

        //TODO: Testing Optimistic Locking/Concurrency using SQLite does not work

    }
}
