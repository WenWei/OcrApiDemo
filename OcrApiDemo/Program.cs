using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OcrApiDemo
{
    class Program
    {
        private static string url = "http://localhost:5000/api/ocr/UploadAjax";
        private static string filename = "00000.png";

        static void Main(string[] args)
        {
            var webRequestResult = WebRequestUpload(url, filename);
            Console.WriteLine($"WebRequest Result: {webRequestResult}");

            var result = Upload(url, filename).Result;
            Console.WriteLine($"HttpClient Result: {result}");

            Console.ReadLine();
        }

        public static string WebRequestUpload(string url, string filepath)
        {
            string returnResponseText = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                //byte[] bytes = wc.DownloadData(filepath);
                FileInfo fi = new FileInfo(filepath);
                FileStream fs = File.OpenRead(fi.FullName);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes);

                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                postParameters.Add("fileToUpload", new FormUpload.FileParameter(bytes, Path.GetFileName(filepath), "image/png"));
                string userAgent = "Someone";
                HttpWebResponse webResponse = FormUpload.MultipartFormPost(url, userAgent, postParameters, "demo", "demo");

                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                returnResponseText = responseReader.ReadToEnd();
                webResponse.Close();
            }
            catch (Exception exp) { }

            return returnResponseText;
        }


        /// <summary>
        /// HttpClient in Net4.5 with multipart/form-data
        /// </summary>
        public static async Task<string> Upload(string url, string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            FileStream fs = File.OpenRead(fi.FullName);
            byte[] image = new byte[fs.Length];
            fs.Read(image);

            using (var client = new HttpClient())
            {
                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(image)), fi.Name, fi.Name);

                    using (var message = await client.PostAsync(url, content))
                    {
                        var input = await message.Content.ReadAsStringAsync();
                        return input;
                    }
                }
            }
        }
    }
}
