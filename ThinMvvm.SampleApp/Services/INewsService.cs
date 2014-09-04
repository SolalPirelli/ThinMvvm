// Copyright (c) Solal Pirelli 2014
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