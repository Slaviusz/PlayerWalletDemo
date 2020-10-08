using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using PlayerWalletAPI.Models.Response;
using Shouldly;
using Xunit;

namespace PlayerWalletTests
{
    public partial class PlayerApiTests : IClassFixture<WebApplicationFactory<PlayerWalletAPI.Startup>>
    {
        private static readonly Guid SeedPlayerId = Guid.ParseExact("11111111111111111111111111111111", "N");
        private const string SeedPlayerName = "NicknameJim";

        private readonly WebApplicationFactory<PlayerWalletAPI.Startup> _factory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public PlayerApiTests(WebApplicationFactory<PlayerWalletAPI.Startup> factory)
        {
            _factory = factory;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        private async Task TestPlayerGetAll()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("v1/Player/GetAll");

            Should.NotThrow(() => { response.EnsureSuccessStatusCode(); });

            PlayerModelResponse[] players = null;

            Should.NotThrow(async () => { players = JsonSerializer.Deserialize<PlayerModelResponse[]>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            players.ShouldNotBeNull();
            players.Length.ShouldBeGreaterThanOrEqualTo(1); // Database seeding should provide at least one Player
            players.FirstOrDefault(player => player.Id == SeedPlayerId).ShouldNotBeNull();
        }

        [Fact]
        private async Task TestPlayerGetById()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"v1/Player/{SeedPlayerId.ToString()}");

            Should.NotThrow(() => { response.EnsureSuccessStatusCode(); });

            PlayerModelResponse player = null;

            Should.NotThrow(async () => { player = JsonSerializer.Deserialize<PlayerModelResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions); });

            player.ShouldNotBeNull();
            player.Id.ShouldBe(SeedPlayerId);
        }

        [Fact]
        private async Task TestPlayerGetById_NonExistentId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"v1/Player/{Guid.NewGuid().ToString()}");

            response.ShouldNotBeNull();
            response.StatusCode.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        private async Task TestPlayerGetById_InvalidId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"v1/Player/00000000-0000-0000-0000-00000000000x");

            response.ShouldNotBeNull();
            response.StatusCode.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }
}
