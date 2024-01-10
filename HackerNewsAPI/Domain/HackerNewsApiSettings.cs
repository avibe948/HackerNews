namespace HackerNewsAPI.Domain
{
    // Create a class HackerNewsApiSettings.cs
    public class HackerNewsApiSettings
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

        /// <summary>
        /// If set to true , the Stories Controller will not assume that the stories returned from the url call to Hacker news are sorted 
        /// (Looking at the reponse of the Hacker news BestStoriesUrl the returned json appears to be  sorted already by the highest score so there may be no need to re-sort , 
        /// hence this flag allows to avoid double sorting)
        /// </summary>
        public bool ForceReSortBestStoryIds { get; set; }


    }

}
