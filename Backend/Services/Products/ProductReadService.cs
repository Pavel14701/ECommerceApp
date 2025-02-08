using Microsoft.EntityFrameworkCore;
using Npgsql;

public class PagedProductsDto : Result
{
    public List<IEnumerable<ProductPreviewDto>> Products { get; set; } = new List<IEnumerable<ProductPreviewDto>>();
}


public class ResultProductDto : Result
{
    public ProductDto? Product { get; protected set; }
    public ResultProductDto(Result result, ProductDto? product)
    {
        Success = result.Success;
        Message = result.Message;
        Product = product;
    }
}


public interface IProductReadService
{
    Task<PagedProductsDto> GetAllProducts();
    Task<PagedProductsDto> GetProductsToSearch();
    Task<PagedProductsDto> GetProductsByFilters();
    Task<ResultProductDto> GetProductById(Guid id);
}




public class ProductReadService : IProductReadService
{
    private readonly SessionIterator _sessionIterator;
    private readonly ReadCrud _readCrud;
    public ProductReadService
    (
        SessionIterator sessionIterator,
        ReadCrud readCrud
    )
    {
        _sessionIterator = sessionIterator;
        _readCrud = readCrud;
    }

    public async Task<PagedProductsDto> GetAllProducts()
    {
        try
        {
            return new PagedProductsDto
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new PagedProductsDto
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }

    public async Task<PagedProductsDto>  GetProductsToSearch()
    {
        try
        {
            return new PagedProductsDto
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new PagedProductsDto
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }

    public async Task<PagedProductsDto> GetProductsByFilters()
    {
        try
        {
            return new PagedProductsDto
            { 
                Success = true,
                Products = null
            };
        }
        catch (Exception ex)
        {
            return new PagedProductsDto
            {
                Success = false,
                Message = $"Error: {ex}",
                Products = null
            };
        }
    } 

    public async Task<ResultProductDto> GetProductById(Guid id)
    {
        try
        {
            return new ResultProductDto
            ( 
                new Result{ Success = true }, null 
            );
        }
        catch (Exception ex)
        {
            return new ResultProductDto
            (
                new Result{ Success = false, Message = $"Error: {ex.Message}" }, null 
            );
        }
    }
}