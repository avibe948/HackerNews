using HackerNewsAPI.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace HackerNewsAPI.Providers
{


    public class HackerNewsStoryProvider : IStoryProvider
    {
        private readonly HttpClient _httpClient;
        private readonly HackerNewsStoryProviderSettings _providerSettings;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HackerNewsStoryProvider> _logger;

        public HackerNewsStoryProvider(ILogger<HackerNewsStoryProvider> logger,
               IHttpClientFactory httpClientFactory,
               IOptions<HackerNewsStoryProviderSettings> providerSettings, 
               IMemoryCache cache)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _providerSettings = providerSettings.Value ?? throw new NullReferenceException("Provider settings must be supplied");
            _cache = cache ?? throw new NullReferenceException("memory cache must be supplied");
        }

        public async Task<List<int>> GetBestStoriesIdsAsync()
        {
            _logger.LogDebug($"GetBestStoriesIds called");

            if (!_cache.TryGetValue(CacheKeys.BestStoriesCacheKey, out List<int> bestStoriesIds))
            {
                var response = await _httpClient.GetFromJsonAsync<List<int>>(_providerSettings.BestStoriesUrl);
                // cache entry expiration policy
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_providerSettings.BestStoryIdsCacheExpiration) // Set your desired cache duration in the app settings. 
                };
                _cache.Set(CacheKeys.BestStoriesCacheKey, response, cacheEntryOptions);
                return response;
            }
            _logger.LogDebug($"GetBestStoriesIds returing {bestStoriesIds.Count} ids");

            return bestStoriesIds;
        }
        public async Task<IEnumerable<StoryDetailResponseDto>> GetStoriesDetailsAsync(IEnumerable<int> storyIds)
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
        public async Task<StoryHackerNewsDto> FetchStoryDetails(int id)
        {
            var requestUrl = $"{_providerSettings.StoryDetailsUrl}{id}.json";
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
        private async Task CacheStoryDetails(string cacheKey, StoryDetailResponseDto storyDetailResponseDto)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_providerSettings.StoryDetailsCacheExpiration)
            };
            await Task.Run(() => _cache.Set(cacheKey, storyDetailResponseDto, cacheEntryOptions));
        }

       
    }
}
