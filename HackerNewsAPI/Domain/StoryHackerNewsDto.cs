
using System.Text.Json.Serialization;

namespace HackerNewsAPI.Domain
{
    public enum StoryType {Story}

    /// <summary>
    /// The DTO representation of a story returned from the hacker news story details URL
    /// </summary>
    public record StoryHackerNewsDto
    {
        public string By { get; init; }
        public int Descendants { get; init; }
        public int Id { get; init; }
        public int[] Kids { get; init; }
        public int Score { get; init; }
        public int Time { get; init; }
        public string Title { get; init; }
        public StoryType Type { get; init; }
        public string Url { get; init; }
    }

    /// <summary>
    /// The domain representation of a story
    /// </summary>
    public record StoryDetailModel
    {
        public required string By { get; init; }
        public int Descendants { get; init; }
        public required int Id { get; init; }
        public required int[] Kids { get; init; }
        public required int Score { get; init; }
        public required int Time { get; init; }
        public required string Title { get; init; }
        public required StoryType Type { get; init; }
        public required string Url { get; init; }

    }

    /// <summary>
    /// The story response DTO returned from GetBestStories Http Get call of the BestStoriesController
    /// </summary>
    public record StoryDetailResponseDto
    {
        public string Title { get; init; }
        public string Uri { get; init; }
        [property: JsonPropertyName("postedBy")]
        public string PostedBy { get; init; }
        public int Time { get; init; }
        public int Score { get; init; }

        [property: JsonPropertyName("commentCount")]
        public int CommentCount { get; init; }

        [property: JsonIgnore]
        public int Id { get; init; }
    }


}
