using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;

namespace RosieAgents.SkillFunctions
{
    public class ContentSkillFunction:SkillFunctionBase
    {
        public ContentSkillFunction(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        [Function(nameof(ContentCreation))]
        public async Task<HttpResponseData> ContentCreation([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedInput = queryDictionary["input"] ?? string.Empty;
            string requestedSkill = queryDictionary["skill"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedSkill) || string.IsNullOrWhiteSpace(requestedInput))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> skill = GetSemanticsSkill("Content");

            if (!skill.ContainsKey(requestedSkill))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var result = await Kernel.RunAsync(requestedInput, skill[requestedSkill]);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(result.Result);

            return response;
        }
    }
}
