
namespace HackerNewsAPI.Domain
{
    public enum StoryType {Story}
    public record Story
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



}
