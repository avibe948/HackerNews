using HackerNewsAPI.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using HackerNewsAPI.Providers;

namespace HackerNewsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BestStoriesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly HackerNewsApiSettings _apiSettings;
        private readonly ILogger<BestStoriesController> _logger;
        private readonly IStoryProvider _storyProvider;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storyProvider">HackerNews story provider</param>
        /// <param name="httpClientFactory">Http client factory</param>
        /// <param name="apiSettings">Hacker news api settings</param>
        
        public BestStoriesController(ILogger<BestStoriesController> logger,
                                     IStoryProvider storyProvider,
                                     IHttpClientFactory httpClientFactory,
                                     IOptions<HackerNewsApiSettings> apiSettings)
        {
            
            _httpClient = httpClientFactory.CreateClient();
            _apiSettings = apiSettings.Value;
            _logger = logger;
            _storyProvider = storyProvider;
        }

        [HttpGet("{numberOfTopStories}")]
        public async Task<ActionResult<IEnumerable<StoryDetailResponseDto>>> GetBestStories(int numberOfTopStories)
        {
            _logger.LogDebug($"GetBestStories called with parameter numberOfTopStories = {numberOfTopStories}");

            var bestStoriesIds = await _storyProvider.GetBestStoriesIds();

            var stories = await GetBestStoriesDetails(bestStoriesIds, numberOfTopStories);

            _logger.LogDebug($"GetBestStories returning {stories.Count()} stories");
            return Ok(stories);
        }

        private async Task<IEnumerable<StoryDetailResponseDto>> GetBestStoriesDetails(IEnumerable<int> storyIds,
                                                                                      int numberOfTopStories)
        {
            if (storyIds == null || !storyIds.Any())
                return Enumerable.Empty<StoryDetailResponseDto>();

            var bestStories = await _storyProvider.GetStoriesDetails(storyIds.Take(numberOfTopStories));

            if (_apiSettings.ForceReSortBestStoryIds)
            {
                var otherBestStories = await _storyProvider.GetStoriesDetails(storyIds.Skip(numberOfTopStories));
                bestStories = bestStories.Union(otherBestStories).OrderByDescending(story => story.Score).Take(numberOfTopStories).ToList();
            }

            return bestStories;
        }



    }
}
