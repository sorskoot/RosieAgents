using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Azure;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;

namespace RosieAgents.CodeSkills
{
    internal class VisionSkills
    {
        [SKFunction("Analyze online image")]
        [SKFunctionContextParameter(Name = "url", Description = "URL of the image to analyze")]
        public string CaptionImage(SKContext context)
        {
            var imageUrl = context["url"];
            //return await MakeRequest(imageUrl);
            var result = Analyze(imageUrl, ImageAnalysisFeature.Caption);
                                           //| ImageAnalysisFeature.Text
                                           //| ImageAnalysisFeature.Objects
                                           //| ImageAnalysisFeature.DenseCaptions
                                           //| ImageAnalysisFeature.CropSuggestions
                                           //| ImageAnalysisFeature.People
                                           //| ImageAnalysisFeature.Tags);

            return result.Caption.Content;
        }


        [SKFunction("Extract Text from online image")]
        [SKFunctionContextParameter(Name = "url", Description = "URL of the image to analyze")]
        public string TextFromImage(SKContext context)
        {
            var imageUrl = context["url"];
            //return await MakeRequest(imageUrl);
            var result = Analyze(imageUrl, ImageAnalysisFeature.Text);
            //| ImageAnalysisFeature.Text
            //| ImageAnalysisFeature.Objects
            //| ImageAnalysisFeature.DenseCaptions
            //| ImageAnalysisFeature.CropSuggestions
            //| ImageAnalysisFeature.People
            //| ImageAnalysisFeature.Tags);

            return result.Text.Lines.Last().Content;
        }

        private ImageAnalysisResult Analyze(string imageUrl, ImageAnalysisFeature imageAnalysisFeature)
        {
            VisionServiceOptions serviceOptions = new VisionServiceOptions(
                Environment.GetEnvironmentVariable("VISION_ENDPOINT"),
                new AzureKeyCredential(Environment.GetEnvironmentVariable("VISION_KEY")));

            VisionSource? imageSource = VisionSource.FromUrl(new Uri(imageUrl));

            ImageAnalysisOptions analysisOptions = new ImageAnalysisOptions()
            {
                Features = imageAnalysisFeature,
                Language = "en",
                GenderNeutralCaption = true
            };

            using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

            var result = analyzer.Analyze();

            return result;

            //if (result.Reason == ImageAnalysisResultReason.Analyzed)
            //{
            //    if (result.Caption != null)
            //    {
            //        Console.WriteLine(" Caption:");
            //        Console.WriteLine($"   \"{result.Caption.Content}\", Confidence {result.Caption.Confidence:0.0000}");
            //    }

            //    if (result.Text != null)
            //    {
            //        Console.WriteLine($" Text:");
            //        foreach (var line in result.Text.Lines)
            //        {
            //            string pointsToString = "{" + string.Join(',', line.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
            //            Console.WriteLine($"   Line: '{line.Content}', Bounding polygon {pointsToString}");

            //            foreach (var word in line.Words)
            //            {
            //                pointsToString = "{" + string.Join(',', word.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
            //                Console.WriteLine($"     Word: '{word.Content}', Bounding polygon {pointsToString}, Confidence {word.Confidence:0.0000}");
            //            }
            //        }
            //    }
            //}
            //else if (result.Reason == ImageAnalysisResultReason.Error)
            //{
            //    var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
            //    Console.WriteLine(" Analysis failed.");
            //    Console.WriteLine($"   Error reason : {errorDetails.Reason}");
            //    Console.WriteLine($"   Error code : {errorDetails.ErrorCode}");
            //    Console.WriteLine($"   Error message: {errorDetails.Message}");
            //}

            //return result.Caption.Content;
        }

    }


}
