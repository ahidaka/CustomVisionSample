using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleAppNETCore
{
    class Program
 {
        private string customImageUrlEndpoint = "<< Custom Vision Service URL>>/url";
        private string customImageImgEndpoint = "<< Custom Vision Service URL>>/image";
        private string predictionKey = "<< Prediction-Key >>";

        static void Main(string[] args)
        {
            Console.WriteLine("Args - " + args.Length);
            if (args.Length <= 0)
            {
                Console.WriteLine("dotnet run url|filepath");
                return;
            }
            var program = new Program();
            var predictions = new List<CustomVisionAccess.PredictionResult>();
            foreach (var a in args)
            {
               var task =   program.Analyze(a);
                task.Wait();
                predictions.AddRange(task.Result);
            }

            Console.WriteLine("All predictions:");
            foreach(var p in predictions)
            {
                Console.WriteLine("Tag:{0},Probability:{1} : {2}", p.Tag, p.Probability, p.Target);
            }
        }

        async Task< List<CustomVisionAccess.PredictionResult>> Analyze(string target)
        {
            string result = "";
            CustomVisionAccess.CustomVisionAccess access = new CustomVisionAccess.CustomVisionAccess(predictionKey);
            if (System.IO.File.Exists(target))
            {
                Console.WriteLine("Analyze " + target + " as Image File...");
                using (var fileStream = new System.IO.FileStream(target, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    result = await access.AnalyzeAsync(customImageImgEndpoint, fileStream);
                }
            }
            else if (Uri.IsWellFormedUriString(target, UriKind.Absolute))
            {
                Console.WriteLine("Analyze " + target + " as Image URL...");
                result = await access.AnalyzeAsync(customImageUrlEndpoint, target);
            }

            Console.WriteLine("Result:");
            Console.WriteLine(result);
            var results = access.ParseJSON(result);
            foreach(var p in results)
            {
                p.Target = target;
            }
            return results;
        }
    }
}