using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Text;
using static System.Net.WebRequestMethods;

namespace RosieAgents.SkillFunctions
{
    public class QdrantFunction
    {
        private const string MemoryCollectionName = "WebXR Spec";

        private const int MaxTokens = 256;

        private readonly ILogger _logger;

        public QdrantFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QdrantFunction>();
        }

        [Function("QdrantFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            string apiKey = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_API_KEY)!;
            string model = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_DEPLOYMENT_NAME)!;
            string endPoint = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_ENDPOINT)!;
            string chatModel = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_CHATMODEL)!;

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string question = queryDictionary["q"];

            int qdrantPort = int.Parse(Environment.GetEnvironmentVariable("QDRANT_PORT"), CultureInfo.InvariantCulture);
            QdrantMemoryStore memoryStore = new QdrantMemoryStore(Environment.GetEnvironmentVariable("QDRANT_ENDPOINT"), qdrantPort, vectorSize: 1536, ConsoleLogger.Log);
      
            IKernel kernel = Kernel.Builder
                .WithLogger(ConsoleLogger.Log)
                .Configure(c =>
                {
                    c.AddAzureTextEmbeddingGenerationService("text-embedding-ada-002", endPoint, apiKey);
                    c.AddAzureChatCompletionService(chatModel, endPoint, apiKey);
                })
                .WithMemoryStorage(memoryStore)
                .Build();

            //Console.WriteLine("== Printing Collections in DB ==");
            //var collections = memoryStore.GetCollectionsAsync();
            //await foreach (var collection in collections)
            //{
            //    Console.WriteLine(collection);
            //}

            //Console.WriteLine("== Adding Memories ==");
            //List<string> repositoryUris = new List<string>()
            //{
            //    "https://raw.githubusercontent.com/immersive-web/webxr/gh-pages/explainer.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/accessibility-considerations-explainer.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/input-explainer.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/privacy-security-explainer.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/spatial-tracking-explainer.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/designdocs/dynamic-viewport-scaling-privacy-security.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/designdocs/navigation.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/designdocs/privacy-design.md",
            //    "https://github.com/immersive-web/webxr/raw/gh-pages/designdocs/session-creation.md"
            //};

            //for (var index = 0; index < repositoryUris.Count; index++)
            //{
            //    var repositoryUri = repositoryUris[index];
            //    Console.WriteLine(repositoryUri);
            //    SmartReader.Reader sr = new SmartReader.Reader(repositoryUri);
            //    SmartReader.Article article = sr.GetArticle();

            //    var content = StripHtmlTags(article.Content);

            //    List<string> lines;
            //    List<string> paragraphs;

            //    lines = TextChunker.SplitMarkDownLines(content, MaxTokens);
            //    paragraphs = TextChunker.SplitMarkdownParagraphs(lines, MaxTokens);

            //    for (int i = 0; i < paragraphs.Count; i++)
            //    {
            //        await kernel.Memory.SaveInformationAsync(
            //            MemoryCollectionName,
            //            text: $"{paragraphs[i]}",
            //            id: $"webxr_{index}_{i}",
            //            additionalMetadata: repositoryUri);
            //    }
            //}

            //return req.CreateResponse(HttpStatusCode.OK);

            const string skPrompt = @"
====
WRITE A BLOGPOST USING THE SAMPLE BELOW. USE THE FOLLOWING RULES:
- USE MARKDOWN FOR FORMATTING
- KEEP SENTENSES SHORT
- DON'T USE HASHTAGS
- DON'T USE LINKS
- DON'T USE EMOJIS
- WRITE IN THE FIRST PERSON
- WRITE IN A CREATIVE, INFORMAL STYLE FOR TECHNICAL PEOPLE
- WRITE IN AN EXPOSITORY STYLE
- WRITE IN A STYLE THAT IS EASY TO UNDERSTAND
- DON'T MAKE UP FACTS
- USE THESE FACTS:
{{$facts}} 
========================================================
{{$INPUT}}";
//ChatBot can answer questions webxr.
//It can give explicit answer or say 'I don't know' if it does not have an answer.

//Only use this information:
//{{$facts}} 
//========================================================
//{{$INPUT}}
//========================================================
//Answer: ";

            var chatFunction = kernel.CreateSemanticFunction(skPrompt, maxTokens: 4000, temperature: 0.8);


            var searchResults = kernel.Memory.SearchAsync(MemoryCollectionName, question, limit:15);

            List<string> facts = new List<string>();

            await foreach (var item in searchResults)
            {
                //Console.WriteLine(item.Metadata.Text + " : " + item.Relevance);
                facts.Add(item.Metadata.Text);
            }

            var context = kernel.CreateNewContext();
            context["facts"] = string.Join('\n', facts);
            context["userInput"] = question;

            var output = await kernel.RunAsync(context.Variables, chatFunction);

            var data = req.CreateResponse(HttpStatusCode.OK);
            await data.WriteStringAsync(output.Result);

            return data;
        }

        private static string StripHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
