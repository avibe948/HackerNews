namespace HackerNewsAPI.Domain
{
    // Create a class HackerNewsApiSettings.cs
    public class HackerNewsApiSettings
    {
        /// <summary>
        /// If set to true , the Stories Controller will not assume that the stories returned from the url call to Hacker news are sorted 
        /// (Looking at the reponse of the Hacker news BestStoriesUrl the returned json appears to be  sorted already by the highest score so there may be no need to re-sort , 
        /// hence this flag allows to avoid double sorting)
        /// </summary>
        public bool ForceReSortBestStoryIds { get; set; }
    }
}
