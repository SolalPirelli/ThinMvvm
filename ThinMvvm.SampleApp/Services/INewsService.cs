// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System.Threading.Tasks;
using ThinMvvm.SampleApp.Models;

namespace ThinMvvm.SampleApp.Services
{
    public interface INewsService
    {
        Task<NewsFeed> GetFeedAsync();
    }
}