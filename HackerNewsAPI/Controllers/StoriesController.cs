using HackerNewsAPI.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HackerNewsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public StoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("{n}")]
        public async Task<ActionResult<IEnumerable<Story>>> GetBestStories(int n)
        {
            var bestStoriesIds = await GetBestStoriesIds();

            var stories = new List<Story>();
            for (int i = 0; i < n && i < bestStoriesIds.Count; i++)
            {
                var story = await GetStoryDetails(bestStoriesIds[i]);
                if (story != null)
                {
                    stories.Add(story);
                }
            }

            return Ok(stories);
        }

        private async Task<List<int>> GetBestStoriesIds()
        {
            var response = await _httpClient.GetFromJsonAsync<List<int>>(
                "https://hacker-news.firebaseio.com/v0/beststories.json");

            return response ?? new List<int>();
        }

        private async Task<Story> GetStoryDetails(int storyId)
        {
            var response = await _httpClient.GetFromJsonAsync<Story>(
                $"https://hacker-news.firebaseio.com/v0/item/{storyId}.json");

            return response;
        }
    }
}
