namespace HackerNewsAPI.Domain
{
    public class HackerNewsStoryProviderSettings
    {

        /// <summary>
        /// the Url to get the best story ids
        /// </summary>
        public string BestStoriesUrl { get; set; }

        /// <summary>
        /// The URL of the story details 
        /// </summary>
        public string StoryDetailsUrl { get; set; }

        /// <summary>
        /// BestStoryIds Cache Expiration policy in seconds. 
        /// </summary>
        public int BestStoryIdsCacheExpiration { get; set; }


        /// <summary>
        /// Story detail Cache Expiration policy in seconds. 
        /// </summary>
        public int StoryDetailsCacheExpiration { get; set; }
    }
}
