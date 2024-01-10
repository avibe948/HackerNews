using HackerNewsAPI.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HackerNewsAPI.Providers
{
    public interface IStoryProvider
    {
        Task<List<int>> GetBestStoriesIdsAsync();
    
        Task<IEnumerable<StoryDetailResponseDto>> GetStoriesDetailsAsync(IEnumerable<int> storyIds);
    }
}
