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

namespace RosieAgents.SkillFunctions
{
    public class QdrantFunction
    {
        private const string MemoryCollectionName = "webxr-test";

        private const int MaxTokens = 1024;

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
                    c.AddAzureTextEmbeddingGenerationService("ada", "text-embedding-ada-002", endPoint, apiKey);
                    c.AddAzureTextCompletionService("gpt", model, endPoint, apiKey);
                })
                .WithMemoryStorage(memoryStore)
                .Build();

            Console.WriteLine("== Printing Collections in DB ==");
            var collections = memoryStore.GetCollectionsAsync();
            await foreach (var collection in collections)
            {
                Console.WriteLine(collection);
            }

            //Console.WriteLine("== Adding Memories ==");
            //var repositoryUri = "https://raw.githubusercontent.com/immersive-web/webxr/gh-pages/explainer.md";

            //SmartReader.Reader sr = new SmartReader.Reader(repositoryUri);
            //SmartReader.Article article = sr.GetArticle();

            //var content = StripHtmlTags(article.Content);

            //List<string> lines;
            //List<string> paragraphs;

            //lines = SemanticTextPartitioner.SplitMarkDownLines(content, MaxTokens);
            //paragraphs = SemanticTextPartitioner.SplitMarkdownParagraphs(lines, MaxTokens);

            //for (int i = 0; i < paragraphs.Count; i++)
            //{
            //    await kernel.Memory.SaveInformationAsync(
            //        MemoryCollectionName,
            //        text: $"{paragraphs[i]}",
            //        id: $"webxr_{i}");
            //}


            const string skPrompt = @"
ChatBot can answer questions webxr.
It can give explicit answer or say 'I don't know' if it does not have an answer.

Only use this information:
{{$facts}} 
========================================================
{{$INPUT}}
========================================================
Answer: ";

            var chatFunction = kernel.CreateSemanticFunction(skPrompt, maxTokens: 200, temperature: 0.8);


            var searchResults = kernel.Memory.SearchAsync(MemoryCollectionName, question);

            List<string> facts = new List<string>();

            await foreach (var item in searchResults)
            {
                //Console.WriteLine(item.Metadata.Text + " : " + item.Relevance);
                facts.Add(item.Metadata.Text);
            }

            var context = kernel.CreateNewContext();
            context["facts"] = string.Join('\n', facts);
            context["userInput"] = question;

            var output = await chatFunction.InvokeAsync(context);

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
