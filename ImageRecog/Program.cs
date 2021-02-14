using ImageRecog.Classes;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ImageRecog
{
    static class Program
    {
        // Add your Computer Vision subscription key and endpoint to your environment variables.
        static string subscriptionKey = "";

        static string endpoint = "";

        // the Batch Read method endpoint
        static string uriBase = endpoint + "/vision/v3.1/read/analyze";

        // Add a local image with text here (png or jpg is OK)
        static string imageFilePath = @"C:\Users\Tom\Desktop\pdf.pdf";

        static List<string> _tweets = new List<string>();


        static async Task Main(string[] args)
        {
            // Call the REST API method.
            Console.WriteLine("\nExtracting text...\n");
            await ReadText(imageFilePath);

            while (true) // Loop indefinitely
            {
                Console.WriteLine("Enter search term:"); // Prompt
                string term = Console.ReadLine(); // Get string from user
                if (term == "exit") // Check string
                {
                    break;
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------");
                var sw = new Stopwatch();
                sw.Start();
                var results = _tweets.Where(a => a.ToLower().Contains(term.ToLower()));
                sw.Stop();
                Console.WriteLine("FOUND " + results.Count() + " TWEETS in " + (sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000)) + " MICROSECONDS");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine();
                foreach (var tweet in results)
                    Console.WriteLine(tweet);
            }
        }

        /// <summary>
        /// Gets the text from the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with text.</param>
        static async Task ReadText(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string url = uriBase;

                HttpResponseMessage response;

                // Two REST API methods are required to extract text.
                // One method to submit the image for processing, the other method
                // to retrieve the text found in the image.

                // operationLocation stores the URI of the second REST API method,
                // returned by the first REST API method.
                string operationLocation;

                // Reads the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Adds the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // The first REST API method, Batch Read, starts
                    // the async process to analyze the written text in the image.
                    response = await client.PostAsync(url, content);
                }

                // The response header for the Batch Read method contains the URI
                // of the second method, Read Operation Result, which
                // returns the results of the process in the response body.
                // The Batch Read operation does not return anything in the response body.
                if (response.IsSuccessStatusCode)
                    operationLocation =
                        response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    // Display the JSON error data.
                    string errorString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n\nResponse:\n{0}\n",
                        JToken.Parse(errorString).ToString());
                    return;
                }

                // If the first REST API method completes successfully, the second 
                // REST API method retrieves the text written in the image.
                //
                // Note: The response may not be immediately available. Text
                // recognition is an asynchronous operation that can take a variable
                // amount of time depending on the length of the text.
                // You may need to wait or retry this operation.
                //
                // This example checks once per second for ten seconds.
                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1);

                if (i == 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1)
                {
                    Console.WriteLine("\nTimeout error.\n");
                    return;
                }

                var tempTweets = JsonConvert.DeserializeObject<Root>(contentString).analyzeResult.readResults.SelectMany(a => a.lines)
                    .Where(a => a.text.Contains(" ")).Select(q => q.text).Distinct().ToList();

                Dictionary<int, string> filtered = new Dictionary<int, string>();
                int i2 = 0;
                foreach (var t in tempTweets)
                {
                    filtered.Add(i2, t);
                    i2++;
                }

                Dictionary<int, string> trimmed = new Dictionary<int, string>();
                int i3 = 0;

                foreach (var t in tempTweets)
                {
                    if (t.Length >= 20)
                    {
                        trimmed.Add(i3, t.Substring(0, 20));
                    }
                    i3++;
                }

                var distincted = trimmed.DistinctBy(a => a.Value);

                _tweets = filtered.Where(a => distincted.Select(q => q.Key).Contains(a.Key)).Select(q => q.Value).ToList();
                 





            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
