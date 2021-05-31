# TourmalineCore.AspNetCore.Pagination

This library can work with version .NET Core 3.0 and higher. 

In order to ensure compatibility of the library with .NET Core 3.0, we use Z.EntityFramework.Plus.EFCore version 5.1.29.
This library contains models and queryable extensions.
With this library, you can very easily implement paging, filtering and ordering of your EF Core queries.
Also, this library contains tools to extract query params from HTTP requests.

## Implement Pagination Query
Create a class that will inherit from **PageQueryBase** class provided by this package.

```c#
...
using TourmalineCore.AspNetCore.Pagination;
using TourmalineCore.AspNetCore.Pagination.Extensions;
using TourmalineCore.AspNetCore.Pagination.Models;

public class ProductsQuery : PageQueryBase<Product, ProductDto>{
    private readonly AppDbContext _context;

    public ProductsQuery(AppDbContext context)
    {
        _context = context;
    }
}
```

To implement basic functionality you will need to override **Map** method. 

```c#
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
```

After that you will be able to use GetPageByPaginationParamsAsync method in your queries like that:

```c#
public Task<PaginationResult<ProductDto>> GetPageAsync(PaginationParams paginationParams)
{
    var queryable = _context
        .Products
        .AsQueryable()
        .AsNoTracking();

    return GetPageByPaginationParamsAsync(
            queryable,
            paginationParams
        );
}
```

At this stage GetPageByPaginationParamsAsync method does pagination only. To implement other functionality you will need to override relevant methods. 

## Query modification

### Include

To include some related entities to the query you will need to override **DoIncludes** method.

```c#
protected override IQueryable<Product> DoIncludes(IQueryable<Product> queryable)
{
    return queryable
        .Include(x => x.Vendor);
}
```

### Filter

To filter the query you will need to override **DoFiltration** method.

```c#
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
```

### Order

To sort the query you will need to override **DoOrdering** method.

```c#
protected override IOrderedQueryable<Product> DoOrdering(
  IQueryable<Product> queryable, 
  string orderBy, 
  ListSortDirection sortDirection)
{
    return orderBy switch
            {
                nameof(ProductDto.VendorName) => sortDirection == ListSortDirection.Ascending 
                    ? queryable.OrderBy(x => x.Vendor.Name.ToLower())
                    : queryable.OrderByDescending(x => x.Vendor.Name.ToLower()),
                _ => queryable.OrderBy(orderBy, sortDirection),
            };
}
```

## Query Usage

To use pagination query you will need to provide PaginationParams object as input params. You can get that object from the Query of the Http Request by using GetPaginationParams extension.

```c#
...
using TourmalineCore.AspNetCore.Pagination.Extensions;
using TourmalineCore.AspNetCore.Pagination.Models;
...

[HttpGet]
public async Task<PaginationResult<ProductDto>> GetProducts()
{
    var paginationParams = Request.Query.GetPaginationParams();
    return await _productsQuery.GetPageAsync(paginationParams);
}
```

If you will choose to use that method you will need to ensure that queries provided to a request consist of certain parameters:

| Name | Type | Default | Description |
|-|-|-|-|
| draw | int | 1 | This value returns in the response completelly unchanged. It can be used by the client to define queries order.  |
| page | int | 1 | Number of page to take |
| pageSize | int | 10 | Number that defines size of the pages |
| orderBy | string | "" | Property name used for sorting |
| orderingDirection | string | "" | Any string for ascending order or 'desc' for descending |
| filteredByColumns | string[] | [] | Collection of property names to be used for filtering separated by coma |
| filteredByValues | string[] | [] | Collection of property values to be used for filtering separated by coma. Thier indexes must correspond with the ones from the *filteredByColumns* array |

Example:
```
https://{app-url}/products/all?draw=2&page=1&pageSize=100&orderBy=name&orderingDirection=desc&filteredByColumns=name,vendorName&filteredByValues=First,TestVendor1
```