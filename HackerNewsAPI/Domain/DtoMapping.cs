using System.Linq;

namespace HackerNewsAPI.Domain
{
    public static class DtoMappingHelper
    {

        /// <summary>
        /// In a proper large application , Use a mapper like mapperly, automapster, automapper  ( best reported performance is mapperly) 
        /// </summary>
        /// <param name="storyHackerNews"></param>
        /// <returns></returns>
        public static StoryDetailResponseDto MapStoryHackerNewsToResponseDto(this StoryHackerNewsDto storyHackerNews)
        {
            return new StoryDetailResponseDto
            {
                Title = storyHackerNews.Title,
                Uri = storyHackerNews.Url,
                PostedBy = storyHackerNews.By,
                Time = storyHackerNews.Time,
                Score = storyHackerNews.Score,
                CommentCount = storyHackerNews?.Kids?.Count() ?? 0
            };
          
        }

    }
    
}
