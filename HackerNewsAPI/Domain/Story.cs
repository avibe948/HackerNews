
namespace HackerNewsAPI.Domain
{
    public enum StoryType {Story}
    public record Story(string By, int Descendants,
        int Id, int[] Kids, int Score, long Time, 
        string Title, string type, string Url);
    

}
