using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Example.NetCore5._0.CommonUsage.Models;
using Example.NetCore5._0.CommonUsage.Queries;
using TourmalineCore.AspNetCore.Pagination.Extensions;
using TourmalineCore.AspNetCore.Pagination.Models;

namespace Example.NetCore5._0.CommonUsage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsQuery _productsQuery;

        public ProductsController(ProductsQuery productsQuery)
        {
            _productsQuery = productsQuery;
        }

        [HttpGet("all")]
        public async Task<PaginationResult<ProductDto>> GetProducts()
        {
            var paginationParams = Request.Query.GetPaginationParams();
            return await _productsQuery.GetPageAsync(paginationParams);
        }
    }
}
