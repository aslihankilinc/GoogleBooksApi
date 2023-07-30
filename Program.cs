using Google.Apis.Services;
using Google.Apis.Books.v1;

namespace GoogleBooksApi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = new BooksService(new BaseClientService.Initializer
            {
                ApplicationName = "Google",
                ApiKey = "AIzaSyBxjx5JZSOxQaC6wez-9bJ6I3Alms0QFXY",
            });
        }
    }
}
