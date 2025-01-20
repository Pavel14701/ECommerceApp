using System.Collections.Generic;
using System.Threading.Tasks;

public interface INewsService
{
    Task<(IEnumerable<News>, int)> GetAllNews(int pageNumber, int pageSize);
    Task<News?> GetNewsById(Guid id);
    Task AddNews(News news);
    Task UpdateNews(News news);
    Task DeleteNews(Guid id);
    Task AddImageToNews(Guid newsId, Images image);
    Task RemoveImageFromNews(Guid newsId, Guid imageId);
    Task AddContentToNews(Guid newsId, Content content);
    Task RemoveContentFromNews(Guid newsId, Guid contentId);
    Task UpdateNewsTitle(Guid id, string title);
    Task UpdateNewsPublishDate(Guid id, DateTime publishDate);
    Task UpdateNewsContentText(Guid newsId, Guid contentId, string text);
}
