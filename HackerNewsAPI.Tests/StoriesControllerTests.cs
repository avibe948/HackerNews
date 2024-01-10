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
            HackerNewsApiSettings settings = GetAppSettings();

            var expectedBestStoryIds = new int[] { 21233041, 21233229, 21232873, 21233237, 21233211 };
            Mock<HttpMessageHandler> mockHttpMessageHandler = GetMockedHttpHandlerForStoryIds(expectedBestStoryIds);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerMock = new Mock<ILogger<BestStoriesController>>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var apiSettingsMock = new Mock<IOptions<HackerNewsApiSettings>>();
            apiSettingsMock.SetupGet(_ => _.Value).Returns(settings);

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

        private static HackerNewsApiSettings GetAppSettings()
        {
            return new HackerNewsApiSettings
            {
                BestStoriesUrl = "https://mocked.url/beststories.json",
                StoryDetailsUrl = "https://mocked.url/item/",
                BestStoryIdsCacheExpiration = 600,
                ForceReSortBestStoryIds = false,
                StoryDetailsCacheExpiration = 600,
            };
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
            StoryDetailResponseDto[] expectedStoriesDetailsResponseDtos = GetStories().Select(story => story.MapStoryHackerNewsToResponseDto()).ToArray();
            string[] expectedStoryDetailsAsJsonString = expectedStoriesDetailsResponseDtos.Select(x => JsonSerializer.Serialize(x)).ToArray();

            var settings = GetAppSettings();

            for (int i = 0; i < expectedStoriesDetailsResponseDtos.Count(); i++)
            {
                SetupMockedStoryDetailMessageHandler(settings, mockHttpMessageHandler, expectedStoriesDetailsResponseDtos[i].Id, expectedStoryDetailsAsJsonString[i]);
            }

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerMock = new Mock<ILogger<BestStoriesController>>();
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var apiSettingsMock = new Mock<IOptions<HackerNewsApiSettings>>();
            apiSettingsMock.SetupGet(_ => _.Value).Returns(settings);

            var controller = new BestStoriesController(loggerMock.Object, httpClientFactoryMock.Object, apiSettingsMock.Object, cache);

            var storyIds = expectedStoriesDetailsResponseDtos.Select(x => x.Id);

            // Act
            var actualStoriesDetails = await controller.GetStoriesDetails(storyIds);

            Assert.NotEmpty(actualStoriesDetails);
            Assert.Equal(2, actualStoriesDetails.Count());

            foreach (var storyDetail in actualStoriesDetails)
            {

                Assert.Equal(storyDetail.Id, storyDetail.Id);

                // Verify indirectly if caching occurred by checking if a method that uses Set is invoked
                var cachedStoryDetails = cache.Get<StoryDetailResponseDto>($"Story_{storyDetail.Id}");
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
            var expectedStoriesDetails = GetStories();

            var bestStoryIds = expectedStoriesDetails.Select(x => x.Id).ToArray();
            var bestStoriesIdsResponseContent = JsonSerializer.Serialize(bestStoryIds);
            IEnumerable<string> storyDetailsAsJson = expectedStoriesDetails.Select(story => JsonSerializer.Serialize<StoryHackerNewsDto>(story));

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
            var stories = Assert.IsAssignableFrom<IEnumerable<StoryDetailResponseDto>>(okObjectResult.Value);
            Assert.Equal(2, stories.Count());

            // Verify indirectly if caching occurred by checking if a method that uses Set is invoked
            var cachedStories = cache.Get<IEnumerable<int>>("BestStories");
        
            Assert.Equal(cachedStories, bestStoryIds);

            // check story details is stored in the cache : 
            foreach (var storyId in bestStoryIds)
            {
                var cachedStoryDetails = cache.Get<StoryDetailResponseDto>($"Story_{storyId}");
                Assert.Equal(expectedStoriesDetails.First(x => x.Id == storyId).MapStoryHackerNewsToResponseDto(), cachedStoryDetails); 
            }

        }

        private StoryHackerNewsDto[] GetStories()
        {
            return new[]
            {
                new StoryHackerNewsDto
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
                new StoryHackerNewsDto
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
