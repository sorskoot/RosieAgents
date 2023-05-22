using System.Collections.Specialized;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;
using System.Web;
using Microsoft.SemanticKernel.SkillDefinition;

namespace RosieAgents.SkillFunctions
{
    public class IdeasSkillFunction : SkillFunctionBase
    {
        public IdeasSkillFunction(ILoggerFactory loggerFactory) : base(loggerFactory) { }

        [Function("IdeasSkill")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedSkill = queryDictionary["skill"] ?? string.Empty;
            string requestedInput = queryDictionary["input"] ?? string.Empty;
            string requestedNumber = queryDictionary["number"] ?? "10";

            if (string.IsNullOrWhiteSpace(requestedSkill) || string.IsNullOrWhiteSpace(requestedInput))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> skill = GetSemanticsSkill("IdeasSkill");

            if (!skill.ContainsKey(requestedSkill))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var variables = new ContextVariables
            {
                ["numIdeas"] = requestedNumber,
                ["input"] = requestedInput
            };

            var result = await Kernel.RunAsync(variables, skill[requestedSkill]);


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(result.Result);

            return response;
        }
    }
}
