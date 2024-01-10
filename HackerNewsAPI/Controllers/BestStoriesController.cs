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

namespace HackerNewsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BestStoriesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly HackerNewsApiSettings _apiSettings;
        private readonly ILogger<BestStoriesController> _logger;
        private readonly IMemoryCache _cache;
        
        public BestStoriesController(ILogger<BestStoriesController> logger, IHttpClientFactory httpClientFactory, IOptions<HackerNewsApiSettings> apiSettings, IMemoryCache cache)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiSettings = apiSettings.Value;
            _logger = logger;
            _cache = cache;
        }
        /// <summary>
        /// An Http Get endpoint which takes an integer : numberOfTopStories (N) and returns the story details of the top N best stories (sorted by the highest score first)
        /// </summary>
        /// <param name="numberOfTopStories">Integer that represents how many highest score stories you wish to retreive. For example 3 will return , the 3 highest scores</param>
        /// <returns></returns>
        [HttpGet("{numberOfTopStories}")]
        public async Task<ActionResult<IEnumerable<StoryDetailResponseDto>>> GetBestStories(int numberOfTopStories)
        {
            _logger.LogDebug($"GetBestStories called with parameter numberOfTopStories = {numberOfTopStories} ");

            var bestStoriesIds = await GetBestStoriesIds();

            var stories = new List<StoryDetailResponseDto>(numberOfTopStories);
            var bestStories = await GetStoriesDetails(bestStoriesIds.Take(numberOfTopStories));

            stories.AddRange(bestStories);

            // If we can't rely on the best story ids being sorted already by the best score , as they are now ( but may not be at some point in the future) , then we need to get 
            // all the story details and then resort. This is not the default option in the app settings as I did notice thathe best story ids are already sorted hence this only happens if the 
            // app setting ForceReSortBestStoryIds will be set to true. 

            if (_apiSettings.ForceReSortBestStoryIds) // read above not used by default. 
            {
                stories.Capacity = bestStoriesIds.Count();  
                var otherBestStories = await GetStoriesDetails(bestStoriesIds.Skip(numberOfTopStories));
                stories = stories.Union(otherBestStories).Take(numberOfTopStories).OrderByDescending(story => story.Score).ToList();
                                                                  
            }
            _logger.LogDebug($"GetBestStories returning {stories.Count} ids");

            return Ok(stories);
        }

        /// <summary>
        /// Gets the best story ids by calling the url specified in the BestStoriesURL settings of the HackerNewsSettings section in in appsettings.json
        /// The values of the best stories are cached. The cache expiration policy can be specified in the app service configuration. 
        /// </summary>
        /// <returns>A list of story id integers</returns>
        internal async Task<List<int>> GetBestStoriesIds()
        {
            _logger.LogDebug($"GetBestStoriesIds called");

            if (!_cache.TryGetValue(CacheKeys.BestStoriesCacheKey, out List<int> bestStoriesIds))
            {
                var response = await _httpClient.GetFromJsonAsync<List<int>>(_apiSettings.BestStoriesUrl);
                // cache entry expiration policy
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_apiSettings.BestStoryIdsCacheExpiration) // Set your desired cache duration in the app settings. 
                };
                _cache.Set(CacheKeys.BestStoriesCacheKey, response, cacheEntryOptions);
                return response;
            }
            _logger.LogDebug($"GetBestStoriesIds returing {bestStoriesIds.Count} ids");

            return bestStoriesIds;
        }


        /// <summary>
        /// Gets a story details from the StoryDetailsUrl which is specified in the HackernewsSettings object
        /// </summary>
        /// <param name="storyIds"> a list of story ids</param>
        /// <returns>an array of the story details for the requested story ids</returns>

        internal async Task<IEnumerable<StoryDetailResponseDto>> GetStoriesDetails(IEnumerable<int> storyIds)
        {
            _logger.LogDebug($"GetStoriesDetails requested to get story details of story ids: {storyIds}");

            if (storyIds == null || !storyIds.Any()) return Enumerable.Empty<StoryDetailResponseDto>();

            var tasks = storyIds.Select(async id =>
            {
                var cacheKey = CacheKeys.GetStoryDetailsCacheKey(id.ToString());
                if (!_cache.TryGetValue(cacheKey, out StoryDetailResponseDto storyDetailResponseDto))
                {
                    var requestUrl = $"{_apiSettings.StoryDetailsUrl}{id}.json";
                    _logger.LogDebug($"GetStoriesDetails is calling  to get story details of story ids: {storyIds}");
                    var storyResponse = await _httpClient.GetAsync(requestUrl);

                    if (!storyResponse.IsSuccessStatusCode)
                    {
                        // Handle error or return null based on your requirement
                        return null;
                    }

                    var storyJson = await storyResponse.Content.ReadAsStringAsync();
                    var storyHackerNewsDto = JsonSerializer.Deserialize<StoryHackerNewsDto>(storyJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                    });

                    storyDetailResponseDto = storyHackerNewsDto.MapStoryHackerNewsToResponseDto();

                    // cache entry expiration policy
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_apiSettings.StoryDetailsCacheExpiration) // Set your desired cache duration in the app settings. 
                    };
                    _cache.Set(cacheKey, storyDetailResponseDto, cacheEntryOptions);
                }
                _logger.LogDebug($"GetBestStoriesIds returing {storyIds.Count()} ids");
                return storyDetailResponseDto;
            });

            return await Task.WhenAll(tasks);
        }

       
    }
}
