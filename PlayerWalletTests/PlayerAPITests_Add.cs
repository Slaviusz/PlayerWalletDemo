using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
            var client = _factory.CreateClient();

            var newPlayerGuid = Guid.NewGuid();

            var json = JsonSerializer
                .Serialize(new PlayerAddRequest
                {
                    TransactionId = newPlayerGuid,
                    BirthDate = DateTime.Now.AddYears(-25),
                    PlayerName = $"NewPlayer{new Random().Next(1000,9999)}"
                });

            var requestModel = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("v1/Player/Add", requestModel);

            Should.NotThrow(() => { response.EnsureSuccessStatusCode(); });

            PlayerModelResponse player = null;

            Should.NotThrow(async () => { player = JsonSerializer.Deserialize<PlayerModelResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            player.ShouldNotBeNull();
            player.Id.ShouldBe(newPlayerGuid);
            player.Repeated.ShouldBeFalse();

            // repeated request

            var response2 = await client.PostAsync("v1/Player/Add", requestModel);

            Should.NotThrow(() => { response2.EnsureSuccessStatusCode(); });

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
    }
}
