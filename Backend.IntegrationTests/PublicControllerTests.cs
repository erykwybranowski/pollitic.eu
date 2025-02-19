using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using backend;
using backend.Data;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.IntegrationTests
{
    public class PublicControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public PublicControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Fact]
        public async Task GetCountries_Returns_NonEmptyList()
        {
            var response = await _client.GetAsync("/api/public/countries");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));

            var countries = JsonSerializer.Deserialize<List<CountryDTO>>(content, _jsonOptions);
            Assert.NotNull(countries);
            Assert.NotEmpty(countries);
        }

        [Fact]
        public async Task GetPartiesByCountry_Returns_ListForPl()
        {
            var response = await _client.GetAsync("/api/public/parties/pl");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));

            var parties = JsonSerializer.Deserialize<List<PartyDTO>>(content, _jsonOptions);
            Assert.NotNull(parties);
            Assert.NotEmpty(parties);
        }

        [Fact]
        public async Task GetPollsByCountry_Returns_PollsForPl()
        {
            var response = await _client.GetAsync("/api/public/polls/pl");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));

            var polls = JsonSerializer.Deserialize<List<PollDTO>>(content, _jsonOptions);
            Assert.NotNull(polls);
            Assert.NotEmpty(polls);
        }

        [Fact]
        public async Task GetGroups_Returns_NonEmptyList()
        {
            var response = await _client.GetAsync("/api/public/groups");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));

            var groups = JsonSerializer.Deserialize<List<GroupDTO>>(content, _jsonOptions);
            Assert.NotNull(groups);
            Assert.NotEmpty(groups);
        }
    }
}
