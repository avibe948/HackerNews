namespace HackerNewsAPI.Domain
{
    public static class CacheKeys
    {
        public static readonly string BestStoriesCacheKey = "BestStories";
        public static readonly string StoryDetailsCacheKey = "Story_{id}";

        public static string GetStoryDetailsCacheKey(string storyId)
        {
           return StoryDetailsCacheKey.Replace("{id}", storyId.ToString());
        }

    }
}
