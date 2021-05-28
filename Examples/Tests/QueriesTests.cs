using System;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Example.NetCore5._0.CommonUsage.Models;
using TourmalineCore.AspNetCore.Pagination.Models;
using Xunit;

namespace Tests
{
    public class QueriesTests : IClassFixture<WebApplicationFactory<Example.NetCore5._0.CommonUsage.Startup>>
    {
        private readonly WebApplicationFactory<Example.NetCore5._0.CommonUsage.Startup> _factory;
        private readonly JsonSerializerOptions _jsonSerializerSettings;

        private const string Path = "/products/all";

        public QueriesTests(WebApplicationFactory<Example.NetCore5._0.CommonUsage.Startup> factory)
        {
            _factory = factory;

            _jsonSerializerSettings = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                AllowTrailingCommas = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(),
                },
            };
        }

        [Fact]
        public async Task NoQueryTest()
        {
            var result = await SendQuery("");

            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task FiltrationTest_CompleteMatch()
        {
            var result = await SendQuery($"filteredByColumns=vendorName&filteredByValues=TestVendor2");

            var product = Assert.Single(result.List);
            Assert.Equal("TestVendor2", product.VendorName);
        }

        [Fact]
        public async Task FiltrationTest_PartialMatch()
        {
            var result = await SendQuery($"filteredByColumns=vendorName&filteredByValues=Vendor");

            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task FiltrationTest_NoMatch()
        {
            var result = await SendQuery($"filteredByColumns=vendorName&filteredByValues=Bendor");

            Assert.Empty(result.List);
        }

        [Fact]
        public async Task FiltrationTest_InvalidColumnThrowsException()
        {
            try
            {
                await SendQuery($"filteredByColumns=invalid&filteredByValues=TestVendor2");
            }
            catch (Exception e)
            {
                Assert.IsType<InvalidOperationException>(e);
            }
        }

        [Fact]
        public async Task OrderingTest()
        {
            var result = await SendQuery($"orderBy=name&orderingDirection=desc");

            Assert.Equal("Third", result.List[0].Name);
            Assert.Equal("Second", result.List[1].Name);
            Assert.Equal("First", result.List[2].Name);
        }

        [Fact]
        public async Task PageSizeTest()
        {
            var result = await SendQuery($"pageSize=1&page=2&orderBy=name");

            var product = Assert.Single(result.List);
            Assert.Equal("Second", product.Name);
        }

        [Fact]
        public async Task ComplexQueryTest()
        {
            var result = await SendQuery($"pageSize=3&page=1&orderBy=name&orderingDirection=desc&filteredByColumns=vendorName&filteredByValues=TestVendor1");

            Assert.Equal(2, result.TotalCount);
            Assert.Equal("Second", result.List[0].Name);
            Assert.Equal("First", result.List[1].Name);
        }

        [Fact]
        public async Task DrawParamReturnsUnchangedTest()
        {
            const int draw = 5;
            var result = await SendQuery($"draw={draw}&pageSize=1&page=2");

            Assert.Equal(draw, result.Draw);
        }

        private async Task<PaginationResult<ProductDto>> SendQuery(string query)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{Path}?{query}");
            var content = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<PaginationResult<ProductDto>>(content, _jsonSerializerSettings);
        }
    }
}
