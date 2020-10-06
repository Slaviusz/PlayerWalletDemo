using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlayerWalletAPI.Models.Request;
using PlayerWalletAPI.Models.Response;
using Shouldly;
using Xunit;

namespace PlayerWalletTests
{
    public partial class PlayerApiTests
    {

        [Fact]
        private async Task TestPlayerAdd()
        {
            var newPlayerGuid = Guid.NewGuid();

            var json = JsonSerializer
                .Serialize(new PlayerAddRequest
                {
                    TransactionId = newPlayerGuid,
                    BirthDate = DateTime.Now.AddYears(-25),
                    PlayerName = $"NewPlayer{new Random().Next(1000,9999)}"
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _factory.CreateClient();
            var response = await client.PostAsync("v1/Player/Add", requestModel);

            Should.NotThrow(() => { response.EnsureSuccessStatusCode(); });

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            PlayerModelResponse player = null;

            Should.NotThrow(async () => { player = JsonSerializer.Deserialize<PlayerModelResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            player.ShouldNotBeNull();
            player.Id.ShouldBe(newPlayerGuid);
            player.Repeated.ShouldBeFalse();

            // repeated request

            var client2 = _factory.CreateClient();
            var response2 = await client2.PostAsync("v1/Player/Add", requestModel);

            Should.NotThrow(() => { response2.EnsureSuccessStatusCode(); });

            response2.StatusCode.ShouldBe(HttpStatusCode.OK);

            PlayerModelResponse player2 = null;

            Should.NotThrow(async () => { player2 = JsonSerializer.Deserialize<PlayerModelResponse>(await response2.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            player2.ShouldNotBeNull();
            player2.Id.ShouldBe(newPlayerGuid);
            player2.Repeated.ShouldBeTrue();
        }

        [Fact]
        private async Task TestPlayerAdd_NameConflict()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new PlayerAddRequest
                {
                    TransactionId = Guid.NewGuid(),
                    BirthDate = DateTime.Now.AddYears(-25),
                    PlayerName = SeedPlayerName
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("v1/Player/Add", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        }

        [Fact]
        private async Task TestPlayerAdd_NameTooShort()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new PlayerAddRequest
                {
                    TransactionId = Guid.NewGuid(),
                    BirthDate = DateTime.Now.AddYears(-25),
                    PlayerName = "a"
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("v1/Player/Add", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problemDetails = await JsonSerializer
                .DeserializeAsync<ValidationProblemDetails>(response.Content.ReadAsStreamAsync().Result);

            problemDetails.Errors.ShouldContainKey("PlayerName");
            problemDetails.Errors["PlayerName"].First().ShouldMatch("name must be between");
        }

        [Fact]
        private async Task TestPlayerAdd_NameTooLong()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new PlayerAddRequest
                {
                    TransactionId = Guid.NewGuid(),
                    BirthDate = DateTime.Now.AddYears(-25),
                    PlayerName = "abcdefghijklmnopqrstuvwxyz"
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("v1/Player/Add", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problemDetails = await JsonSerializer
                .DeserializeAsync<ValidationProblemDetails>(response.Content.ReadAsStreamAsync().Result);

            problemDetails.Errors.ShouldContainKey("PlayerName");
            problemDetails.Errors["PlayerName"].First().ShouldMatch("name must be between");
        }

        [Fact]
        private async Task TestPlayerAdd_PlayerTooYoung()
        {
            var client = _factory.CreateClient();

            var json = JsonSerializer
                .Serialize(new PlayerAddRequest
                {
                    TransactionId = Guid.NewGuid(),
                    BirthDate = DateTime.Now.AddYears(-17).AddDays(-364),
                    PlayerName = "Kiddo123"
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("v1/Player/Add", requestModel);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problemDetails = await JsonSerializer
                .DeserializeAsync<ValidationProblemDetails>(response.Content.ReadAsStreamAsync().Result);

            problemDetails.Errors.ShouldContainKey("BirthDate");
            problemDetails.Errors["BirthDate"].First().ShouldMatch("must be at least 18 years old");
        }
    }
}
