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

        [HttpGet("{numberOfTopStories}")]
        public async Task<ActionResult<IEnumerable<StoryDetailResponseDto>>> GetBestStories(int numberOfTopStories)
        {
            _logger.LogDebug($"GetBestStories called with parameter numberOfTopStories = {numberOfTopStories}");

            var bestStoriesIds = await GetBestStoriesIds();

            var stories = await GetBestStoriesDetails(bestStoriesIds, numberOfTopStories);

            _logger.LogDebug($"GetBestStories returning {stories.Count()} stories");
            return Ok(stories);
        }

        internal async Task<IEnumerable<StoryDetailResponseDto>> GetBestStoriesDetails(IEnumerable<int> storyIds, int numberOfTopStories)
        {
            if (storyIds == null || !storyIds.Any())
                return Enumerable.Empty<StoryDetailResponseDto>();

            var bestStories = await GetStoriesDetails(storyIds.Take(numberOfTopStories));

            if (_apiSettings.ForceReSortBestStoryIds)
            {
                var otherBestStories = await GetStoriesDetails(storyIds.Skip(numberOfTopStories));
                bestStories = bestStories.Union(otherBestStories).OrderByDescending(story => story.Score).Take(numberOfTopStories).ToList();
            }

            return bestStories;
        }

        internal async Task<IEnumerable<StoryDetailResponseDto>> GetStoriesDetails(IEnumerable<int> storyIds)
        {
            if (storyIds == null || !storyIds.Any())
                return Enumerable.Empty<StoryDetailResponseDto>();

            var tasks = storyIds.Select(async id =>
            {
                var cacheKey = CacheKeys.GetStoryDetailsCacheKey(id.ToString());
                if (!_cache.TryGetValue(cacheKey, out StoryDetailResponseDto storyDetailResponseDto))
                {
                    var storyHackerNewsDto = await FetchStoryDetails(id);
                    storyDetailResponseDto = storyHackerNewsDto.MapStoryHackerNewsToResponseDto();

                    await CacheStoryDetails(cacheKey, storyDetailResponseDto);
                }

                return storyDetailResponseDto;
            });

            return await Task.WhenAll(tasks);
        }

        internal async Task<StoryHackerNewsDto> FetchStoryDetails(int id)
        {
            var requestUrl = $"{_apiSettings.StoryDetailsUrl}{id}.json";
            var storyResponse = await _httpClient.GetAsync(requestUrl);

            if (!storyResponse.IsSuccessStatusCode)
            {
                // Handle error or return null based on your requirement
                return null;
            }

            var storyJson = await storyResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StoryHackerNewsDto>(storyJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });
        }

        internal async Task CacheStoryDetails(string cacheKey, StoryDetailResponseDto storyDetailResponseDto)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_apiSettings.StoryDetailsCacheExpiration)
            };
            await Task.Run(() => _cache.Set(cacheKey, storyDetailResponseDto, cacheEntryOptions));
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


      

       
    }
}
