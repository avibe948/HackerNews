namespace HackerNewsAPI.Tests
{
    internal class NonDisposingJsonStringContent : StringContent
    {
        internal NonDisposingJsonStringContent(string content) : base(content, System.Text.Encoding.UTF8, "application/json")
        {
        }

        protected override void Dispose(bool disposing)
        {
            // Do not call the base Dispose method to avoid disposing the content
        }
    }
}
