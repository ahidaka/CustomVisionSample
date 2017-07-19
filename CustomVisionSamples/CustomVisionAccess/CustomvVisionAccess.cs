using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CustomVisionAccess
{
    public class CustomVisionAccess
    {
        public CustomVisionAccess(string predictionKey)
        {
            this.predictionKey = predictionKey;
        }

        private string predictionKey;
        public string PredictionKey { get; }

        public async Task<string> AnalyzeAsync(string serviceUrlEndpoint, string imageUrl)
        {
            string result = "{}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, serviceUrlEndpoint);
                request.Headers.Add("Prediction-Key", predictionKey);
                var contentAsObj = new
                {
                    Url = imageUrl
                };
                var contentAsJson = JsonConvert.SerializeObject(contentAsObj);
                request.Content = new StringContent(contentAsJson, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        public async Task<string> AnalyzeAsync(string serviceImgEndpoint, Stream imageStream)
        {
            var reader = new BinaryReader(imageStream);
            string result = "";
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, serviceImgEndpoint);
                request.Headers.Add("Prediction-Key", predictionKey);
                request.Content = new ByteArrayContent(reader.ReadBytes((int)imageStream.Length));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octed-stream");
                var response = await client.SendAsync(request);
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        public List<PredictionResult> ParseJSON(string json)
        {
            var results = new List<PredictionResult>();

            dynamic jsonRoot = JsonConvert.DeserializeObject(json);
            dynamic predictions = jsonRoot.SelectToken("Predictions");
            foreach( var p in predictions)
            {
                dynamic tagId = p.SelectToken("TagId");
                dynamic tag = p.SelectToken("Tag");
                dynamic prob = p.SelectToken("Probability");
                results.Add(new PredictionResult()
                {
                    TagId = tagId.Value,
                    Tag = tag.Value,
                    Probability = prob.Value
                });
            }
            return results;
        }
    }
}
