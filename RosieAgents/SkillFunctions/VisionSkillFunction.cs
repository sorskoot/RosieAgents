using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;
using RosieAgents.CodeSkills;
using System.Collections.Specialized;
using System.Web;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;

namespace RosieAgents.SkillFunctions
{
    public class VisionSkillFunction:SkillFunctionBase
    {
        public VisionSkillFunction(ILoggerFactory loggerFactory):base(loggerFactory) { }
        

        [Function(nameof(ImageToTweet))]
        public async Task<HttpResponseData> ImageToTweet([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            IDictionary<string, ISKFunction> visionSkills = Kernel.ImportSkill(new VisionSkills());
            IDictionary<string, ISKFunction> summarizeSkill = GetSemanticsSkill("SocialMediaSkill");

            var result = await Run(req, visionSkills["CaptionImage"], summarizeSkill["Tweet"]);

            return await createResponse(req, result);
        }

        [Function(nameof(ImageExplainText))]
        public async Task<HttpResponseData> ImageExplainText([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            var model = "code-davinci-002";
            InitKernel();

            Kernel.UseMemory(new VolatileMemoryStore());

            IDictionary<string, ISKFunction> visionSkills = Kernel.ImportSkill(new VisionSkills());
            IDictionary<string, ISKFunction> CodeSkill = GetSemanticsSkill("CodeSkills");

            var result = await Run(req, 
                visionSkills["TextFromImage"], 
                CodeSkill["ExplainRegEx"]);

            return await createResponse(req, result);
        }

        private static async Task<HttpResponseData> createResponse(HttpRequestData req, string result)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(result);
            return response;
        }


        private async Task<string> Run(HttpRequestData req, params ISKFunction[] functions)
        {
            

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                throw new Exception("No query string");
                
            }

            string requestedUrl = queryDictionary["url"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedUrl))
            {
                throw new Exception("No url");
            }
            
            var variables = new ContextVariables();
            variables.Set("url", requestedUrl);


            SKContext skContext = await Kernel.RunAsync(variables, functions);

            return skContext.Result;
        }
    }
}
