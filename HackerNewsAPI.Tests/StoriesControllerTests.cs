using Castle.Components.DictionaryAdapter.Xml;
using HackerNewsAPI.Controllers;
using HackerNewsAPI.Domain;
using HackerNewsAPI.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace HackerNewsAPI.Tests
{
    public class StoriesControllerTests
    {
        Mock<ILogger<BestStoriesController>> _loggerMock;
        public StoriesControllerTests()
        {
            _loggerMock = new Mock<ILogger<BestStoriesController>>();
        }
        // If I had more time I will add more test methods for different scenarios...

        [Fact]
        public async Task SholdReturnGetBestStoriesIdsAndCacheResults()
        {
            //Arrange
            HackerNewsApiSettings settings = new HackerNewsApiSettings()
            {
                ForceReSortBestStoryIds = false
            };
            var apiSettingsMock = new Mock<IOptions<HackerNewsApiSettings>>();
            apiSettingsMock.Setup(x => x.Value).Returns(settings);
            var mockStoryProvider = new Mock<IStoryProvider>();
            var expectedBestStoryIds = new int[] { 21233041, 21233229, 21232873, 21233237, 21233211 }.ToList();
            var expectedBestStoriesDetails = GetExpectedStoryResponses();
            
            mockStoryProvider.Setup(x => x.GetBestStoriesIdsAsync()).Returns(Task.FromResult(expectedBestStoryIds));
            mockStoryProvider.Setup(x => x.GetStoriesDetailsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(expectedBestStoriesDetails);

            // Act
            var controller = new BestStoriesController(_loggerMock.Object, mockStoryProvider.Object, apiSettingsMock.Object);
            var numBestStories = 2;       
            var result = await controller.GetBestStories(numBestStories);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsAssignableFrom<IEnumerable<StoryDetailResponseDto>>(okResult.Value);
            var actualBestStories = okResult.Value as IEnumerable<StoryDetailResponseDto>;

            Assert.Equal(actualBestStories, expectedBestStoriesDetails);
            // Serialize the expected response
            string expectedJson = JsonSerializer.Serialize(expectedBestStoriesDetails);
            // Serialize the actual response
            string actualJson = JsonSerializer.Serialize(actualBestStories);
            // Compare the serialized JSON strings
            Assert.Equal(expectedJson, actualJson);

        }

       
        private IEnumerable<StoryDetailResponseDto> GetExpectedStoryResponses()
        {
            return new[] {
                new StoryDetailResponseDto
                {
                    PostedBy = "ismaildonmez",
                    Title = "A uBlock Origin update was rejected from the Chrome Web Store",
                    Score = 1757,
                    Time = 1570887781,
                    Uri = "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
                    CommentCount = 572,
                    Id = 21233041
                },
                new StoryDetailResponseDto
                {
                    PostedBy = "some author",
                    Title = "some story title",
                    Score = 1 ,
                    Time = 1570887771,
                    Uri = "https://github.com/uBlockOrigin/uBlock-issues/issues/44",
                    CommentCount = 35,
                    Id = 21233229
                }
            };
        }

    }
}
