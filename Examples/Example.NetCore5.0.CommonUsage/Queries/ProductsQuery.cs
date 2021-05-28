using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Example.NetCore5._0.CommonUsage.Models;
using TourmalineCore.AspNetCore.Pagination;
using TourmalineCore.AspNetCore.Pagination.Extensions;
using TourmalineCore.AspNetCore.Pagination.Models;

namespace Example.NetCore5._0.CommonUsage.Queries
{
    public class ProductsQuery : PageQueryBase<Product, ProductDto>
    {
        private readonly AppDbContext _context;

        public ProductsQuery(AppDbContext context)
        {
            _context = context;
        }

        public Task<PaginationResult<ProductDto>> GetPageAsync(PaginationParams paginationParams)
        {
            var queryable = _context.Products.AsQueryable().AsNoTracking();

            return GetPageByPaginationParamsAsync(
                    queryable,
                    paginationParams
                );
        }

        protected override IQueryable<Product> DoIncludes(IQueryable<Product> queryable)
        {
            return queryable
                .Include(x => x.Vendor);
        }

        protected override IQueryable<Product> DoFiltration(IQueryable<Product> queryable, ColumnFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Value))
            {
                return queryable;
            }

            return filter.Name switch
                   {
                       nameof(ProductDto.Name) => queryable.Where(x => x.Name.ToLower().Contains(filter.Value)),
                       nameof(ProductDto.ExpirationDate) => queryable.Where(x => x.ExpirationDate.ToString().Contains(filter.Value)),
                       nameof(ProductDto.VendorName) => queryable.Where(x => x.Vendor.Name.ToLower().Contains(filter.Value)),
                       _ => throw new InvalidOperationException($"Unexpected filter name: {filter.Name}"),
                   };
        }

        protected override IOrderedQueryable<Product> DoOrdering(IQueryable<Product> queryable, string orderBy, ListSortDirection sortDirection)
        {
            return orderBy switch
                   {
                       nameof(ProductDto.VendorName) => sortDirection == ListSortDirection.Ascending 
                            ? queryable.OrderBy(x => x.Vendor.Name.ToLower())
                            : queryable.OrderByDescending(x => x.Vendor.Name.ToLower()),
                       _ => queryable.OrderBy(orderBy, sortDirection),
                   };
        }

        protected override Task<List<ProductDto>> Map(List<Product> entities)
        {
            var dtos = entities.Select(x => new ProductDto
                    {
                        Name = x.Name,
                        Cost = x.Cost,
                        ExpirationDate = x.ExpirationDate,
                        VendorName = x.Vendor.Name,
                    }
                )
                .ToList();

            return Task.FromResult(dtos);
        }
    }
}
