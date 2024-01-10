using HackerNewsAPI.Controllers;
using HackerNewsAPI.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace HackerNewsAPI.Tests
{
    public class StoriesControllerTests
    {


        // If I had more time I will add more test methods for different scenarios...

        [Fact]
        public async Task SholdReturnGetBestStoriesIdsAndCacheResults()
        {

            var expectedBestStoryIds = new int[] { 21233041, 21233229, 21232873, 21233237, 21233211 };
            Mock<HttpMessageHandler> mockHttpMessageHandler = GetMockedHttpHandlerForStoryIds(expectedBestStoryIds);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerMock = new Mock<ILogger<BestStoriesController>>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var apiSettingsMock = new Mock<IOptions<HackerNewsApiSettings>>();
            apiSettingsMock.SetupGet(_ => _.Value).Returns(new HackerNewsApiSettings
            {
                BestStoriesUrl = "https://mocked.url/beststories.json",
                StoryDetailsUrl = "https://mocked.url/item/",
                BestStoryIdsCacheExpiration = 600,
                ForceReSortBestStoryIds = false,
                StoryDetailsCacheExpiration = 600,
            });

            var controller = new BestStoriesController(loggerMock.Object, httpClientFactoryMock.Object, apiSettingsMock.Object, cache);

            // Act
            var actualStoriesIds = await controller.GetBestStoriesIds();

            Assert.NotEmpty(actualStoriesIds);
            Assert.Equal(5, actualStoriesIds.Count());
            Assert.Equal(expectedBestStoryIds, actualStoriesIds);
            

            // Verify indirectly if caching occurred by checking if a method that uses Set is invoked
            var cachedStories = cache.Get("BestStories");

            Assert.NotNull(cachedStories);
            Assert.Equivalent(cachedStories, actualStoriesIds, true);
        }

        private static Mock<HttpMessageHandler> GetMockedHttpHandlerForStoryIds<T>(T obj)
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = JsonSerializer.Serialize(obj);
            var stringContent = new NonDisposingJsonStringContent(responseContent);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = stringContent
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);


            return mockHttpMessageHandler;
        }

        [Fact]
        public async Task SholdReturnGetBestStoriesDetailsAndCacheResults()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            Story story = GetStories().First();

            string storyJsonString = JsonSerializer.Serialize(story);

            var storyDetailsStringContent = new NonDisposingJsonStringContent(storyJsonString);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = storyDetailsStringContent
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerMock = new Mock<ILogger<BestStoriesController>>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var apiSettingsMock = new Mock<IOptions<HackerNewsApiSettings>>();
            apiSettingsMock.SetupGet(_ => _.Value).Returns(new HackerNewsApiSettings
            {
                BestStoriesUrl = "https://mocked.url/beststories.json",
                StoryDetailsUrl = "https://mocked.url/item/",
                BestStoryIdsCacheExpiration = 600,
                ForceReSortBestStoryIds = false,
                StoryDetailsCacheExpiration = 600,
            });

            var controller = new BestStoriesController(loggerMock.Object, httpClientFactoryMock.Object, apiSettingsMock.Object, cache);

            var storyIds = new int[] { story.Id };
            // Act
            var storyDetails = await controller.GetStoriesDetails(storyIds);

            Assert.NotEmpty(storyDetails);
            Assert.Equal(1, storyDetails.Count());

            var storyDetail = storyDetails.First();
            {
                Assert.Equal(storyDetail.Id, story.Id);
                Assert.Equal(storyDetail.Title, story.Title);
                Assert.Equal(storyDetail.Score, story.Score);
                Assert.Equal(storyDetail.Type, story.Type);
                Assert.Equal(storyDetail.Url, story.Url);

                // Verify indirectly if caching occurred by checking if a method that uses Set is invoked
                var cachedStoryDetails = cache.Get<Story>($"Story_{storyDetail.Id}");

                Assert.NotNull(cachedStoryDetails);
                Assert.Equal(cachedStoryDetails, storyDetail);

            }
        }
        [Fact]
        public async Task GetBestStories_CachesData()
        {
            var settings = new HackerNewsApiSettings
            {
                BestStoriesUrl = "https://mocked.url/beststories.json",
                StoryDetailsUrl = "https://mocked.url/item/",
                BestStoryIdsCacheExpiration = 600,
                ForceReSortBestStoryIds = false,
                StoryDetailsCacheExpiration = 600,
            };

            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var bestStoryIds = new[] { 21233041, 21233229 };
            var bestStoriesIdsResponseContent = JsonSerializer.Serialize(bestStoryIds);
            IEnumerable<string> storyDetailsAsJson = GetStories().Select(story => JsonSerializer.Serialize<Story>(story));

            mockHttpMessageHandler.Protected()
         .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req =>
             req.RequestUri.AbsoluteUri == settings.BestStoriesUrl),
             ItExpr.IsAny<CancellationToken>())
         .ReturnsAsync(new HttpResponseMessage
         {
             StatusCode = HttpStatusCode.OK,
             Content = new NonDisposingJsonStringContent(bestStoriesIdsResponseContent)
         });

            // Setup for Story Details request
            SetupMockedStoryDetailMessageHandler(settings, mockHttpMessageHandler, bestStoryIds.First(), storyDetailsAsJson.First());
            SetupMockedStoryDetailMessageHandler(settings, mockHttpMessageHandler, bestStoryIds.Last(), storyDetailsAsJson.Last());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerMock = new Mock<ILogger<BestStoriesController>>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var apiSettingsMock = new Mock<IOptions<HackerNewsApiSettings>>();
            apiSettingsMock.SetupGet(_ => _.Value).Returns(settings);

            var controller = new BestStoriesController(loggerMock.Object, httpClientFactoryMock.Object, apiSettingsMock.Object, cache);

            // Act
            var result = await controller.GetBestStories(5);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var stories = Assert.IsAssignableFrom<IEnumerable<Story>>(okObjectResult.Value);
            Assert.Equal(2, stories.Count());

            // Verify indirectly if caching occurred by checking if a method that uses Set is invoked
            var cachedStories = cache.Get<IEnumerable<int>>("BestStories");
            Assert.Equal(cachedStories, stories.Select(x => x.Id));
        }

        private Story[] GetStories()
        {
            return new[]
            {
                new Story
                {
                    By = "ismaildonmez",
                    Descendants = 588,
                    Id = 21233041,
                    Kids = new int[] { 21233229 },
                    Score = 1757,
                    Time = 1570887781,
                    Title = "A uBlock Origin update was rejected from the Chrome Web Store",
                    Type = StoryType.Story,
                    Url = "https://github.com/uBlockOrigin/uBlock-issues/issues/745"
                },
                new Story
                {
                    By = "fadfdafa",
                    Descendants = 645,
                    Id = 21233229,
                    Kids = new int[] { 6451 },
                    Score = 1757,
                    Time = 4545645,
                    Title = "whatever story",
                    Type = StoryType.Story,
                    Url = "https://github.com/uBlockOrigin/uBlock-issues/issues/645"
                }
            };
        }



        private static void SetupMockedStoryDetailMessageHandler(HackerNewsApiSettings settings, Mock<HttpMessageHandler> mockHttpMessageHandler, int bestStoryId, string storyDetailsAsJson)
        {
            mockHttpMessageHandler.Protected()
                            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req =>
                                req.RequestUri.AbsoluteUri.StartsWith($"{settings.StoryDetailsUrl}{bestStoryId}")),
                                ItExpr.IsAny<CancellationToken>())
                            .ReturnsAsync(new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = new NonDisposingJsonStringContent(storyDetailsAsJson)
                            });
        }
    }
}
