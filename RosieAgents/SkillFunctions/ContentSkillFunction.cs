using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace RosieAgents.SkillFunctions
{
    public class ContentSkillFunction//:SkillFunctionBase
    {
        [Function(nameof(ContentCreation))]
        public async Task<HttpResponseData> ContentCreation([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            //InitKernel();
            IKernel kernel = new KernelBuilder().WithLogger(ConsoleLogger.Log).Build();

            string apiKey = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_API_KEY)!;
            string endPoint = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_ENDPOINT)!;
            string chatModel = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_CHATMODEL)!;

            // Note: we use Chat Completion and GPT 3.5 Turbo
            kernel.Config.AddAzureChatCompletionService(chatModel, endPoint, apiKey);
            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedInput = queryDictionary["input"] ?? string.Empty;
            string requestedSkill = queryDictionary["skill"] ?? string.Empty;
            string requestedContentType = (queryDictionary["contenttype"] ?? "BLOGPOST").ToUpper();

            if (string.IsNullOrWhiteSpace(requestedSkill) || string.IsNullOrWhiteSpace(requestedInput))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }


            string skillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "skills");
            IDictionary<string, ISKFunction> skill = kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "Content");

            if (!skill.ContainsKey(requestedSkill))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var context = kernel.CreateNewContext();
            context["INPUT"] = requestedInput;
            context["CONTENTTYPE"] = requestedContentType;

            var result = await kernel.RunAsync(context.Variables, skill[requestedSkill]);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(result.Result);

            return response;
        }

       
    }
}
