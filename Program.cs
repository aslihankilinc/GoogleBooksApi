using Google.Apis.Services;
using Google.Apis.Books.v1;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Books.v1.Data;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace GoogleBooksApi
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Books API Sample: List MyLibrary");
            Console.WriteLine("================================");

            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { BooksService.Scope.Books },
                    "user", CancellationToken.None, new FileDataStore("Books.ListMyLibrary"));
            }

            // Create the service.
            var service = new BooksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BooksApi",
            });

            // List library.
            await ListLibrary(service);

            // Revoke the credential.
            Console.WriteLine("\n!!!REVOKE ACCESS TOKEN!!!\n");
            await credential.RevokeTokenAsync(CancellationToken.None);

            // Request should fail now - invalid grant.
            try
            {
                await ListLibrary(service);
            }
            catch (TokenResponseException ex)
            {
                Console.WriteLine(ex.Error);
            }

            // Reauthorize the user. A browser should be opened, and the user should enter his or her credential again.
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credential, CancellationToken.None);

            // The request should succeed now.
            await ListLibrary(service);
        }

        private async Task ListLibrary(BooksService service)
        {
            Console.WriteLine("\n\n\nListing Bookshelves... (Execute ASYNC)");
            Console.WriteLine("======================================");

            // Execute async.
            var bookselve = await service.Mylibrary.Bookshelves.List().ExecuteAsync();

            // On success display my library's volumes.
            await DisplayVolumes(service, bookselve);
        }

        private async Task DisplayVolumes(BooksService service, Bookshelves bookshelves)
        {
            if (bookshelves.Items == null)
            {
                Console.WriteLine("No bookshelves found!");
                return;
            }

            foreach (Bookshelf item in bookshelves.Items)
            {
                Console.WriteLine(item.Title + "\t" +
                    (item.VolumeCount.HasValue ? item.VolumeCount + " volumes" : ""));

                // List all volumes in this bookshelf.
                if (item.VolumeCount > 0)
                {
                    Console.WriteLine("Query volumes... (Execute ASYNC)");
                    Console.WriteLine("--------------------------------");
                    var request = service.Mylibrary.Bookshelves.Volumes.List(item.Id.ToString());
                    Volumes inBookshelf = await request.ExecuteAsync();
                    if (inBookshelf.Items == null)
                    {
                        continue;
                    }
                    foreach (Volume volume in inBookshelf.Items)
                    {
                        Console.WriteLine("-- " + volume.VolumeInfo.Title + "\t" + volume.VolumeInfo.Description ??
                            "no description");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}