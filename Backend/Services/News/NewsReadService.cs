public class NewsPreviewResultsDto : Result
{
    public List<NewsPreviewDto>? Previews { get; set; }
}

public class NewsDetailsResultDto : Result
{
    public NewsDetailsDto? Details { get; set; }
}

public interface IReadNewsService
{
    Task<NewsPreviewResultsDto> GetPaginatedNews(PaginationParamsDto paramsDto);
    Task<NewsDetailsResultDto> GetNews(Guid id);
}


public class ReadNewsService : IReadNewsService
{
    private readonly ReadCrud _readCrud;

    public ReadNewsService(
        ReadCrud readCrud
    )
    {
        _readCrud = readCrud;
    }


    public async Task<NewsPreviewResultsDto> GetPaginatedNews(PaginationParamsDto paramsDto)
    {
        try
        {
            var result = await _readCrud.GetPaginatedNews(paramsDto);
            return new NewsPreviewResultsDto
            {
                Success = true,
                Previews = result
            };
        }
        catch (Exception ex)
        {
            return new NewsPreviewResultsDto
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }

    public async Task<NewsDetailsResultDto> GetNews(Guid id)
    {
        try
        {
            var result = await _readCrud.GetNewsDetailsById(id);
            return new NewsDetailsResultDto
            {
                Success = true,
                Details = result
            };
        }
        catch (Exception ex)
        {
            return new NewsDetailsResultDto
            {
                Success = false,
                Message = $"Error: {ex}"
            };
        }
    }
}
