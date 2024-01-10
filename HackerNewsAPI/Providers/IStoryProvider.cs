using HackerNewsAPI.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HackerNewsAPI.Providers
{
    public interface IStoryProvider
    {
        Task<List<int>> GetBestStoriesIds();
    
        Task<IEnumerable<StoryDetailResponseDto>> GetStoriesDetails(IEnumerable<int> storyIds);
    }
}
