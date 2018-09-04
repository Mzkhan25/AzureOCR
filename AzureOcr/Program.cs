#region
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
#endregion

namespace AzureOcr
{
    internal static class Program
    {
        private const string subscriptionKey = "Your Key Here";
        private const string uriBase = "https://westus.api.cognitive.microsoft.com/vision/v2.0/ocr";
        private static void Main ()
        {
            // Get the path and filename to process from the user.
            Console.WriteLine ("Optical Character Recognition:");
            Console.Write ("Enter the path to an image with text you wish to read: ");
            string imageFilePath = Console.ReadLine ();
            if (File.Exists (imageFilePath))
            {
                // Make the REST API call.
                Console.WriteLine ("\nWait a moment for the results to appear.\n");
                MakeOCRRequest (imageFilePath).Wait ();
            }
            else
            {
                Console.WriteLine ("\nInvalid file path");
            }

            Console.WriteLine ("\nPress Enter to exit...");
            Console.ReadLine ();
        }
        /// <summary>
        ///     Gets the text visible in the specified image file by using
        ///     the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        private static async Task MakeOCRRequest (string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient ();

                // Request headers.
                client.DefaultRequestHeaders.Add ("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters.
                string requestParameters = "language=unk&detectOrientation=true";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;
                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray (imageFilePath);
                using (ByteArrayContent content = new ByteArrayContent (byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue ("application/octet-stream");

                    // Make the REST API call.
                    response = await client.PostAsync (uri, content);
                }

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync ();
                OCRModel ocrModel = JToken.Parse (contentString).ToObject<OCRModel> ();
                List<string> lines = new List<string> ();
                string line = "";
                foreach (var _region in ocrModel.regions)
                {
                    foreach (var _line in _region.lines)
                    {
                        foreach (var _word in _line.words) line = line + _word.text + " ";
                        lines.Add (line);
                        line = "";
                    }
                }

                foreach (var _line in lines) Console.WriteLine (_line);
            }
            catch (Exception e)
            {
                Console.WriteLine ("\n" + e.Message);
            }
        }
        /// <summary>
        ///     Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        private static byte[] GetImageAsByteArray (string imageFilePath)
        {
            using (FileStream fileStream = new FileStream (imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader (fileStream);
                return binaryReader.ReadBytes ((int) fileStream.Length);
            }
        }
    }
}