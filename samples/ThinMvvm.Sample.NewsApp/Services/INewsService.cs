using System.Threading.Tasks;
using ThinMvvm.Sample.NewsApp.Models;

namespace ThinMvvm.Sample.NewsApp.Services
{
    public interface INewsService
    {
        Task<NewsFeed> GetFeedAsync();
    }
}