using System.Threading.Tasks;
using ThinMvvm.Sample.NewsReader.Models;

namespace ThinMvvm.Sample.NewsReader.Services
{
    public interface INewsService
    {
        Task<NewsFeed> GetFeedAsync();
    }
}