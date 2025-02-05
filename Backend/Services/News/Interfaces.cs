public interface IDeleteNewsService
{
    Task<NewsDeletionResultDto> DeleteNews(Guid id);
    Task<ImageUpdateResultDto> RemoveImageFromNews(Guid newsId, Guid imageId);
    Task<ImageUpdateResultDto> DeleteImage(Guid newsId, Guid imageId);
}


public interface IUpdateNewsService
{
    Task<NewsUpdateResultDto> UpdateNews(News news);
    Task<ImageUpdateResultDto> UpdateImage(Guid newsId, Guid imageId, IFormFile file);
    Task<NewsUpdateResultDto> UpdateNewsTitle(Guid id, string title);
    Task<NewsUpdateResultDto> UpdateNewsPublishDate(Guid id, DateTime publishDate);
    Task<NewsUpdateResultDto> UpdateNewsContentText(Guid newsId, Guid contentId, string text);
}
public interface IReadNewsService
{
    Task<PagedNewsDto> GetAllNews(int pageNumber, int pageSize);
    Task<NewsDto> GetNewsById(Guid id);
}



