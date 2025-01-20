using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public interface INewsService
{
    Task<(IEnumerable<News>, int)> GetAllNews(int pageNumber, int pageSize);
    Task<News?> GetNewsById(Guid id);
    Task AddNews(News news);
    Task UpdateNews(News news);
    Task DeleteNews(Guid id);
    Task AddImageToNews(Guid newsId, Images image);
    Task RemoveImageFromNews(Guid newsId, Guid imageId);
    Task<Images?> UploadImage(Guid newsId, IFormFile file);
    Task<bool> DeleteImage(Guid newsId, Guid imageId);
    Task<Images?> UpdateImage(Guid newsId, Guid imageId, IFormFile file);
    Task UpdateNewsTitle(Guid id, string title);
    Task UpdateNewsPublishDate(Guid id, DateTime publishDate);
    Task UpdateNewsContentText(Guid newsId, Guid contentId, string text);
}
